using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.GC;
using VM.VMObjects;

namespace VM {
	class MarkSweepCompactMemoryManager : MemoryManagerBase {
		Word[] memory;
#if DEBUG
		[ThreadStatic]
		static bool verifyAddress = true;
#endif
		int start, size, allocated;
		public override int SizeInWords { get { return size; } }
		public override int FreeSizeInWords { get { return size - allocated - 1; } }
		public override int AllocatedSizeInWords { get { return allocated; } }
		int firstHole = 0;

		public MarkSweepCompactMemoryManager( Word[] memory, int start, int size ) {
			this.memory = memory;
			this.start = start;
			this.size = size;
			allocated = 0;
			firstHole = start;

			memory[firstHole] = size;
			memory[firstHole + size - 1] = int.MaxValue;
		}

		internal override T Allocate<T>( int size ) {
			lock (this) {
				var haveCollected = false;
				var haveCompacted = false;
				if (FreeSizeInWords < size + 2) {
					Collect();
					haveCollected = true;
				}
				if (FreeSizeInWords < size + 2)
					return new T().New( 0 );

			retry:
				var previousHole = firstHole;
				var hole = firstHole;
				int holeSize = hole < memory.Length ? (int) memory[hole] : 0;
				if (hole < memory.Length)
					Sekhmet.Assert.IsTrue( hole + holeSize <= memory.Length );
				while (holeSize < size + 2) {
					if (hole >= SizeInWords) {
						if (!haveCollected) {
							Collect();
							haveCollected = true;
						} else if (!haveCompacted) {
							//Compact();
							haveCompacted = true;
						} else
							return new T().New( 0 );
						hole = firstHole;
						goto retry;
					}
					previousHole = hole;
					hole = memory[hole + holeSize - 1];
					if (hole < memory.Length)
						holeSize = memory[hole];
				}
				Sekhmet.Assert.IsTrue( hole + holeSize <= memory.Length );

				var pos = hole + 2;
				allocated += size + 2;

				if (hole == firstHole && holeSize - (size + 2) < 3)
					firstHole = memory[hole + memory[hole] - 1];
				else if (hole == firstHole) {
					firstHole = pos + size;
					memory[pos + size] = holeSize - (size + 2);
				} else if (holeSize - (size + 2) < 3)
					memory[previousHole + memory[previousHole] - 1] = memory[hole + holeSize - 1];
				else {
					hole = pos + size;
					memory[previousHole + memory[previousHole] - 1] = hole;
					memory[hole] = holeSize - (size + 2);
				}

				var obj = new T().New( pos );

				DisableAddressVerification();
				for (var i = 0; i < size; i++)
					obj[i] = 0;
				memory[pos + VMObjects.ObjectBase.CLASS_POINTER_OFFSET] = obj.VMClass.Start;
				if (holeSize - (size + 2) < 3)
					size += holeSize - (size + 2);
				memory[pos + VMObjects.ObjectBase.SIZE_OFFSET] = size << VMObjects.ObjectBase.SIZE_RSHIFT;
				EnableAddressVerification();

				AssertHeap();
				return obj;
			}
		}

		protected override void RelocateHere( int obj, int size ) {
			throw new NotImplementedException();
		}


		void Collect() {
			var interps = VirtualMachine.GetInterpreters();
			interps.ForEach( i => i.BeginPause() );
			interps.ForEach( i => i.EndPause() );
			MemoryManagerBase.MakeWeakHandles = true;

			var roots = new Dictionary<int, Action<int, int>>();
			MemoryManagerBase.Handles.ForEach( h => roots.Add( h, ( from, to ) => MemoryManagerBase.MoveHandles( from, to ) ) );

			foreach (var stack in interps.Select( i => i.Stack )) {
				for (var i = 0; i < stack.StackPointer; i++) {
					if (!stack[i].IsReference || stack[i].IsNull)
						continue;
					var adr = stack[i].Value;
					var cls = stack[i].Type;
					Action<int, int> act;
					if (roots.TryGetValue( adr, out act )) {
						roots[adr] = ( from, to ) => {
							act( from, to );
							stack[i] = UValue.Ref( cls, to );
						};
					} else
						roots.Add( adr, ( from, to ) => stack[i] = UValue.Ref( cls, to ) );
				}
			}

			DumpMemory( @"c:\users\sekhmet\temp\premark.txt" );
			Mark( roots.Keys );
			DumpMemory( @"c:\users\sekhmet\temp\presweep.txt" );
			Sweep();
			DumpMemory( @"c:\users\sekhmet\temp\postsweep.txt" );

			MemoryManagerBase.MakeWeakHandles = false;
			interps.ForEach( i => i.Resume() );
		}

		void Mark( IEnumerable<int> roots ) {
			var grey = new Queue<int>();
			roots.ForEach( a => {
				a.SetBlack();
				a.GetReferences().ForEach( r => {
					r.SetGrey();
					grey.Enqueue( r );
				} );
			} );

			while (grey.Count > 0) {
				var elem = grey.Dequeue();
				if (elem.IsBlack())
					continue;

				elem.SetBlack();
				elem.GetReferences().Where( r => !r.IsBlack() ).ForEach( r => grey.Enqueue( r ) );
			}
		}

		void Sweep() {
			var allocatedBefore = AllocatedSizeInWords;
			var previousHole = -1;
			var hole = firstHole;
			var obj = start + 2;
			while (true) {
				if (obj > hole) {
					previousHole = hole;
					obj = hole + memory[hole] + 2;
					hole = memory[hole + memory[hole] - 1];
				}
				if (obj >= size)
					break;

				int objSize = memory[obj + VMObjects.ObjectBase.SIZE_OFFSET] >> VMObjects.ObjectBase.SIZE_RSHIFT;
				if (obj.IsBlack()) {
					obj.SetWhite();
					obj += objSize + 2;
					continue;
				}

				allocated -= objSize + 2;
				if (previousHole != -1 && previousHole + memory[previousHole] + 2 == obj && hole == obj + objSize) // obj right between previousHole and hole
					memory[previousHole] = memory[previousHole] + 2 + objSize + memory[hole];
				else if (previousHole != -1 && previousHole + memory[previousHole] + 2 == obj) { // obj right after previousHole
					memory[previousHole] = memory[previousHole] + objSize + 2;
					memory[obj + objSize - 1] = hole;
				} else if (hole == obj + objSize) { // obj right before hold
					if (previousHole != -1)
						memory[previousHole + memory[previousHole] - 1] = obj - 2;
					memory[obj - 2] = objSize + 2 + memory[hole];
				} else { // obj between two objects
					if (previousHole != -1)
						memory[previousHole + memory[previousHole] - 1] = obj - 2;
					memory[obj - 2] = objSize + 2;
					memory[obj + objSize - 1] = hole;
					previousHole = obj - 2;
				}
				if (firstHole > obj - 2)
					firstHole = obj - 2;
				obj += objSize + 2;
			}
		}

		void Compact() {
			throw new NotImplementedException();
		}

		internal override void NewMemory( Word[] memory, int start, int size ) {
			this.start = start;
			this.size = size;
			this.memory = memory;
			var hole = firstHole;
			while (memory[hole + memory[hole] - 1] < memory.Length)
				hole = memory[hole + memory[hole] - 1];

			memory[hole] = memory.Length - hole;
			memory[hole + memory[hole] - 1] = int.MaxValue;
		}

		//[System.Diagnostics.Conditional( "DEBUG" )]
		void DumpMemory( string file ) {
			//#if DEBUG
			using (var writer = new System.IO.StreamWriter( file )) {
				var hole = firstHole;
				var obj = 3;

				Action<int, object> write = ( adr, str ) => {
					writer.WriteLine( adr.ToString().PadLeft( memory.Length.ToString().Length )
						+ ":" + memory[adr].ToString().PadLeft( uint.MaxValue.ToString().Length )
						+ ": " + str );
				};

				while (true) {
					if (hole <= obj) {
						try {
							var holeSize = memory[hole];
							write( hole, "--- hole" );
							write( hole + 1, "size: " + holeSize );
							for (int i = hole + 2; i < hole + holeSize - 1; i++)
								write( i, "" );
							write( (hole + holeSize - 1), "--- end hole" );
							obj = hole + holeSize + 2;
							hole = memory[hole + holeSize - 1];
						} catch (Exception ex) {
							writer.WriteLine( hole + ": " + ex );
						}
					} else {
						var objSize = memory[obj + ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT;
						write( (obj - 2), "--- obj (size: " + objSize + ", gc: " + (memory[obj + ObjectBase.SIZE_OFFSET] & 3) + ")" );
						try {
							int cls = memory[obj + ObjectBase.CLASS_POINTER_OFFSET];
							write( (obj - 1), "class " + (cls > 0 ? ((VMObjects.ObjectBase) obj).ToString() : cls.ToString()) );
							for (int i = obj; i < obj + objSize - 1; i++)
								write( i, "" );
							write( (obj + objSize - 1), "--- end obj" );
						} catch (Exception ex) {
							writer.WriteLine( obj + ": " + ex );
						}
						obj += objSize + 2;
					}
					if (obj >= memory.Length && hole >= memory.Length)
						break;
				}
			}
			//#endif
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		internal void AssertValidAddress( bool gettting, int adr ) {
#if DEBUG
			lock (this) {
				var hole = firstHole;
				while (hole < memory.Length) {
					var holeSize = memory[hole];
					Sekhmet.Assert.IsFalse( hole <= adr && adr < hole + holeSize, "Address " + adr + " points into hole starting at " + hole + "." );

					hole = memory[hole + holeSize - 1];
				}

				if (!verifyAddress || gettting)
					return;

				var closestHole = firstHole;
				hole = firstHole;
				while (hole < adr && memory[hole + memory[hole] - 1] < adr)
					hole = memory[hole + memory[hole] - 1];

				var obj = hole < adr ? hole + memory[hole] + 2 : 3;
				while (obj + (memory[obj + ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT) < adr)
					obj += (memory[obj + ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT) + 2;
				Sekhmet.Assert.IsTrue( adr >= obj, "Address " + adr + " in object " + obj + " is not valid." );
			}
#endif
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		internal static void DisableAddressVerification() {
#if DEBUG
			verifyAddress = false;
#endif
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		internal static void EnableAddressVerification() {
#if DEBUG
			verifyAddress = true;
#endif
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		private void AssertHeap() {
#if DEBUG
			lock (this) {
				var holeMap = new SortedList<int, int>();
				var hole = firstHole;
				while (hole < memory.Length) {
					holeMap.Add( hole, hole );
					var holeSize = memory[hole];
					Sekhmet.Assert.IsTrue( holeSize < memory.Length );
					var nextHole = memory[hole + holeSize - 1];
					Sekhmet.Assert.IsTrue( nextHole > hole );
					hole = nextHole;
				}
				holeMap.Add( hole, hole );

				var holeIdx = 0;
				hole = holeMap.Values[holeIdx];
				var obj = 3;

				while (true) {
					if (hole > memory.Length && obj > memory.Length)
						break;
					if (hole < obj) {
						var holeEnd = memory[hole] + hole - 1;
						if (obj < memory.Length)
							Sekhmet.Assert.AreEqual( holeEnd + 1, obj - 2 );
						hole = holeMap.Values[++holeIdx];
					} else if (obj < hole) {
						var objSize = memory[obj + ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT;
						Sekhmet.Assert.IsTrue( objSize < memory.Length );
						Sekhmet.Assert.IsTrue( obj + objSize <= memory.Length );
						var objEnd = obj + (memory[obj + ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT) - 1;

						int cls = memory[obj + ObjectBase.CLASS_POINTER_OFFSET];
						int clsCls;
						if (cls > 0) {
							clsCls = KnownClasses.Resolve( memory[cls + ObjectBase.CLASS_POINTER_OFFSET] ).Start;
							Sekhmet.Assert.AreEqual( clsCls, KnownClasses.System_Reflection_Class.Start );
							Sekhmet.Assert.IsTrue( clsCls < 6521 );
						}

						if (objEnd + 1 == hole)
							obj = hole + memory[hole] + 2;
						else
							obj += objSize + 2;
					}
				}
			}
#endif
		}
	}
}

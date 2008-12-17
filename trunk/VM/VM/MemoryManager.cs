using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet;
using VM.VMObjects;

namespace VM {
	class MemoryManager : MemoryManagerBase {
		int initialHeapSize, maxHeapSize, heapGrowFactor;
		public override int SizeInWords { get { return memory.Length; } }
		public override int FreeSizeInWords { get { return gens.Sum( m => m.FreeSizeInWords ); } }
		public override int AllocatedSizeInWords { get { return NurserySize + gens.Sum( m => m.AllocatedSizeInWords ); } }
		public readonly int NurserySize;

		internal Word this[int index] {
			get {
#if DEBUG
				if (gens[0] is MarkSweepCompactMemoryManager)
					((MarkSweepCompactMemoryManager) gens[0]).AssertValidAddress( true, index );
#endif
				return memory[index];
			}
			set {
#if DEBUG
				if (gens[0] is MarkSweepCompactMemoryManager)
					((MarkSweepCompactMemoryManager) gens[0]).AssertValidAddress( false, index );
#endif
				memory[index] = value;
			}
		}

		Word[] memory;
		MemoryManagerBase[] gens;

		public MemoryManager( bool useGC, int initialHeapSize, int maxHeapSize, int heapGrowFactor ) {
			this.initialHeapSize = initialHeapSize;
			this.maxHeapSize = maxHeapSize;
			this.heapGrowFactor = heapGrowFactor;
			memory = new Word[initialHeapSize];
			if (useGC)
				gens = new[] { new MarkSweepCompactMemoryManager( memory, 1, this.initialHeapSize - 1 ) };
			else
				gens = new[] { new NoncollectingMemoryManager( memory, 1, this.initialHeapSize - 1 ) };
		}

		internal override T Allocate<T>( int size ) {
		retry:
			var res = gens[0].Allocate<T>( size );
			if (res.IsNull()) {
				if (memory.Length < maxHeapSize) {
					Expand();
					goto retry;
				} else {
					if (VirtualMachine.OutOfMemoryException == null)
						throw new OutOfMemoryException( false );
					throw VirtualMachine.OutOfMemoryException;
				}
			}
			return res;
		}

		protected override void RelocateHere( int obj, int size ) {
			throw new NotSupportedException();
		}

		internal MemoryManagerBase CreateNusery() {
			return null;
		}

		internal void Expand() {
			lock (this) {
				var temp = memory;
				memory = new Word[Math.Min( temp.Length * heapGrowFactor, maxHeapSize )];
				System.Array.Copy( temp, memory, temp.Length );
				gens.ForEach( m => m.NewMemory( memory, 1, memory.Length - 1 ) );
			}
		}

		internal override void NewMemory( Word[] memory, int start, int size ) {
			throw new NotSupportedException();
		}
	}
}

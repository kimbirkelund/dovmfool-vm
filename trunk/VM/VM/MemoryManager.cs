using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet;
using VM.VMObjects;

namespace VM {
	class MemoryManager : MemoryManagerBase {
		int initialSize, maxSize;
		public override int SizeInWords { get { return initialSize; } }
		public override int FreeSizeInWords { get { return gens.Sum( m => m.FreeSizeInWords ); } }
		public override int AllocatedSizeInWords { get { return NurserySize + gens.Sum( m => m.AllocatedSizeInWords ); } }
		public readonly int NurserySize;

		internal Word this[int index] {
			get {
				((MarkSweepCompactMemoryManager) gens[0]).AssertValidAddress( true, index );
				return memory[index];
			}
			set {
				((MarkSweepCompactMemoryManager) gens[0]).AssertValidAddress( false, index );
				memory[index] = value;
			}
		}

		Word[] memory;
		MemoryManagerBase[] gens;

		public MemoryManager( int initialHeapSize, int maxHeapSize ) {
			this.initialSize = initialHeapSize;
			this.maxSize = maxHeapSize;
			memory = new Word[initialSize];
			gens = new[] { new MarkSweepCompactMemoryManager( memory, 1, initialSize - 1 ) };
			//gens = new[] { new NoncollectingMemoryManager( memory, 1, initialSize - 1 ) };
		}

		internal override T Allocate<T>( int size ) {
		retry:
			var res = gens[0].Allocate<T>( size );
			if (res.IsNull()) {
				if (memory.Length < maxSize) {
					Expand();
					goto retry;
				} else
					throw VirtualMachine.OutOfMemoryException;
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
				memory = new Word[Math.Min( temp.Length * 2, maxSize )];
				System.Array.Copy( temp, memory, temp.Length );
				gens.ForEach( m => m.NewMemory( memory, 1, memory.Length - 1 ) );
			}
		}

		internal override void NewMemory( Word[] memory, int start, int size ) {
			throw new NotSupportedException();
		}
	}
}

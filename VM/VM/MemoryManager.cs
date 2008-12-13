using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet;

namespace VM {
	class MemoryManager : MemoryManagerBase {
		int size;
		public override int SizeInWords { get { return size; } }
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

		public MemoryManager( int heapSize ) {
			this.size = heapSize;
			memory = new Word[heapSize];
			gens = new[] { new MarkSweepCompactMemoryManager( memory, 1, heapSize - 1 ) };
			//gens = new[] { new NoncollectingMemoryManager( memory, 1, heapSize - 1 ) };
		}

		internal override T Allocate<T>( int size ) {
			return gens[0].Allocate<T>( size );
		}

		protected override void RelocateHere( int obj, int size ) {
			throw new NotSupportedException();
		}

		internal MemoryManagerBase CreateNusery() {
			return null;
		}
	}
}

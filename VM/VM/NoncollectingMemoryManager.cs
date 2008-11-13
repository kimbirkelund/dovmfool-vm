using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class NoncollectingMemoryManager : MemoryManagerBase {
		uint[] memory;
		int position;

		public override int SizeInWords { get { return memory.Length; } }
		public override int FreeSizeInWords { get { return memory.Length - position; } }
		public override int AllocatedSizeInWords { get { return position; } }

		public override Word this[int index] {
			get { return memory[index]; }
			set { memory[index] = value; }
		}

		public NoncollectingMemoryManager( int size ) {
			if (size < 0)
				throw new ArgumentOutOfRangeException( "size" );

			memory = new uint[size];
		}

		public override int Allocate( int size ) {
			var pos = position;
			position += size;
			return pos;
		}
	}
}

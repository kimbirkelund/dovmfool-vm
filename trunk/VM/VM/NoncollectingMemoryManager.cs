using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class NoncollectingMemoryManager : MemoryManagerBase {
		Word[] memory;
		int position;

		public override int SizeInWords { get { return memory.Length; } }
		public override int FreeSizeInWords { get { return memory.Length - position; } }
		public override int AllocatedSizeInWords { get { return position; } }

		internal override Word this[int index] {
			get { return memory[index]; }
			set { memory[index] = value; }
		}

		public NoncollectingMemoryManager( int size ) {
			if (size < 0)
				throw new ArgumentOutOfRangeException( "size" );

			memory = new Word[size];
		}

		internal override T Allocate<T>( int size ) {
			var pos = position;
			size += 1;
			position += size;

			var obj = new T() { Start = pos };
			memory[pos] = (size << 4) | (((int) obj.TypeId) & 0x0000000F);
			return obj;
		}
	}
}

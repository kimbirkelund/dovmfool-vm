using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public partial class NoncollectingMemoryManager : MemoryManagerBase {
		Word[] memory;
		int position = 0;
		int start, size;

		public override int SizeInWords { get { return size; } }
		public override int FreeSizeInWords { get { return size - position; } }
		public override int AllocatedSizeInWords { get { return position; } }

		internal Word this[int index] {
			get { return memory[start + index]; }
			set { memory[start + index] = value; }
		}

		public NoncollectingMemoryManager( Word[] memory, int start, int size ) {
			this.memory = memory;
			this.start = start;
			this.size = size;
		}

		internal override T Allocate<T>( int size ) {
			if (position + size >= SizeInWords)
				throw VirtualMachine.OutOfMemoryException;
			if (size > 0x40000000)
				throw VirtualMachine.OutOfMemoryException;

			var pos = position + 2;
			size += 2;
			position += size;

			var obj = new T().New( pos + start );
			this[pos + VMObjects.ObjectBase.CLASS_POINTER_OFFSET] = obj.VMClass.Start;
			this[pos + VMObjects.ObjectBase.SIZE_OFFSET] = size << ObjectBase.SIZE_RSHIFT;
			return obj;
		}

		protected override void RelocateHere( int obj, int size ) {
			var w = Allocate<AppObject>( size ).Start;
			for (int i = 0; i < size; i++)
				this[w + i] = this[obj + i];
		}

		internal override void NewMemory( Word[] memory, int start, int size ) {
			this.start = start;
			this.size = size;
			this.memory = memory;
		}
	}
}

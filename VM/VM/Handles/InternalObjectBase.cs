using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public abstract class InternalObjectBase : ObjectBase {
		public const uint SIZE_MASK = 0xFFFFFFF0;
		public const int SIZE_RSHIFT = 4;

		public const uint TypeId = Class.TypeId;

		public override uint Size {
			get { return (this[0] & SIZE_MASK) >> SIZE_RSHIFT; }
		}

		protected InternalObjectBase( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

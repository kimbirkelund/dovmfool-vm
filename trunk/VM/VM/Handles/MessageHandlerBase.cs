using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class MessageHandlerBase : InternalObjectBase {
		public const uint VISIBILITY_MASK = 0x00000003;
		public const uint IS_INTERNAL_MASK = 0x00000004;
		public const int IS_INTERNAL_RSHIFT = 3;
		public const uint NAME_MASK = 0xFFFFFFF0;
		public const int NAME_RSHIFT = 4;

		public AccessModifier Visibility { get { return (AccessModifier) (this[1] & VISIBILITY_MASK); } }
		public bool IsInternal { get { return ((this[1] & IS_INTERNAL_MASK) >> IS_INTERNAL_RSHIFT) != 0; } }

		public String Name {
			get {
				var nameIndex = (this[1] & NAME_MASK) >> NAME_RSHIFT;
				return this.VirtualMachine.ConstantPool.GetConstant<String>( nameIndex );
			}
		}

		internal MessageHandlerBase( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class ConstantPool : InternalObjectBase {
		internal ConstantPool( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }

		public T GetConstant<T>( uint index ) where T : ObjectBase {
			return null;
		}
	}
}

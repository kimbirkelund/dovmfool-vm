using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public interface IVMObject {
		int Start { get; set; }
	}

	public static class ExtIVMObject {
		public static Handle<T> AsHandle<T>( this T value ) where T : struct, IVMObject {
			return VirtualMachine.MemoryManager.Register( value );
		}
	}
}

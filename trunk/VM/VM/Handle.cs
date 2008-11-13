using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public class HandleBase {
		public bool IsValid { get; private set; }
		public int Start { get; internal set; }

		public void Unregister() {
			IsValid = false;
			VirtualMachine.MemoryManager.Unregister( this );
		}

		protected HandleBase( int start ) {
			this.Start = start;
		}
	}

	public class Handle<T> : HandleBase where T : struct, IVMObject {
		public T Value { get { return new T { Start = Start }; } }

		public Handle( T value ) : base( value.Start ) { }
	}
}

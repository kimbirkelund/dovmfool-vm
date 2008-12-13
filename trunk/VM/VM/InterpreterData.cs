using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	class InterpretorData {
		public int Id;
		public IInterpretor AttachedTo;
		public ExecutionStack Stack = new ExecutionStack( 100 );
		public Handle<MessageHandlerBase> Handler;
		public int PC = 0;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Debugging.Service.Server {
	class DebuggerServiceImpl : IDebuggerService {
		public int[] GetInterpreters() {
			return DebuggerService.GetInterpreters();
		}

		public InterpreterPosition Break( int id ) {
			return DebuggerService.Break( id );
		}

		public InterpreterPosition StepOne( int id ) {
			return DebuggerService.StepOne( id );
		}

		public void Continue( int id ) {
			DebuggerService.Continue( id );
		}

		public void Attach() {
			DebuggerService.Attach();
		}

		public void Detach() {
			DebuggerService.Detach();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Debugging.Service.Server {
	class DebuggerServiceImpl : IDebuggerService {
		public int[] GetInterpretors() {
			return DebuggerService.GetInterpretors();
		}

		public InterpretorPosition Break( int id ) {
			return DebuggerService.Break( id );
		}

		public InterpretorPosition StepOne( int id ) {
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

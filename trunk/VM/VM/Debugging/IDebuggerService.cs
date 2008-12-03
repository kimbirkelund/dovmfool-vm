using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace VM.Debugging {
	[ServiceContract( Namespace = "http://vm.sekhmet.dk" )]
	public interface IDebuggerService {

		void BreakAllInterpretors();

		void BreakInterpretor( int id );

		int[] GetInterpretors();

		//void SetBreakpoint(
	}
}

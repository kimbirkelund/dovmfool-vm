using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpretorFactory {
		IInterpretor CreateInstance( InterpretorThread thread, Handle<VMObjects.AppObject> entrypointObject, Handle<VMObjects.MessageHandlerBase> entrypoint, params Handle<AppObject>[] args );
		IInterpretor CreateInstance( InterpretorThread thread );
		IInterpretor CreateInstance( InterpretorThread thread, InterpretorPosition position );

		IExecutionStack CreateStack();
	}
}

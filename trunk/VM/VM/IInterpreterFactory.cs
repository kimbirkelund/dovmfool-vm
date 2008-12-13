using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpreterFactory {
		IInterpreter CreateInstance( InterpreterThread thread, Handle<VMObjects.AppObject> entrypointObject, Handle<VMObjects.MessageHandlerBase> entrypoint, params Handle<AppObject>[] args );
		IInterpreter CreateInstance( InterpreterThread thread );
		IInterpreter CreateInstance( InterpreterThread thread, InterpreterPosition position );

		IExecutionStack CreateStack();
		IExecutionStack CreateStack( IExecutionStack stack );
	}
}

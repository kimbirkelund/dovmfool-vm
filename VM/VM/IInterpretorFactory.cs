using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpretorFactory {
		IInterpretor CreateInstance( Handle<VMObjects.AppObject> entrypointObject, Handle<VMObjects.VMILMessageHandler> entrypoint, params Handle<AppObject>[] args );
		IInterpretor CreateInstance();
	}
}

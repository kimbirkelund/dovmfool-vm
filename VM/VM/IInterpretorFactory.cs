using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpretorFactory {
		IInterpretor CreateInstance( int id, Handle<VMObjects.AppObject> entrypointObject, Handle<VMObjects.MessageHandlerBase> entrypoint, params Handle<AppObject>[] args );
		IInterpretor CreateInstance( int id );
	}
}

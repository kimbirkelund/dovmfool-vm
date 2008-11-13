using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpretorFactory {
		IInterpretor CreateInstance( VMObjects.AppObject entryObject, VMObjects.MessageHandlerBase entryPoint, params AppObject[] args );
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VM.VMObjects;

namespace VM.Debugging.Service {
	[ServiceContract( Namespace = "http://vm.sekhmet.dk/ObjectReflection" )]
	interface IObjectReflectionService {
		Value GetField( int objectId, int classId, int index );
		int Class( int objectId );
	}
}

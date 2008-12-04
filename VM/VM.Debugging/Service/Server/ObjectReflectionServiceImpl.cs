using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Debugging.Service.Server {
	class ObjectReflectionServiceImpl : IObjectReflectionService {
		public Value GetField( int objectId, int classId, int index ) {
			throw new NotImplementedException();
		}

		public int Class( int objectId ) {
			throw new NotImplementedException();
		}
	}
}

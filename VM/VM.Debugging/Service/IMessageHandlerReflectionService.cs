using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VMILLib;

namespace VM.Debugging.Service {
	[ServiceContract( Namespace = "http://vm.sekhmet.dk/MessageHandlerReflection" )]
	interface IMessageHandlerReflectionService {
		[OperationContract]
		string Name( int id );
		[OperationContract]
		int Class( int id );
		[OperationContract]
		VisibilityModifier Visibility( int id );
		[OperationContract]
		bool IsExternal( int id );
		[OperationContract]
		bool IsDefault( int id );
		[OperationContract]
		bool IsEntrypoint( int id );
		[OperationContract]
		int ArgumentCount( int id );
		[OperationContract]
		int LocalCount( int id );
	}
}

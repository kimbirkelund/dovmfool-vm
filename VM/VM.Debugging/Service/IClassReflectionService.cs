using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using VMILLib;

namespace VM.Debugging.Service {
	[ServiceContract( Namespace = "http://vm.sekhmet.dk/ClassReflection" )]
	interface IClassReflectionService {
		[OperationContract]
		int ResolveClass( string name );

		[OperationContract]
		string Filename( int id );
		[OperationContract]
		string Name( int id );
		[OperationContract]
		VisibilityModifier Visibility( int id );
		[OperationContract]
		string Fullname( int id );
		[OperationContract]
		int SuperClassCount( int id );
		[OperationContract]
		string[] SuperClassNames( int id );
		[OperationContract]
		int[] SuperClasses( int id );
		[OperationContract]
		int? DefaultMessageHandler( int id );
		[OperationContract]
		int MessageHandlerCount( int id );
		[OperationContract]
		int[] MessageHandlers( int id );
		[OperationContract]
		int InnerClassCount( int id );
		[OperationContract]
		int[] InnerClasses( int id );
		[OperationContract]
		int? ParentClass( int id );
		[OperationContract]
		int FieldCount( int id );
		[OperationContract]
		int TotalFieldCount( int id );
	}
}

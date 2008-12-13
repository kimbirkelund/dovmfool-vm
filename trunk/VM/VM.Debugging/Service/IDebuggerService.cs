using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace VM.Debugging.Service {
	[ServiceContract( Namespace = "http://vm.sekhmet.dk/Debugger", CallbackContract = typeof( IDebuggerCallbackService ) )]
	internal interface IDebuggerService {
		[OperationContract]
		void Attach();
		[OperationContract]
		void Detach();
		[OperationContract]
		int[] GetInterpreters();

		[OperationContract]
		InterpreterPosition Break( int id );
		[OperationContract]
		InterpreterPosition StepOne( int id );
		[OperationContract]
		void Continue( int id );
	}

	internal interface IDebuggerCallbackService {
		void NewInterpreter( int id );
		void Pop( int interpreterId, int popCount );
		void Push( int interpreterId, Value value );
		void StackChange( int interpreterId, Value newValue );
	}

	[DataContract]
	class InterpreterPosition {
		[DataMember]
		public int MessageHandlerId { get; set; }
		[DataMember]
		public int Position { get; set; }

		public InterpreterPosition( int messageHandlerId, int position ) {
			this.MessageHandlerId = messageHandlerId;
			this.Position = position;
		}
	}
}

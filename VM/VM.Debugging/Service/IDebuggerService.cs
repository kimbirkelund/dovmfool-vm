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
		int[] GetInterpretors();

		[OperationContract]
		InterpretorPosition Break( int id );
		[OperationContract]
		InterpretorPosition StepOne( int id );
		[OperationContract]
		void Continue( int id );
	}

	internal interface IDebuggerCallbackService {
		void NewInterpretor( int id );
		void Pop( int interpretorId, int popCount );
		void Push( int interpretorId, Value value );
		void StackChange( int interpretorId, Value newValue );
	}

	[DataContract]
	class InterpretorPosition {
		[DataMember]
		public int MessageHandlerId { get; set; }
		[DataMember]
		public int Position { get; set; }

		public InterpretorPosition( int messageHandlerId, int position ) {
			this.MessageHandlerId = messageHandlerId;
			this.Position = position;
		}
	}
}

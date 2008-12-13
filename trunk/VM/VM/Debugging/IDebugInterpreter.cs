using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM.Debugging {
	interface IDebugInterpreter : IInterpreter {
		event EventHandler<StackPushEventArgs> StackPushed;
		event EventHandler<StackPopEventArgs> StackPopped;
		event EventHandler<StackChangeEventArgs> StackChanged;
		event EventHandler<BreakpointHitEventArgs> BreakpointHit;

		void SetBreakpoint( Handle<VMILMessageHandler> messageHandler, int offset );
		void ClearBreakpoint( Handle<VMILMessageHandler> messageHandler, int offset );
		void ClearBreakpoints();
		InterpreterPosition Break();
		InterpreterPosition StepOne();
		void Continue();
	}

	class BreakpointHitEventArgs : EventArgs {
		public readonly Handle<MessageHandlerBase> MessageHandler;
		public readonly int Position;

		public BreakpointHitEventArgs( Handle<MessageHandlerBase> messageHandler, int position ) {
			this.MessageHandler = messageHandler;
			this.Position = position;
		}
	}

	class StackPushEventArgs : EventArgs {
		public readonly UValue Object;

		public StackPushEventArgs( UValue obj ) {
			this.Object = obj;
		}
	}

	class StackPopEventArgs : EventArgs {
		public readonly int PopCount;

		public StackPopEventArgs( int popCount ) {
			this.PopCount = popCount;
		}
	}

	class StackChangeEventArgs : EventArgs {
		public readonly int Position;
		public readonly UValue NewValue;

		public StackChangeEventArgs( int position, UValue newValue ) {
			this.Position = position;
			this.NewValue = newValue;
		}
	}
}

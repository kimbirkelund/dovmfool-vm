using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using VM.Debugging;

namespace VM {
	interface IInterpreter : IDisposable {
		InterpreterThread Thread { get; }
		InterpreterPosition CurrentPosition { get; set; }
		Handle<AppObject> FinalResult { get; set; }

		bool ReturnWhenPossible { get; set; }
		void Run();
	}

	public enum InterpreterState {
		NotStarted,
		Running,
		Paused,
		Blocked,
		Stopped
	}

	class InterpreterPosition {
		public readonly Handle<MessageHandlerBase> MessageHandler;
		public readonly int Position;

		public InterpreterPosition( Handle<MessageHandlerBase> messageHandler, int position ) {
			this.MessageHandler = messageHandler;
			this.Position = position;
		}
	}
}

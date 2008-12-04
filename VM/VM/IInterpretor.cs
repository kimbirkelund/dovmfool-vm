using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using VM.Debugging;

namespace VM {
	interface IInterpretor : IDisposable {
		InterpretorThread Thread { get; }
		InterpretorPosition CurrentPosition { get; }
		Handle<AppObject> FinalResult { get; }

		void Run();
		UValue Send( Handle<VMObjects.String> message, Handle<VMObjects.AppObject> to, params Handle<AppObject>[] arguments );
	}

	public enum InterpretorState {
		NotStarted,
		Running,
		Paused,
		Blocked,
		Stopped
	}

	class InterpretorPosition {
		public readonly Handle<MessageHandlerBase> MessageHandler;
		public readonly int Position;

		public InterpretorPosition( Handle<MessageHandlerBase> messageHandler, int position ) {
			this.MessageHandler = messageHandler;
			this.Position = position;
		}
	}
}

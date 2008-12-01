using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	interface IInterpretor {
		InterpretorState State { get; }
		void Start();
		void Pause();
		void Resume();
		void Kill();
		void Join();

		UValue Send( Handle<VMObjects.String> message, Handle<VMObjects.AppObject> to, params Handle<AppObject>[] arguments );
	}

	public enum InterpretorState {
		NotStarted,
		Running,
		Paused,
		Blocked,
		Stopped
	}
}

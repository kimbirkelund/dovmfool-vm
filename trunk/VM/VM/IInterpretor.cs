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
	}

	public enum InterpretorState {
		NotStarted,
		Running,
		Paused,
		Blocked,
		Stopped
	}
}

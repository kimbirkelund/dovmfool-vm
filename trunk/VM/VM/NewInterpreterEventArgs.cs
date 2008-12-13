using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	internal sealed class NewThreadEventArgs : EventArgs {
		public readonly InterpreterThread Thread;

		public NewThreadEventArgs( InterpreterThread thread ) {
			this.Thread = thread;
		}
	}
}

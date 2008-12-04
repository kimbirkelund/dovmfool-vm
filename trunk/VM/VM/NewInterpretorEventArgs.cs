using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	internal sealed class NewThreadEventArgs : EventArgs {
		public readonly InterpretorThread Thread;

		public NewThreadEventArgs( InterpretorThread thread ) {
			this.Thread = thread;
		}
	}
}

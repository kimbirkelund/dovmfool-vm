using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VMILLib;

namespace VM.Debugging.Service.Client {
	public sealed class MessageHandler {
		public string Name { get; private set; }

		public VisibilityModifier Visibility { get; private set; }

		public Class Class { get; private set; }


	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct DelegateMessageHandler : IVMObject {
		public const uint TypeId = 5;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( DelegateMessageHandler v ) {
			return v.start;
		}

		public static implicit operator DelegateMessageHandler( int v ) {
			return new DelegateMessageHandler { start = v };
		}
	}
}

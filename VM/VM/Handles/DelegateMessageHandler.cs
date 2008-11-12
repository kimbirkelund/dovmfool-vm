using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct DelegateMessageHandler  {
		public const uint TypeId = 5;

		uint start;

		public static implicit operator uint( DelegateMessageHandler v ) {
			return v.start;
		}

		public static implicit operator DelegateMessageHandler( uint v ) {
			return new DelegateMessageHandler { start = v };
		}
	}
}

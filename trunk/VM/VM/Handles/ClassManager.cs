using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct ClassManager {
		uint start;

		public static implicit operator uint( ClassManager v ) {
			return v.start;
		}

		public static implicit operator ClassManager( uint v ) {
			return new ClassManager { start = v };
		}
	}
}

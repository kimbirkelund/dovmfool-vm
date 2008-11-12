using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct Integer {
		uint start;

		public static implicit operator uint( Integer v ) {
			return v.start;
		}

		public static implicit operator Integer( uint v ) {
			return new Integer { start = v };
		}
	}
}

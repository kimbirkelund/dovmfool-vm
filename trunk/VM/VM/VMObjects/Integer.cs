using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct Integer : IVMObject {
		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( Integer v ) {
			return v.start;
		}

		public static implicit operator Integer( int v ) {
			return new Integer { start = v };
		}

		public static implicit operator AppObject( Integer s ) {
			return (int) s;
		}

		public static explicit operator Integer( AppObject obj ) {
			return (int) obj;
		}
	}
}

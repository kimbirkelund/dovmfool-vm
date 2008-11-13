using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct ClassManager:IVMObject {
		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( ClassManager v ) {
			return v.start;
		}

		public static implicit operator ClassManager( int v ) {
			return new ClassManager { start = v };
		}
	}
}

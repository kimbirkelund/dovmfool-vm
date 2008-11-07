using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class CString {
		public readonly int Index;
		public readonly string Value;

		public CString( int index, string value ) {
			this.Index = index;
			this.Value = value;
		}

		public override string ToString() {
			return Value;
		}
	}
}

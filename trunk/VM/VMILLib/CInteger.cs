using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class CInteger {
		public readonly int Index;
		public readonly int Value;

		public CInteger( int index, int value ) {
			this.Index = index;
			this.Value = value;
		}

		public override string ToString() {
			return Value.ToString();
		}
	}
}

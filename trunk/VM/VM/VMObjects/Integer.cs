using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct Integer {
		#region Properties
		int value;
		public int Value { get { return value; } }
		#endregion

		#region Cons
		public Integer( int value ) {
			this.value = value;
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return value.ToString();
		}
		#endregion

		#region Casts
		public static implicit operator int( Integer v ) {
			return v.value;
		}

		public static implicit operator Integer( int v ) {
			return new Integer( v );
		}
		#endregion
	}
}

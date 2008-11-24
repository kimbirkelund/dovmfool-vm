using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct Character {
		#region Properties
		public bool IsNull { get { return false; } }

		byte b1, b2;
		public byte Byte1 { get { return b1; } }
		public byte Byte2 { get { return b2; } }
		#endregion

		#region Cons
		public Character( int start ) {
			b1 = (byte) (start >> 8);
			b2 = (byte) start;
		}

		public Character( byte b1, byte b2 ) {
			this.b1 = b1;
			this.b2 = b2;
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return Encoding.Unicode.GetString( new byte[] { Byte1, Byte2 } );
		}
		#endregion

		#region Casts
		public static explicit operator Character( int v ) {
			return new Character( v );
		}

		public static implicit operator int( Character c ) {
			return (((int) c.b1) << 8) | c.b2;
		}
		#endregion
	}
}

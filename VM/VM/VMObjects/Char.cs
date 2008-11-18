using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct Char : IVMObject {
		#region Properties
		byte b1, b2;
		public byte Byte1 { get { return b1; } set { b1 = value; } }
		public byte Byte2 { get { return b2; } set { b2 = value; } }

		public int Start {
			get { return (((int) b1) << 8) | b2; }
			set {
				b1 = (byte) (value >> 8);
				b2 = (byte) value;
			}
		}

		public Word this[int index] {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		public int Size {
			get { return 1; }
		}

		public VMILLib.TypeId TypeId {
			get { return VMILLib.TypeId.Char; }
		}
		#endregion

		#region Casts
		public static explicit operator Char( int v ) {
			return new Char { Start = v };
		}

		public static implicit operator int( Char c ) {
			return c.Start;
		}
		#endregion
	}
}

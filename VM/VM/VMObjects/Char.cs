using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct Char : IVMObject<Char> {
		#region Properties
		public bool IsNull { get { return false; } }
		
		byte b1, b2;
		public byte Byte1 { get { return b1; } }
		public byte Byte2 { get { return b2; } }

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

		#region Cons
		public Char( int start ) {
			b1 = (byte) (start >> 8);
			b2 = (byte) start;
		}

		public Char( byte b1, byte b2 ) {
			this.b1 = b1;
			this.b2 = b2;
		}

		public Char New( int startPosition ) {
			return new Char( startPosition );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return Encoding.Unicode.GetString( new byte[] { Byte1, Byte2 } );
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

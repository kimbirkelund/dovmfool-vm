using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct Integer : IVMObject {
		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.Integer; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }
		
		public Word this[int index] {
			get { throw new NotSupportedException(); }
			set { throw new NotSupportedException(); }
		}

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}
		#endregion

		#region Casts
		public static implicit operator int( Integer v ) {
			return v.start;
		}

		public static implicit operator Integer( int v ) {
			return new Integer { start = v };
		}

		public static implicit operator AppObject( Integer s ) {
			return new AppObject { Start = s.start };
		}

		public static explicit operator Integer( AppObject obj ) {
			return (int) obj;
		}
		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public static class ExtSize {
		public static int Size( this AppObject obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this AppObjectSet obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this Class obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this ClassManager obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this MessageHandlerBase obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this VMILMessageHandler obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this DelegateMessageHandler obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static int Size( this String obj ) {
			var length = obj.Length();
			return length / 2 + length % 2;
		}

		public static int Size( this Integer obj ) {
			return 1;
		}
	}
}

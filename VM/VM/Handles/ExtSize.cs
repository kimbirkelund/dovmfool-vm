using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public static class ExtSize {
		public static uint Size( this AppObject obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this AppObjectSet obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this Class obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this ClassManager obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this MessageHandlerBase obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this VMILMessageHandler obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this DelegateMessageHandler obj ) {
			return obj.Get( ObjectBase.OBJECT_HEADER_OFFSET ) >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static uint Size( this String obj ) {
			var length = obj.Length();
			return length / 2 + length % 2;
		}

		public static uint Size( this Integer obj ) {
			return 1;
		}
	}
}

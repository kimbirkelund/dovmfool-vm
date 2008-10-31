using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public static class InternalObjectExt {
		public static uint GetSize<T>( this Handle<T> h ) where T : InternalObject {
			return (h[0] & Masks.OBJ_HEADER_INTERNAL_SIZE) >> Masks.OBJ_HEADER_INTERNAL_SIZE_RSHIFT;
		}
	}
}

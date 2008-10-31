using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public static class AppObjectExt {
		public static Handle<Class> GetClass<T>( this Handle<T> h ) where T : AppObject {
			if (!h.IsAppObject())
				return null;

			int clsIndex = (int) ((h[0] & Masks.OBJ_HEADER_APPOBJECT_CLASS) >> Masks.OBJ_HEADER_APPOBJECT_CLASS_RSHIFT);
			return h.VirtualMachine.ClassManager.Get( clsIndex );
		}

		public static uint GetSize( this Handle<AppObject> h ) {
			return 0;
		}
	}
}

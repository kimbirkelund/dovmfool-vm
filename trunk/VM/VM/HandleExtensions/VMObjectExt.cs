using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public static class VMObjectExt {
		public static uint GetObjectType<T>( this Handle<T> h ) where T : VMObject {
			return h[0] & Masks.OBJ_HEADER_OBJECT_TYPE;
		}

		public static bool IsAppObject<T>( this Handle<T> h ) where T : VMObject {
			return h.GetObjectType() == AppObject.TypeId;
		}

		public static bool IsAppObjectSet<T>( this Handle<T> h ) where T : VMObject {
			return h.GetObjectType() == AppObjectSet.TypeId;
		}

		public static bool IsInternalObject<T>( this Handle<T> h ) where T : VMObject {
			return h.GetObjectType() > InternalObject.TypeId;
		}

		public static uint GetSize( this Handle<VMObject> h ) {
			var t = h.GetObjectType();

			if (t == AppObject.TypeId)
				return h.To<AppObject>().GetSize();
			else if (t == AppObjectSet.TypeId)
				return AppObjectSetExt.GetSize( h.To<AppObjectSet>() );
			else if (t >= InternalObject.TypeId)
				return InternalObjectExt.GetSize( h.To<InternalObject>() );
			else
				throw new InvalidOperationException( "Invalid object type: " + h.GetObjectType() );
		}
	}
}

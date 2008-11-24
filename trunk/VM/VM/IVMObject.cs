using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;
using VM.VMObjects;

namespace VM {
	public interface IVMObject<T> where T : struct, IVMObject<T> {
		int Start { get; }
		T New( int start );
		TypeId TypeIdAtInstancing { get; }
	}

	public static class ExtIVMObject {
		public static Handle<T> ToHandle<T>( this T obj ) where T : struct, IVMObject<T> {
			if (obj.Start == 0)
				return null;
			return (Handle<T>) obj;
		}

		public static IntHandle ToHandle( this Integer obj ) {
			return new IntHandle( obj.Value );
		}

		public static int Size<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			return obj[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT;
		}

		public static TypeId TypeId<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			return (TypeId) (int) (obj[0] & VMObjects.ObjectBase.OBJECT_TYPE_MASK);
		}

		public static bool IsNull<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			return object.ReferenceEquals( obj, null ) ? true : obj.Start == 0;
		}
	}
}

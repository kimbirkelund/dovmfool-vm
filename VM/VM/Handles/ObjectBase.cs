using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct ObjectBase {
		public const uint OBJECT_HEADER_OFFSET = 0;
		public const uint OBJECT_TYPE_MASK = 0x00000100;
		public const int OBJECT_SIZE_RSHIFT = 4;

		uint start;

		public static implicit operator uint( ObjectBase cls ) {
			return cls.start;
		}

		public static implicit operator ObjectBase( uint cls ) {
			return new ObjectBase { start = cls };
		}
	}

	public static class ExtObjectBase {
		public static uint ObjectType( this ObjectBase objectBase ) {
			return objectBase.Get( ObjectBase.OBJECT_HEADER_OFFSET ) & ObjectBase.OBJECT_TYPE_MASK;
		}

		public static bool IsAppObject( this ObjectBase objectBase ) {
			return objectBase.ObjectType() == AppObject.TypeId;
		}

		/// <summary>
		/// Gets a value indicating if <paramref name="h"/> is an application object set. Constant time operation.
		/// </summary>
		public static bool IsAppObjectSet( this ObjectBase objectBase ) {
			return objectBase.ObjectType() == AppObjectSet.TypeId;
		}

		/// <summary>
		/// Gets a value indicating if <paramref name="h"/> is an internal object. Constant time operation.
		/// </summary>
		public static bool IsInternalObject( this ObjectBase objectBase ) {
			return objectBase.ObjectType() >= InternalObjectBase.TypeId;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct ObjectBase : IVMObject {
		public const int OBJECT_HEADER_OFFSET = 0;
		public static readonly Word OBJECT_TYPE_MASK = 0x00000100;
		public const int OBJECT_SIZE_RSHIFT = 4;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( ObjectBase cls ) {
			return cls.start;
		}

		public static implicit operator ObjectBase( int cls ) {
			return new ObjectBase { start = cls };
		}

		#region Casts
		public static explicit operator AppObject( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator AppObjectSet( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator Class( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator ClassManager( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator MessageHandlerBase( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator VMILMessageHandler( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator DelegateMessageHandler( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator String( ObjectBase obj ) {
			return (int) obj;
		}

		public static explicit operator Integer( ObjectBase obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( AppObject obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( AppObjectSet obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( Class obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( ClassManager obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( MessageHandlerBase obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( VMILMessageHandler obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( DelegateMessageHandler obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( String obj ) {
			return (int) obj;
		}

		public static implicit operator ObjectBase( Integer obj ) {
			return (int) obj;
		}
		#endregion
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

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct ObjectBase : IVMObject<ObjectBase> {
		#region Constants
		public const int CLASS_POINTER_OFFSET = -1;
		public const int SIZE_OFFSET = -2;
		public const int SIZE_RSHIFT = 2;
		#endregion

		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.Object; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}
		#endregion

		#region Cons
		public ObjectBase( int start ) {
			this.start = start;
		}

		public ObjectBase New( int startPosition ) {
			return new ObjectBase( startPosition );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			using (var hThis = this.ToHandle())
				return ExtObjectBase.ToString( hThis );
		}

		public bool Equals( Handle<ObjectBase> obj1, Handle<ObjectBase> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion

		#region Casts
		public static implicit operator int( ObjectBase cls ) {
			return cls.start;
		}

		public static implicit operator ObjectBase( int cls ) {
			return new ObjectBase { start = cls };
		}

		public static explicit operator AppObject( ObjectBase obj ) {
			return new AppObject( obj.start );
		}

		public static explicit operator Class( ObjectBase obj ) {
			return new Class( obj.start );
		}

		public static explicit operator MessageHandlerBase( ObjectBase obj ) {
			return new MessageHandlerBase( obj.start );
		}

		public static explicit operator VMILMessageHandler( ObjectBase obj ) {
			return new VMILMessageHandler( obj.start );
		}

		public static explicit operator DelegateMessageHandler( ObjectBase obj ) {
			return new DelegateMessageHandler( obj.start );
		}

		public static explicit operator String( ObjectBase obj ) {
			return new String( obj.start );
		}

		public static explicit operator Integer( ObjectBase obj ) {
			return new Integer( obj.start );
		}

		public static implicit operator ObjectBase( AppObject obj ) {
			return new ObjectBase( obj.Start );
		}

		public static implicit operator ObjectBase( Class obj ) {
			return new ObjectBase( obj.Start );
		}

		public static implicit operator ObjectBase( MessageHandlerBase obj ) {
			return new ObjectBase( obj.Start );
		}

		public static implicit operator ObjectBase( VMILMessageHandler obj ) {
			return new ObjectBase( obj.Start );
		}

		public static implicit operator ObjectBase( DelegateMessageHandler obj ) {
			return new ObjectBase( obj.Start );
		}

		public static implicit operator ObjectBase( String obj ) {
			return new ObjectBase( obj.Start );
		}
		#endregion
	}

	public static class ExtObjectBase {
		public static string ToString( this Handle<ObjectBase> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			using (var hObjClass = obj.Class().ToHandle())
				return "{" + hObjClass.Name().ToString() + "}";
		}

		public static Handle<T> ToHandle<T>( this T obj ) where T : struct, IVMObject<T> {
			if (obj.Start == 0)
				return null;
			var h = MemoryManagerBase.CreateHandle<T>( obj, false );
#if DEBUG
			if (!h.IsWeak)
				MemoryManagerBase.AssertHandle( h );
#endif
			return h;
		}

		public static IntHandle ToHandle( this Integer obj ) {
			return new IntHandle( obj.Value );
		}

		public static IntHandle ToHandle( this int obj ) {
			return new IntHandle( obj );
		}

		public static Handle<T> ToWeakHandle<T>( this T obj ) where T : struct, IVMObject<T> {
			if (obj.Start == 0)
				return null;
			return MemoryManagerBase.CreateHandle<T>( obj, true );
		}

		public static IntHandle ToDebugHandle( this Integer obj ) {
			return new IntHandle( obj.Value );
		}

		public static IntHandle ToDebugHandle( this int obj ) {
			return new IntHandle( obj );
		}

		public static Class Class<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			if (obj is IntHandle)
				return KnownClasses.System_Integer;
			if (((int) obj[ObjectBase.CLASS_POINTER_OFFSET]) < 0) {
				MarkSweepCompactMemoryManager.DisableAddressVerification();
				obj[ObjectBase.CLASS_POINTER_OFFSET] = KnownClasses.Resolve( obj[ObjectBase.CLASS_POINTER_OFFSET] );
				MarkSweepCompactMemoryManager.EnableAddressVerification();
			}
			return (Class) obj[ObjectBase.CLASS_POINTER_OFFSET];
		}

		public static void Class<T>( this Handle<T> obj, Handle<Class> cls ) where T : struct, IVMObject<T> {
			obj[ObjectBase.CLASS_POINTER_OFFSET] = cls;
		}

		public static int Size<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			return obj[ObjectBase.SIZE_OFFSET] >> ObjectBase.SIZE_RSHIFT;
		}

		public static bool IsNull<T>( this Handle<T> obj ) where T : struct, IVMObject<T> {
			if (obj is IntHandle)
				return false;
			return (object.ReferenceEquals( obj, null ) ? true : obj.Start == 0);
		}

		public static bool IsNull<T>( this T obj ) where T : struct, IVMObject<T> {
			return obj.Start == 0;
		}
	}
}

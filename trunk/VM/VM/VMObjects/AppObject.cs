using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class AppObjectConsts {
		public const int FIELDS_OFFSET_OFFSET = 1;
		public const int FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET = 2;
	}

	public struct AppObject : IVMObject<AppObject> {
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
		public AppObject( int start ) {
			this.start = start;
		}

		public AppObject New( int startPosition ) {
			return new AppObject( startPosition );
		}
		#endregion

		#region Casts
		public static implicit operator int( AppObject cls ) {
			return cls.start;
		}

		public static explicit operator AppObject( int cls ) {
			return new AppObject { start = cls };
		}
		#endregion

		#region Static methods
		public static AppObject CreateInstance( Handle<Class> cls ) {
			var linearization = cls.Linearization().ToHandle();
			var obj = VirtualMachine.MemoryManager.Allocate<AppObject>( cls.TotalFieldCount() * 2 + linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET - 1 );
			obj[ObjectBase.CLASS_POINTER_OFFSET] = cls;
			obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] = linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET;

			int fieldOffset = 0;
			for (int i = 0; i < linearization.Length(); i++) {
				obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i] = fieldOffset;
				fieldOffset += linearization.Get<Class>( i ).FieldCount();
			}
			return obj;
		}

		public static AppObject CreateInstance( Handle<String> clsName ) {
			var cls = VirtualMachine.ResolveClass( null, clsName ).ToHandle();
			if (cls != null)
				throw new ClassNotFoundException( clsName );

			return CreateInstance( cls );
		}

		public static AppObject CreateInstance( string clsName ) {
			return CreateInstance( clsName.ToVMString() );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtAppObject.ToString( this.ToHandle() );
		}

		public bool Equals( Handle<AppObject> obj1, Handle<AppObject> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion
	}

	public static class ExtAppObject {
		public static bool Extends( this Handle<AppObject> obj, Handle<Class> cls ) {
			return obj.Class().ToHandle().Extends( cls );
		}

		internal static UValue GetField( this Handle<AppObject> obj, int index ) {
			if (index < 0 || obj.Class().ToHandle().TotalFieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields.".ToVMString() );

			var type = (Class) obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2];
			var value = obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1];
			if (type == KnownClasses.System_Integer.Start)
				return UValue.Int( value );
			return UValue.Ref( type, value );
		}

		public static void SetField<T>( this Handle<AppObject> obj, int index, Handle<T> value ) where T : struct, IVMObject<T> {
			if (value == null) {
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = 0;
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = 0;
			} else {
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = value.Class().Start;
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = value.Start;
			}
		}

		internal static void SetField( this Handle<AppObject> obj, int index, UValue value ) {
			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = value.Type;
			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = value.Value;
		}

		public static int GetFieldOffset( this Handle<AppObject> obj, Handle<Class> superClass ) {
			var lin = obj.Class().ToHandle().Linearization().ToHandle();
			for (int i = 0; i < lin.Length(); i++)
				if (lin.Get<Class>( i ) == superClass)
					return obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i];

			throw new ClassNotFoundException( "Specified class is not in the inheritance hierachy.".ToVMString(), superClass.Name().ToHandle() );
		}

		public static string ToString( this Handle<AppObject> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Instance of " + obj.Class();
		}

		public static Handle<AppObject> Send( this Handle<AppObject> obj, Handle<String> message, params Handle<AppObject>[] arguments ) {
			var cls = obj.Class().ToHandle();
			var handler = cls.ResolveMessageHandler( null, message ).ToHandle();
			if (handler == null)
				throw new MessageNotUnderstoodException( message );

			var interp = VirtualMachine.Fork( handler, obj, arguments );
			interp.Start();
			return interp.Join();
		}
	}
}

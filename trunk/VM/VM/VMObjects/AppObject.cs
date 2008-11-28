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
		public static Handle<AppObject> CreateInstance( Handle<Class> cls ) {
			var linearization = cls.Linearization();
			var obj = VirtualMachine.MemoryManager.Allocate<AppObject>( cls.TotalFieldCount() * 2 + linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET - 1 );
			obj.Class( cls );
			obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] = linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET;

			int fieldOffset = 0;
			for (int i = 0; i < linearization.Length(); i++) {
				obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i] = fieldOffset;
				fieldOffset += linearization.Get<Class>( i ).FieldCount();
			}
			return obj;
		}

		public static Handle<AppObject> CreateInstance( Handle<String> clsName ) {
			var cls = VirtualMachine.ResolveClass( null, clsName );
			if (cls != null)
				throw new ClassNotFoundException( clsName );

			return CreateInstance( cls );
		}

		public static Handle<AppObject> CreateInstance( string clsName ) {
			return CreateInstance( clsName.ToVMString() );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtAppObject.ToString( this );
		}
		#endregion
	}

	public static class ExtAppObject {
		public static bool Extends( this Handle<AppObject> obj, Handle<Class> cls ) {
			return obj.Class().Extends( cls );
		}

		public static Handle<Class> GetFieldType( this Handle<AppObject> obj, int index ) {
			if (index < 0 || obj.Class().TotalFieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return (Class) obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2];
		}

		public static Handle<T> GetFieldValue<T>( this Handle<AppObject> obj, int index ) where T : struct, IVMObject<T> {
			if (index < 0 || obj.Class().TotalFieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return new T().New( obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] );
		}

		public static void SetField<T>( this Handle<AppObject> obj, int index, Handle<T> value ) where T : struct, IVMObject<T> {
			if (index < 0 || obj.Class().TotalFieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = value.Class().Start;
			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = value.Start;
		}

		public static int GetFieldOffset( this Handle<AppObject> obj, Handle<Class> superClass ) {
			var lin = obj.Class().Linearization();
			for (int i = 0; i < lin.Length(); i++)
				if (lin.Get<Class>( i ) == superClass)
					return obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i];

			throw new ClassNotFoundException( "Specified class is not in the inheritance hierachy.", superClass.Name() );
		}

		public static string ToString( this Handle<AppObject> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Instance of " + obj.Class();
		}

		public static Handle<AppObject> Send( this Handle<AppObject> obj, Handle<String> message, params Handle<AppObject>[] args ) {
			return VirtualMachine.Send( message, obj, args );
		}

		public static Handle<AppObject> Send( this Handle<AppObject> obj, string message, params Handle<AppObject>[] args ) {
			return Send( obj, message.ToVMString(), args );
		}
	}
}

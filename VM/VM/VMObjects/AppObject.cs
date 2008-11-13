using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct AppObject : IVMObject {
		public const int CLASS_OFFSET = 1;
		public const int FIELDS_OFFSET = 2;

		public const int TypeId = 0;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( AppObject cls ) {
			return cls.start;
		}

		public static implicit operator AppObject( int cls ) {
			return new AppObject { start = cls };
		}
	}


	public static class ExtAppObject {
		public static Class Class( this AppObject obj ) {
			return (Class) obj.Get( AppObject.CLASS_OFFSET );
		}

		public static bool Extends( this AppObject appObject, Class cls ) {
			return appObject.Class().Extends( cls );
		}

		public static Class GetFieldType( this AppObject obj, int index ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return (Class) obj.Get( AppObject.FIELDS_OFFSET + index * 2 );
		}

		public static Word GetFieldValue( this AppObject obj, int index ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return obj.Get( AppObject.FIELDS_OFFSET + index * 2 + 1 );
		}

		public static void SetField( this AppObject obj, int index, Class cls, Word value ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			obj.Set( AppObject.FIELDS_OFFSET + index * 2, cls );
			obj.Set( AppObject.FIELDS_OFFSET + index * 2 + 1, value );
		}

		public static void SetField( this AppObject obj, int index, Class cls, Integer value ) {
			obj.SetField( cls, index, value );
		}

		public static void SetField( this AppObject obj, int index, Class cls, String value ) {
			obj.SetField( cls, index, value );
		}

		public static void SetField( this AppObject obj, int index, Class cls, AppObject value ) {
			obj.SetField( cls, index, value );
		}

		public static void SetField( this AppObject obj, int index, Class cls, AppObjectSet value ) {
			obj.SetField( cls, index, value );
		}
	}
}

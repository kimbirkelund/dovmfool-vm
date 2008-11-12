using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct AppObject {
		public const uint CLASS_OFFSET = 1;
		public const uint FIELDS_OFFSET = 2;

		public const uint TypeId = 0;

		uint start;

		public static implicit operator uint( AppObject cls ) {
			return cls.start;
		}

		public static implicit operator AppObject( uint cls ) {
			return new AppObject { start = cls };
		}
	}


	public static class ExtAppObject {
		public static Class Class( this AppObject obj ) {
			return obj.Get( AppObject.CLASS_OFFSET );
		}

		public static bool Extends( this AppObject appObject, Class cls ) {
			return appObject.Class().Extends( cls );
		}

		public static Class GetFieldType( this AppObject obj, uint index ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return obj.Get( AppObject.FIELDS_OFFSET + index * 2 );
		}

		public static Class GetFieldValue( this AppObject obj, uint index ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return obj.Get( AppObject.FIELDS_OFFSET + index * 2 + 1 );
		}

		public static void SetField( this AppObject obj, uint index, Class cls, uint value ) {
			if (index < 0 || obj.Class().FieldCount() <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			obj.Set( AppObject.FIELDS_OFFSET + index * 2, cls );
			obj.Set( AppObject.FIELDS_OFFSET + index * 2 + 1, value );
		}

		public static void SetField( this AppObject obj, uint index, Class cls, Integer value ) {
			obj.SetField( cls, index, (uint) value );
		}

		public static void SetField( this AppObject obj, uint index, Class cls, String value ) {
			obj.SetField( cls, index, (uint) value );
		}

		public static void SetField( this AppObject obj, uint index, Class cls, AppObject value ) {
			obj.SetField( cls, index, (uint) value );
		}

		public static void SetField( this AppObject obj, uint index, Class cls, AppObjectSet value ) {
			obj.SetField( cls, index, (uint) value );
		}
	}
}

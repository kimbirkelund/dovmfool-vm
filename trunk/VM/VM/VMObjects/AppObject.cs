using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct AppObject : IVMObject<AppObject> {
		#region Constants
		public const int CLASS_OFFSET = 1;
		public const int FIELDS_OFFSET = 2;
		#endregion

		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.AppObject; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public Class Class { get { return (Class) this[AppObject.CLASS_OFFSET]; } }
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

		#region Instance methods
		public bool Extends( Class cls ) {
			return this.Class.Extends( cls );
		}

		public Class GetFieldType( int index ) {
			if (index < 0 || this.Class.FieldCount <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return (Class) this[AppObject.FIELDS_OFFSET + index * 2];
		}

		public Word GetFieldValue( int index ) {
			if (index < 0 || this.Class.FieldCount <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			return this[AppObject.FIELDS_OFFSET + index * 2 + 1];
		}

		public void SetField( int index, Class cls, Word value ) {
			if (index < 0 || this.Class.FieldCount <= index)
				throw new ArgumentOutOfRangeException( "Index must be less than the number of fields." );

			this[AppObject.FIELDS_OFFSET + index * 2] = cls;
			this[AppObject.FIELDS_OFFSET + index * 2 + 1] = value;
		}

		public void SetField( int index, Class cls, AppObject value ) {
			this.SetField( index, cls, value );
		}
		#endregion

		#region Static methods
		public static AppObject CreateInstance( Class cls ) {
			var obj = VirtualMachine.MemoryManager.Allocate<AppObject>( cls.InstanceSize );
			obj[AppObject.CLASS_OFFSET] = cls;
			return obj;
		}
		#endregion
	}
}

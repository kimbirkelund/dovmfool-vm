using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct AppObjectSet : IVMObject<AppObjectSet> {
		#region Constants
		public const uint OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT_RSHIFT = 4;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return VMILLib.TypeId.AppObjectSet; } }
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
		#endregion

		#region Cons
		public AppObjectSet( int start ) {
			this.start = start;
		}

		public AppObjectSet New( int startPosition ) {
			return new AppObjectSet( startPosition );
		}
		#endregion

		#region Casts
		public static implicit operator int( AppObjectSet cls ) {
			return cls.start;
		}

		public static explicit operator AppObjectSet( int cls ) {
			return new AppObjectSet { start = cls };
		}

		public static implicit operator AppObject( AppObjectSet s ) {
			return new AppObject { Start = s.start };
		}

		public static explicit operator AppObjectSet( AppObject obj ) {
			return new AppObjectSet { start = obj.Start };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return base.ToString();
		}
		#endregion
	}
}

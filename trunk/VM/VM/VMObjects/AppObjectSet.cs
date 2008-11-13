using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct AppObjectSet : IVMObject {
		public const uint OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT_RSHIFT = 4;

		public const int TypeId = 1;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( AppObjectSet cls ) {
			return cls.start;
		}

		public static implicit operator AppObjectSet( int cls ) {
			return new AppObjectSet { start = cls };
		}

		public static implicit operator AppObject( AppObjectSet s ) {
			return (int) s;
		}

		public static explicit operator AppObjectSet( AppObject obj ) {
			return (int) obj;
		}
	}

	class ExtAppObjectSet {
	}
}

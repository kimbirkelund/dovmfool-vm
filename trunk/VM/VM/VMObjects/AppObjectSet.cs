using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class AppObjectSetConsts {
		public const uint OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT_RSHIFT = 4;
	}

	public struct AppObjectSet : IVMObject<AppObjectSet> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.ObjectSet; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
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
			return new AppObject( s.start );
		}

		public static explicit operator AppObjectSet( AppObject obj ) {
			return new AppObjectSet { start = obj.Start };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtAppObjectSet.ToString( this.ToHandle() );
		}

		public bool Equals( Handle<AppObjectSet> obj1, Handle<AppObjectSet> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion
	}

	public static class ExtAppObjectSet {
		public static string ToString( this Handle<AppObjectSet> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "object-set";
		}
	}
}

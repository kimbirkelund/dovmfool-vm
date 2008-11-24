using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct ClassManager : IVMObject<ClassManager> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.ClassManager; } }
		#endregion

		#region Cons
		public ClassManager( int start ) {
			this.start = start;
		}

		public ClassManager New( int start ) {
			return new ClassManager( start );
		}
		#endregion

		#region Casts
		public static implicit operator int( ClassManager v ) {
			return v.start;
		}

		public static explicit operator ClassManager( int v ) {
			return new ClassManager { start = v };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtClassManager.ToString( this );
		}
		#endregion
	}

	public static class ExtClassManager {
		public static string ToString( this Handle<ClassManager> obj ) {
			if (obj == null)
				return "{NULL}";
			return "ClassManager";
		}
	}
}

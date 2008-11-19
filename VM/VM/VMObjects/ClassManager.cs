using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct ClassManager : IVMObject<ClassManager> {
		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.ClassManager; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
		}
		#endregion

		#region Cons
		public ClassManager( int start ) {
			this.start = start;
		}

		public ClassManager New( int startPosition ) {
			return new ClassManager( startPosition );
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
	}
}

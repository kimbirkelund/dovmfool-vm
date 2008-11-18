using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct DelegateMessageHandler : IVMObject {
		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.DelegateMessageHandler; } }
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

		public VisibilityModifier Visibility { get { return (VisibilityModifier) (this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.VISIBILITY_MASK); } }
		public bool IsInternal { get { return true; } }
		public String Name { get { return (String) (this[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT); } }
		public Class Class { get { return (Class) this[MessageHandlerBase.CLASS_POINTER_OFFSET]; } }
		public bool IsEntrypoint { get { return (this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0; } }
		#endregion

		#region Casts
		public static implicit operator int( DelegateMessageHandler v ) {
			return v.start;
		}

		public static explicit operator DelegateMessageHandler( int v ) {
			return new DelegateMessageHandler { start = v };
		}
		#endregion
	}
}

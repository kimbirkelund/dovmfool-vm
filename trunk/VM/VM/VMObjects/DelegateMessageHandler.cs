using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct DelegateMessageHandler : IVMObject<DelegateMessageHandler> {
		#region Constants
		public const int EXTERNAL_NAME_OFFSET = 3;
		public const int ARGUMENT_COUNT_OFFSET = 4;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return VMILLib.TypeId.DelegateMessageHandler; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
		}

		public VisibilityModifier Visibility { get { return (VisibilityModifier) (this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.VISIBILITY_MASK); } }
		public bool IsInternal { get { return true; } }
		public String Name { get { return VirtualMachine.ConstantPool.GetString( this[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT ); } }
		public Class Class { get { return (Class) this[MessageHandlerBase.CLASS_POINTER_OFFSET]; } }
		public bool IsEntrypoint { get { return (this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0; } }
		public String ExternalName { get { return (String) this[EXTERNAL_NAME_OFFSET]; } }
		public int ArgumentCount { get { return this[ARGUMENT_COUNT_OFFSET]; } }
		#endregion

		#region Cons
		public DelegateMessageHandler( int start ) {
			this.start = start;
		}

		public DelegateMessageHandler New( int startPosition ) {
			return new DelegateMessageHandler( startPosition );
		}
		#endregion

		#region Casts
		public static implicit operator int( DelegateMessageHandler v ) {
			return v.start;
		}

		public static explicit operator DelegateMessageHandler( int v ) {
			return new DelegateMessageHandler { start = v };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return ".handler " + Visibility.ToString().ToLower() + " " + Name + " .external " + ExternalName + "(" + ArgumentCount + ")";
		}
		#endregion

		#region Static methods
		internal static DelegateMessageHandler CreateInstance() {
			return VirtualMachine.MemoryManager.Allocate<DelegateMessageHandler>( 3 );
		}
		#endregion
	}
}

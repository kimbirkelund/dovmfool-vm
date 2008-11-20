using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct MessageHandlerBase : IVMObject<MessageHandlerBase> {
		#region Constants
		public const int HEADER_OFFSET = 1;
		public const int CLASS_POINTER_OFFSET = 2;
		public static readonly Word VISIBILITY_MASK = 0x00000003;
		public static readonly Word IS_INTERNAL_MASK = 0x00000004;
		public const int IS_INTERNAL_RSHIFT = 2;
		public static readonly Word IS_ENTRYPOINT_MASK = 0x00000008;
		public const int IS_ENTRYPOINT_RSHIFT = 3;
		public const int NAME_RSHIFT = 4;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return (TypeId) (int) (this[0] & 0x0000000F); } }
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
		public bool IsInternal { get { return ((this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_INTERNAL_MASK) >> MessageHandlerBase.IS_INTERNAL_RSHIFT) != 0; } }
		public String Name { get { return VirtualMachine.ConstantPool.GetString( this[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT ); } }
		public Class Class { get { return (Class) this[CLASS_POINTER_OFFSET]; } }
		public bool IsEntrypoint { get { return (this[HEADER_OFFSET] & IS_ENTRYPOINT_MASK) != 0; } }
		#endregion

		#region Cons
		public MessageHandlerBase( int start ) {
			this.start = start;
		}

		public MessageHandlerBase New( int startPosition ) {
			return new MessageHandlerBase( startPosition );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return ".handler " + Visibility.ToString().ToLower() + " " + Name;
		}
		#endregion

		#region Casts
		public static implicit operator int( MessageHandlerBase v ) {
			return v.start;
		}

		public static explicit operator MessageHandlerBase( int v ) {
			return new MessageHandlerBase( v );
		}

		public static explicit operator VMILMessageHandler( MessageHandlerBase v ) {
			return new VMILMessageHandler( v.start );
		}

		public static explicit operator DelegateMessageHandler( MessageHandlerBase v ) {
			return new DelegateMessageHandler( v.start );
		}

		public static implicit operator MessageHandlerBase( VMILMessageHandler v ) {
			return new MessageHandlerBase( v.Start );
		}

		public static implicit operator MessageHandlerBase( DelegateMessageHandler v ) {
			return new MessageHandlerBase( v.Start );
		}
		#endregion
	}
}

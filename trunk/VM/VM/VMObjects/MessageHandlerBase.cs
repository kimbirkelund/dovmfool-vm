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
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.Undefined; } }
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
			return ExtMessageHandlerBase.ToString( this );
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

	public static class ExtMessageHandlerBase {
		public static VisibilityModifier Visibility( this Handle<MessageHandlerBase> obj ) {
			return (VisibilityModifier) (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static bool IsInternal( this Handle<MessageHandlerBase> obj ) {
			return ((obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_INTERNAL_MASK) >> MessageHandlerBase.IS_INTERNAL_RSHIFT) != 0;
		}

		public static String Name( this Handle<MessageHandlerBase> obj ) {
			return VirtualMachine.ConstantPool.GetString( obj[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT );
		}

		public static Class Class( this Handle<MessageHandlerBase> obj ) {
			return (Class) obj[MessageHandlerBase.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<MessageHandlerBase> obj ) {
			return (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0;
		}

		public static string ToString( this Handle<MessageHandlerBase> obj ) {
			if (obj.IsInternal())
				return obj.To<DelegateMessageHandler>().ToString();
			return obj.To<VMILMessageHandler>().ToString();
		}
	}
}

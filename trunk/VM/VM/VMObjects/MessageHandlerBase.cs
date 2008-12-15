using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	static class MessageHandlerBaseConsts {
		public const int HEADER_OFFSET = 0;
		public const int CLASS_POINTER_OFFSET = 1;
		public static readonly Word VISIBILITY_MASK = 0x00000003;
		public static readonly Word IS_EXTERNAL_MASK = 0x00000004;
		public static readonly Word IS_ENTRYPOINT_MASK = 0x00000008;
		public const int NAME_RSHIFT = 4;
	}

	public struct MessageHandlerBase : IVMObject<MessageHandlerBase> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.System_Reflection_MessageHandler; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}
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
			using (var hThis = this.ToHandle())
				return ExtMessageHandlerBase.ToString( hThis );
		}

		public bool Equals( Handle<MessageHandlerBase> obj1, Handle<MessageHandlerBase> obj2 ) {
			return obj1.Start == obj2.Start;
		}

		internal static IEnumerable<int> GetReferences( int adr ) {
			if ((VirtualMachine.MemoryManager[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.IS_EXTERNAL_MASK) != 0)
				return DelegateMessageHandler.GetReferences( adr );
			else
				return VMILMessageHandler.GetReferences( adr );
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
			return (VisibilityModifier) (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.VISIBILITY_MASK);
		}

		public static bool IsExternal( this Handle<MessageHandlerBase> obj ) {
			return (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.IS_EXTERNAL_MASK) != 0;
		}

		public static String Name( this Handle<MessageHandlerBase> obj ) {
			if (obj[MessageHandlerBaseConsts.HEADER_OFFSET] == 0)
				return (String) 0;
			return String.GetString( obj[MessageHandlerBaseConsts.HEADER_OFFSET] >> MessageHandlerBaseConsts.NAME_RSHIFT );
		}

		public static Class Class( this Handle<MessageHandlerBase> obj ) {
			return (Class) obj[MessageHandlerBaseConsts.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<MessageHandlerBase> obj ) {
			return (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.IS_ENTRYPOINT_MASK) != 0;
		}

		public static int ArgumentCount( this Handle<MessageHandlerBase> obj ) {
			if (obj.IsExternal())
				using (var hDel = obj.To<DelegateMessageHandler>())
					return hDel.ArgumentCount();
			using (var hVmil = obj.To<VMILMessageHandler>())
				return hVmil.ArgumentCount();
		}

		public static string ToString( this Handle<MessageHandlerBase> obj ) {
			if (obj.IsExternal())
				using (var hDel = obj.To<DelegateMessageHandler>())
					return hDel.ToString();
			using (var hVmil = obj.To<VMILMessageHandler>())
				return hVmil.ToString();
		}
	}
}

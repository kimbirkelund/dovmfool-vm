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
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.DelegateMessageHandler; } }
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
			return ExtDelegateMessageHandler.ToString( this );
		}
		#endregion

		#region Static methods
		internal static Handle<DelegateMessageHandler> CreateInstance() {
			return VirtualMachine.MemoryManager.Allocate<DelegateMessageHandler>( ARGUMENT_COUNT_OFFSET );
		}
		#endregion
	}

	public static class ExtDelegateMessageHandler {
		public static VisibilityModifier Visibility( this Handle<DelegateMessageHandler> obj ) {
			return (VisibilityModifier) (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static bool IsInternal( this Handle<DelegateMessageHandler> obj ) { return true; }

		public static Handle<String> Name( this Handle<DelegateMessageHandler> obj ) {
			return VirtualMachine.ConstantPool.GetString( obj[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT );
		}

		public static Handle<Class> Class( this Handle<DelegateMessageHandler> obj ) {
			return (Class) obj[MessageHandlerBase.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<DelegateMessageHandler> obj ) {
			return (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0;
		}

		public static String ExternalName( this Handle<DelegateMessageHandler> obj ) {
			return (String) obj[DelegateMessageHandler.EXTERNAL_NAME_OFFSET];
		}

		public static int ArgumentCount( this Handle<DelegateMessageHandler> obj ) {
			return obj[DelegateMessageHandler.ARGUMENT_COUNT_OFFSET];
		}

		public static string ToString( this Handle<DelegateMessageHandler> obj ) {
			if (obj == null)
				return "{NULL}";
			return ".handler " + obj.Visibility().ToString().ToLower() + " " + obj.Name() + " .external " + obj.ExternalName() + "(" + obj.ArgumentCount() + ")";
		}
	}
}

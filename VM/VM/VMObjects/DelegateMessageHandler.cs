using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct DelegateMessageHandler : IVMObject<DelegateMessageHandler> {
		#region Constants
		public const int EXTERNAL_NAME_OFFSET = 2;
		public const int ARGUMENT_COUNT_OFFSET = 3;
		#endregion

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
			using (var hThis = this.ToHandle())
				return ExtDelegateMessageHandler.ToString( hThis );
		}

		public bool Equals( Handle<DelegateMessageHandler> obj1, Handle<DelegateMessageHandler> obj2 ) {
			return obj1.Start == obj2.Start;
		}

		internal static int[] GetReferences( int adr ) {
			return new int[] { 
				VirtualMachine.MemoryManager[adr + MessageHandlerBaseConsts.CLASS_POINTER_OFFSET], 
				VirtualMachine.MemoryManager[adr + EXTERNAL_NAME_OFFSET] 
			};
		}
		#endregion

		#region Static methods
		internal static DelegateMessageHandler CreateInstance() {
			return VirtualMachine.MemoryManager.Allocate<DelegateMessageHandler>( ARGUMENT_COUNT_OFFSET + 1 );
		}
		#endregion
	}

	public static class ExtDelegateMessageHandler {
		public static VisibilityModifier Visibility( this Handle<DelegateMessageHandler> obj ) {
			return (VisibilityModifier) (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.VISIBILITY_MASK);
		}

		public static bool IsExternal( this Handle<DelegateMessageHandler> obj ) { return true; }

		public static String Name( this Handle<DelegateMessageHandler> obj ) {
			return String.GetString( obj[MessageHandlerBaseConsts.HEADER_OFFSET] >> MessageHandlerBaseConsts.NAME_RSHIFT );
		}

		public static Class Class( this Handle<DelegateMessageHandler> obj ) {
			return (Class) obj[MessageHandlerBaseConsts.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<DelegateMessageHandler> obj ) {
			return (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.IS_ENTRYPOINT_MASK) != 0;
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

		static void SetHeader( this Handle<DelegateMessageHandler> obj, Handle<String> name, bool isEntrypoint, VisibilityModifier visibility ) {
			if (name == null && visibility != VisibilityModifier.None)
				throw new InvalidVMProgramException( "Non-default message handler specified with no name.".ToVMString().ToHandle() );
			if (name != null && visibility == VisibilityModifier.None)
				throw new InvalidVMProgramException( "Default message handler specified with name.".ToVMString().ToHandle() );
			if (isEntrypoint && visibility == VisibilityModifier.None)
				throw new InvalidVMProgramException( "Default message handler can not be entrypoint.".ToVMString().ToHandle() );

			obj[MessageHandlerBaseConsts.HEADER_OFFSET] = (name.Value.GetInternIndex() << MessageHandlerBaseConsts.NAME_RSHIFT) | (isEntrypoint ? MessageHandlerBaseConsts.IS_ENTRYPOINT_MASK : (Word) 0) | MessageHandlerBaseConsts.IS_EXTERNAL_MASK | (int) visibility;
		}

		public static void InitInstance( this Handle<DelegateMessageHandler> obj, Handle<String> name, VisibilityModifier visibility, Handle<Class> cls, bool isEntrypoint, int argumentsCount, Handle<String> externalName ) {
			obj.SetHeader( name, isEntrypoint, visibility );
			obj[MessageHandlerBaseConsts.CLASS_POINTER_OFFSET] = cls;
			obj[DelegateMessageHandler.EXTERNAL_NAME_OFFSET] = externalName;
			obj[DelegateMessageHandler.ARGUMENT_COUNT_OFFSET] = argumentsCount;
		}
	}
}

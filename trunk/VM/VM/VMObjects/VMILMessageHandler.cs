using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class VMILMessageHandlerConsts {
		public const int COUNTS_OFFSET = 3;
		public const int INSTRUCTIONS_OFFSET = 4;

		public const int INSTRUCTION_COUNT_RSHIFT = 16;
		public static readonly Word ARGUMENT_COUNT_MASK = 0x0000FF00;
		public const int ARGUMENT_COUNT_RSHIFT = 8;
		public static readonly Word LOCAL_COUNT_MASK = 0x000000FF;
	}

	public struct VMILMessageHandler : IVMObject<VMILMessageHandler> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.SystemReflectionMessageHandler; } }
		#endregion

		#region Cons
		public VMILMessageHandler( int start ) {
			this.start = start;
		}

		public VMILMessageHandler New( int startPosition ) {
			return new VMILMessageHandler( startPosition );
		}
		#endregion

		#region Static methods
		public static Handle<VMILMessageHandler> CreateInstance( int instructionCount ) {
			return VirtualMachine.MemoryManager.Allocate<VMILMessageHandler>( VMILMessageHandlerConsts.INSTRUCTIONS_OFFSET + instructionCount );
		}
		#endregion

		#region Casts
		public static implicit operator int( VMILMessageHandler v ) {
			return v.start;
		}

		public static explicit operator VMILMessageHandler( int v ) {
			return new VMILMessageHandler { start = v };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtVMILMessageHandler.ToString( this );
		}
		#endregion
	}

	public static class ExtVMILMessageHandler {
		public static bool IsExternal( this Handle<VMILMessageHandler> obj ) { return false; }

		public static VisibilityModifier Visibility( this Handle<VMILMessageHandler> obj ) {
			return (VisibilityModifier) (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.VISIBILITY_MASK);
		}

		public static Handle<String> Name( this Handle<VMILMessageHandler> obj ) {
			return String.GetString( obj[MessageHandlerBaseConsts.HEADER_OFFSET] >> MessageHandlerBaseConsts.NAME_RSHIFT );
		}

		public static int ArgumentCount( this Handle<VMILMessageHandler> obj ) {
			return (obj[VMILMessageHandlerConsts.COUNTS_OFFSET] & VMILMessageHandlerConsts.ARGUMENT_COUNT_MASK) >> VMILMessageHandlerConsts.ARGUMENT_COUNT_RSHIFT;
		}

		public static int LocalCount( this Handle<VMILMessageHandler> obj ) {
			return obj[VMILMessageHandlerConsts.COUNTS_OFFSET] & VMILMessageHandlerConsts.LOCAL_COUNT_MASK;
		}

		public static int InstructionCount( this Handle<VMILMessageHandler> obj ) {
			return obj[VMILMessageHandlerConsts.COUNTS_OFFSET] >> VMILMessageHandlerConsts.INSTRUCTION_COUNT_RSHIFT;
		}

		public static Handle<Class> Class( this Handle<VMILMessageHandler> obj ) {
			return (Class) obj[MessageHandlerBaseConsts.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<VMILMessageHandler> obj ) {
			return (obj[MessageHandlerBaseConsts.HEADER_OFFSET] & MessageHandlerBaseConsts.IS_ENTRYPOINT_MASK) != 0;
		}

		public static Word GetInstruction( this Handle<VMILMessageHandler> obj, int instruction ) {
			return obj[VMILMessageHandlerConsts.INSTRUCTIONS_OFFSET + instruction];
		}

		public static string ToString( this Handle<VMILMessageHandler> obj ) {
			if (obj == null)
				return "{NULL}";
			return ".handler " + obj.Visibility().ToString().ToLower() + " " + obj.Name() + "(" + obj.ArgumentCount() + ")";
		}

		static void SetCounts( this Handle<VMILMessageHandler> obj, int argumentCount, int localCount, int instructionCount ) {
			if (argumentCount > 0x000000FF)
				throw new InvalidVMProgramException( "Message handler has specifies more than 255 arguments." );
			if (localCount > 0x000000FF)
				throw new InvalidVMProgramException( "Message handler has specifies more than 255 local variables." );
			if (instructionCount > 0x0000FFFF)
				throw new InvalidVMProgramException( "Message handler has specifies more than 65535 local variables." );

			obj[VMILMessageHandlerConsts.COUNTS_OFFSET] =
				(instructionCount << VMILMessageHandlerConsts.INSTRUCTION_COUNT_RSHIFT) |
				(argumentCount << VMILMessageHandlerConsts.ARGUMENT_COUNT_RSHIFT) | localCount;
		}

		static void SetHeader( this Handle<VMILMessageHandler> obj, Handle<String> name, bool isEntrypoint, VisibilityModifier visibility ) {
			if (name == null && visibility != VisibilityModifier.None)
				throw new InvalidVMProgramException( "Non-default message handler specified with no name." );
			if (name != null && visibility == VisibilityModifier.None)
				throw new InvalidVMProgramException( "Default message handler specified with name." );
			if (isEntrypoint && visibility == VisibilityModifier.None)
				throw new InvalidVMProgramException( "Default message handler can not be entrypoint." );

			obj[MessageHandlerBaseConsts.HEADER_OFFSET] = (name.GetInternIndex() << MessageHandlerBaseConsts.NAME_RSHIFT) | (isEntrypoint ? MessageHandlerBaseConsts.IS_ENTRYPOINT_MASK : (Word) 0) | (int) visibility;
		}

		internal static void InitInstance( this Handle<VMILMessageHandler> obj, Handle<String> name, VisibilityModifier visibility, Handle<Class> cls, bool isEntrypoint, int argumentsCount, int localCount, IList<Word> instructions ) {
			obj.SetHeader( name, isEntrypoint, visibility );
			obj.SetCounts( argumentsCount, localCount, instructions.Count );
			obj[MessageHandlerBaseConsts.CLASS_POINTER_OFFSET] = cls.Start;

			instructions.ForEach( ( ins, idx ) => obj[VMILMessageHandlerConsts.INSTRUCTIONS_OFFSET + idx] = ins );
		}
	}
}

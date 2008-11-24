using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct VMILMessageHandler : IVMObject<VMILMessageHandler> {
		#region Constants
		public const int COUNTS_OFFSET = 3;
		public const int INSTRUCTIONS_OFFSET = 4;

		public const int ARGUMENT_COUNT_RSHIFT = 16;
		public static readonly Word LOCAL_COUNT_MASK = 0x0000FFFF;
		#endregion

		#region Properties
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.VMILMessageHandler; } }
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
			return VirtualMachine.MemoryManager.Allocate<VMILMessageHandler>( INSTRUCTIONS_OFFSET + instructionCount );
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
		#endregion
	}

	public static class ExtVMILMessageHandler {
		public static bool IsInternal( this Handle<VMILMessageHandler> obj ) { return false; }

		public static VisibilityModifier Visibility( this Handle<VMILMessageHandler> obj ) {
			return (VisibilityModifier) (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.VISIBILITY_MASK);
		}

		public static Handle<String> Name( this Handle<VMILMessageHandler> obj ) {
			return VirtualMachine.ConstantPool.GetString( obj[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT );
		}

		public static int ArgumentCount( this Handle<VMILMessageHandler> obj ) {
			return obj[VMILMessageHandler.COUNTS_OFFSET] >> VMILMessageHandler.ARGUMENT_COUNT_RSHIFT;
		}

		public static int LocalCount( this Handle<VMILMessageHandler> obj ) {
			return obj[VMILMessageHandler.COUNTS_OFFSET] & VMILMessageHandler.LOCAL_COUNT_MASK;
		}

		public static int InstructionCount( this Handle<VMILMessageHandler> obj ) {
			return obj.Size() - VMILMessageHandler.INSTRUCTIONS_OFFSET;
		}

		public static Handle<Class> Class( this Handle<VMILMessageHandler> obj ) {
			return (Class) obj[MessageHandlerBase.CLASS_POINTER_OFFSET];
		}

		public static bool IsEntrypoint( this Handle<VMILMessageHandler> obj ) {
			return (obj[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0;
		}

		public static Word GetInstruction( this Handle<VMILMessageHandler> obj, int instruction ) {
			return obj[VMILMessageHandler.INSTRUCTIONS_OFFSET + instruction];
		}

		public static string ToString( this Handle<VMILMessageHandler> obj ) {
			if (obj == null)
				return "{NULL}";
			return ".handler " + obj.Visibility().ToString().ToLower() + " " + obj.Name() + "(" + obj.ArgumentCount() + ")";
		}
	}
}

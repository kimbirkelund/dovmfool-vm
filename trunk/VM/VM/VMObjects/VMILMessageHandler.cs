using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct VMILMessageHandler : IVMObject {
		#region Constants
		public const int COUNTS_OFFSET = 3;
		public const int INSTRUCTIONS_OFFSET = 4;

		public static readonly Word ARGUMENT_COUNT_MASK = 0xFFFF0000;
		public const int ARGUMENT_COUNT_RSHIFT = 16;
		public static readonly Word LOCAL_COUNT_MASK = 0x0000FFFF;
		#endregion

		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.VMILMessageHandler; } }
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
		public static bool IsInternal { get { return false; } }
		public String Name { get { return (String) (this[MessageHandlerBase.HEADER_OFFSET] >> MessageHandlerBase.NAME_RSHIFT); } }
		public int ArgumentCount { get { return (this[VMILMessageHandler.COUNTS_OFFSET] & VMILMessageHandler.ARGUMENT_COUNT_MASK) >> VMILMessageHandler.ARGUMENT_COUNT_RSHIFT; } }
		public int LocalCount { get { return this[VMILMessageHandler.COUNTS_OFFSET] & VMILMessageHandler.LOCAL_COUNT_MASK; } }
		public int InstructionCount { get { return this.Size - VMILMessageHandler.INSTRUCTIONS_OFFSET; } }
		public Class Class { get { return (Class) this[MessageHandlerBase.CLASS_POINTER_OFFSET]; } }
		public bool IsEntrypoint { get { return (this[MessageHandlerBase.HEADER_OFFSET] & MessageHandlerBase.IS_ENTRYPOINT_MASK )!= 0; } }
		#endregion

		#region Static methods
		public static VMILMessageHandler New( int instructionCount ) {
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
		public Word GetInstruction( int instruction ) {
			return this[VMILMessageHandler.INSTRUCTIONS_OFFSET - 1 + instruction];
		}
		#endregion
	}
}

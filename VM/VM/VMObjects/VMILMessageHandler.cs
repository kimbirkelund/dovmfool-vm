using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct VMILMessageHandler : IVMObject {
		public const int COUNTS_OFFSET = 2;
		public const int INSTRUCTIONS_OFFSET = 3;

		public static readonly Word ARGUMENT_COUNT_MASK = 0xFFFF0000;
		public const int ARGUMENT_COUNT_RSHIFT = 16;
		public static readonly Word LOCAL_COUNT_MASK = 0x0000FFFF;

		public const int TypeId = 6;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( VMILMessageHandler v ) {
			return v.start;
		}

		public static implicit operator VMILMessageHandler( int v ) {
			return new VMILMessageHandler { start = v };
		}
	}

	public static class ExtVMILMessageHandler {
		public static int ArgumentCount( this VMILMessageHandler handler ) {
			return (handler.Get( VMILMessageHandler.COUNTS_OFFSET ) & VMILMessageHandler.ARGUMENT_COUNT_MASK) >> VMILMessageHandler.ARGUMENT_COUNT_RSHIFT;
		}

		public static int LocalCount( this VMILMessageHandler handler ) {
			return handler.Get( VMILMessageHandler.COUNTS_OFFSET ) & VMILMessageHandler.LOCAL_COUNT_MASK;
		}

		public static int InstructionCount( this VMILMessageHandler handler ) {
			return handler.Size() - VMILMessageHandler.INSTRUCTIONS_OFFSET;
		}

		public static Word Instruction( this VMILMessageHandler handler, int instruction ) {
			return handler.Get( VMILMessageHandler.INSTRUCTIONS_OFFSET + instruction );
		}
	}
}

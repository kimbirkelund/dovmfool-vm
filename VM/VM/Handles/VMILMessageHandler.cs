using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct VMILMessageHandler {
		public const uint COUNTS_OFFSET = 2;
		public const uint INSTRUCTIONS_OFFSET = 3;

		public const uint ARGUMENT_COUNT_MASK = 0xFFFF0000;
		public const int ARGUMENT_COUNT_RSHIFT = 16;
		public const uint LOCAL_COUNT_MASK = 0x0000FFFF;

		public const uint TypeId = 6;

		uint start;

		public static implicit operator uint( VMILMessageHandler v ) {
			return v.start;
		}

		public static implicit operator VMILMessageHandler( uint v ) {
			return new VMILMessageHandler { start = v };
		}
	}

	public static class ExtVMILMessageHandler {
		public static uint ArgumentCount( this VMILMessageHandler handler ) {
			return (handler.Get( VMILMessageHandler.COUNTS_OFFSET ) & VMILMessageHandler.ARGUMENT_COUNT_MASK) >> VMILMessageHandler.ARGUMENT_COUNT_RSHIFT;
		}

		public static uint LocalCount( this VMILMessageHandler handler ) {
			return handler.Get( VMILMessageHandler.COUNTS_OFFSET ) & VMILMessageHandler.LOCAL_COUNT_MASK;
		}

		public static uint InstructionCount( this VMILMessageHandler handler ) {
			return handler.Size() - VMILMessageHandler.INSTRUCTIONS_OFFSET;
		}

		public static IEnumerable<uint> Instructions( this VMILMessageHandler handler ) {
			return handler.Instructions( 0 );
		}

		public static IEnumerable<uint> Instructions( this VMILMessageHandler handler, uint firstInstruction ) {
			for (uint i = VMILMessageHandler.INSTRUCTIONS_OFFSET + firstInstruction; i < VMILMessageHandler.INSTRUCTIONS_OFFSET + handler.InstructionCount(); i++)
				yield return handler.Get( i );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class Instruction {
		public readonly OpCode OpCode;
		public readonly object Operand;

		public Instruction( OpCode opCode, object operand ) {
			this.OpCode = opCode;
			this.Operand = operand;
		}

		public Instruction( OpCode opCode )
			: this( opCode, null ) {
		}
	}
}

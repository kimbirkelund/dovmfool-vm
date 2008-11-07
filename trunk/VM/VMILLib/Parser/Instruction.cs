using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class Instruction : ASTNode {
		public readonly OpCode OpCode;
		public readonly object Operand;

		public Instruction( LexLocation location, OpCode opCode, object operand )
			: base( location ) {
			this.OpCode = opCode;
			this.Operand = operand;
		}

		public Instruction( LexLocation location, OpCode opCode ) : this( location, opCode, null ) { }
	}
}

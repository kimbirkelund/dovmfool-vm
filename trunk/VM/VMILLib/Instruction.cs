using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class Instruction {
		public readonly SourcePosition Position;
		public readonly OpCode OpCode;
		public readonly object Operand;

		VMILMessageHandler messageHandler;
		public VMILMessageHandler MessageHandler {
			get { return messageHandler; }
			internal set {
				if (messageHandler != null)
					throw new InvalidOperationException( "Message handler already set." );
				this.messageHandler = value;
			}
		}

		public Instruction( SourcePosition position, OpCode opCode, object operand ) {
			this.Position = position;
			this.OpCode = opCode;
			this.Operand = operand;
		}

		public Instruction( SourcePosition position, OpCode opCode )
			: this( position, opCode, null ) {
		}

		public override string ToString() {
			return OpCode.ToString() + (Operand != null ? " " + (Operand is string ? "\"" + Operand + "\"" : Operand) : "");
		}
	}
}

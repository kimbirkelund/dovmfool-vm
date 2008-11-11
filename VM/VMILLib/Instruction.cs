using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class Instruction {
		public readonly OpCode OpCode;
		public readonly object Operand;

		MessageHandler messageHandler;
		public MessageHandler MessageHandler {
			get { return messageHandler; }
			internal set {
				if (messageHandler != null)
					throw new InvalidOperationException( "Message handler already set." );
				this.messageHandler = value;
			}
		}

		public Instruction( OpCode opCode, object operand ) {
			this.OpCode = opCode;
			this.Operand = operand;
		}

		public Instruction( OpCode opCode )
			: this( opCode, null ) {
		}

		public override string ToString() {
			return OpCode.ToString() + (Operand != null ? " " + (Operand is CString ? "\"" + Operand + "\"" : Operand) : "");
		}
	}
}

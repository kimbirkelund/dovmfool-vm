using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class VMILMessageHandler : MessageHandlerBase {
		public readonly IList<string> Locals;
		public readonly InstructionList Instructions;
		bool isEntrypoint;
		public override bool IsEntrypoint { get { return isEntrypoint; } }
		public override bool IsExternal { get { return false; } }

		public VMILMessageHandler( VisibilityModifier visibility, string name, IEnumerable<string> arguments, IEnumerable<string> locals, InstructionList instructions, bool isEntrypoint )
			: base( visibility, name, arguments ) {
			this.Instructions = instructions;
			this.Locals = locals.ToList().AsReadOnly();
			this.Instructions.ForEach( i => i.MessageHandler = this );
			this.isEntrypoint = isEntrypoint;
		}

		public override string ToString() {
			return Name == null ? "Default handler" : Visibility.ToString().ToLower() + " " + Name + "(" + Arguments.Join( ", " ) + ")";
		}
	}
}

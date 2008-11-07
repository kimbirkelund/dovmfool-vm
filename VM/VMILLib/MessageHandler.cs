using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class MessageHandler {
		public readonly VisibilityModifier Visibility;
		public readonly CString Name;
		public readonly NameList Arguments;
		public readonly NameList Locals;
		public readonly InstructionList Instructions;

		public MessageHandler( VisibilityModifier visibility, CString name, NameList arguments, NameList locals, InstructionList instructions ) {
			this.Visibility = visibility;
			this.Name = name;
			this.Instructions = instructions;
			this.Arguments = arguments;
			this.Locals = locals;
		}
	}
}

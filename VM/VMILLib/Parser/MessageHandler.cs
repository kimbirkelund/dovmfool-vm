using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class MessageHandler : ASTNode {
		public readonly VisibilityModifier Visibility;
		public readonly string Name;
		public readonly List<string> Arguments;
		public readonly List<string> Locals;
		public readonly List<Instruction> Instructions;

		public MessageHandler( LexLocation location, VisibilityModifier visibility, string name, List<string> arguments, List<string> locals, List<Instruction> instructions )
			: base( location ) {
			this.Visibility = visibility;
			this.Name = name;
			this.Instructions = instructions;
			this.Arguments = arguments;
			this.Locals = locals;
		}
	}
}

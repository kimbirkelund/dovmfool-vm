using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class VMILMessageHandler : MessageHandlerBase {
		public readonly List<string> Locals;
		public readonly List<Instruction> Instructions;
		public readonly bool IsEntrypoint;

		public VMILMessageHandler( LexLocation location, VisibilityModifier visibility, string name, List<string> arguments, List<string> locals, List<Instruction> instructions, bool isEntrypoint )
			: base( location, visibility, name, arguments ) {
			this.Instructions = instructions;
			this.Locals = locals;
			this.IsEntrypoint = isEntrypoint;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Label : Instruction {
		public readonly string Name;

		public Label( SourcePosition position, string name )
			: base(position, OpCode.None, null ) {
			this.Name = name;
		}

		public override string ToString() {
			return "Label: " + Name;
		}
	}
}

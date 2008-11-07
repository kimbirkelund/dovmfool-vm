using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class Label : Instruction {
		public readonly string Name;

		public Label( LexLocation location, string name )
			: base( location, 0 ) {
			this.Name = name;
		}
	}
}

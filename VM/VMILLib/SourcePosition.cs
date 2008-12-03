using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class SourcePosition {
		public readonly int Line;
		public readonly int Column;
		public readonly string File;

		public SourcePosition( string file, int line, int column ) {
			this.File = file;
			this.Line = line;
			this.Column = column;
		}

		public override string ToString() {
			return (File ?? "") + "[" + Line + ":" + Column + "]";
		}
	}
}

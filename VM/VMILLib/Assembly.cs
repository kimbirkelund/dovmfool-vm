using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Assembly {
		public readonly CStringPool Strings;
		public readonly ClassList Classes;

		public Assembly( CStringPool strings, ClassList classes ) {
			this.Strings = strings;
			this.Classes = classes;
		}
	}
}

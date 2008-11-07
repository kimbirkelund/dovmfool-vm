using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Assembly {
		public readonly CStringPool Strings;
		public readonly CIntegerPool Integers;
		public readonly ClassList Classes;

		public Assembly( CStringPool strings, CIntegerPool integers, ClassList classes ) {
			this.Strings = strings;
			this.Integers = integers;
			this.Classes = classes;
		}
	}
}

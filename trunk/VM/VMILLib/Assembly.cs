using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Assembly {
		public readonly ClassList Classes;

		public Assembly( ClassList classes ) {
			this.Classes = classes;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public struct Field {
		public readonly Class Class;
		public readonly int Slot;

		public Field( Class cls, int slot ) {
			Class = cls;
			Slot = slot;
		}
	}
}

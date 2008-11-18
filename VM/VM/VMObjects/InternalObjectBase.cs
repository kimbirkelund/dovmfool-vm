using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using VMILLib;

namespace VM.VMObjects {
	public struct InternalObjectBase {
		public const uint SIZE_MASK = 0xFFFFFFF0;
		public const int SIZE_RSHIFT = 4;
	}
}

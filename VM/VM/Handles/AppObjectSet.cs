using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	class AppObjectSet {
		public const uint OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT_RSHIFT = 4;

		public const uint TypeId = 1;
	}
}

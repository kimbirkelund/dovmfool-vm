using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	class AppObject {
		public const uint OBJ_HEADER_APPOBJECT_CLASS = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECT_CLASS_RSHIFT = 4;

		public const uint TypeId = 0;
	}
}

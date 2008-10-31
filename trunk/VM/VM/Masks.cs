using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	static class Masks {
		public const uint OBJ_HEADER_OBJECT_TYPE = 0x00000100;

		public const uint OBJ_HEADER_APPOBJECT_CLASS = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECT_CLASS_RSHIFT = 4;

		public const uint OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT = 0xFFFFFFF0;
		public const int OBJ_HEADER_APPOBJECTSET_OBJECT_COUNT_RSHIFT = 4;

		public const uint OBJ_HEADER_INTERNAL_SIZE = 0xFFFFFF80;
		public const int OBJ_HEADER_INTERNAL_SIZE_RSHIFT = 7;

		public const uint OBJ_HEADER_INTERNAL_TYPE = 0x00000070;
		public const int OBJ_HEADER_INTERNAL_TYPE_RSHIFT = 4;
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public static class VirtualMachine {
		internal static MemoryManagerBase MemoryManager { get; private set; }
		internal static ConstantPool ConstantPool { get; private set; }
		internal static Class StringClass { get; private set; }
		internal static Class IntegerClass { get; private set; }

		internal static VMObjects.Class ResolveClass( VMObjects.String className ) {
			return 0;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public static class VirtualMachine {
		internal static MemoryManagerBase MemoryManager { get; private set; }
		internal static ConstantPool ConstantPool { get; private set; }

		internal static Handles.Class ResolveClass( Handles.String className ) {
			return 0;
		}
	}
}

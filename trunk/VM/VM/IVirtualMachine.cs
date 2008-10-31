using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public interface IVirtualMachine {
		MemoryManagerBase MemoryManager { get; }
		Handles.ClassManager ClassManager { get; }
		Handles.ConstantPool ConstantPool { get; }
	}
}

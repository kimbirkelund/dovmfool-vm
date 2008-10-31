using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public abstract class HandleBase {
		protected MemoryManagerBase memoryManager;
		public MemoryManagerBase MemeoryManager { get { return memoryManager; } }

		protected uint start;
		public uint Start { get { return start; } }

		public IVirtualMachine VirtualMachine { get { return memoryManager.VirtualMachine; } }

		internal uint this[uint index] {
			get { return memoryManager[start + index]; }
			set { memoryManager[start + index] = value; }
		}

		protected HandleBase( MemoryManagerBase memoryManager, uint start ) {
			this.memoryManager = memoryManager;
			this.start = start;
		}
	}
}

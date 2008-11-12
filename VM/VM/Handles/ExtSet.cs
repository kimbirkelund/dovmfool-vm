using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public static class ExtSet {
		public static void Set( this ObjectBase objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this AppObject objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this AppObjectSet objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this Class objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this ClassManager objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this MessageHandlerBase objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this VMILMessageHandler objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this DelegateMessageHandler objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this String objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this Integer objectBase, uint offset, uint value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}
	}
}

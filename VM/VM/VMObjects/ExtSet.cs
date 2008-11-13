using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public static class ExtSet {
		public static void Set( this ObjectBase objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this AppObject objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this AppObjectSet objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this Class objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this ClassManager objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this MessageHandlerBase objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this VMILMessageHandler objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this DelegateMessageHandler objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this String objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}

		public static void Set( this Integer objectBase, int offset, Word value ) {
			VirtualMachine.MemoryManager[objectBase + offset] = value;
		}
	}
}

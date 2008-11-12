using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public static class ExtGet {
		public static uint Get( this ObjectBase objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this AppObject objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this AppObjectSet objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this Class objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this ClassManager objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this MessageHandlerBase objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this VMILMessageHandler objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this DelegateMessageHandler objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this String objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static uint Get( this Integer objectBase, uint offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}
	}
}

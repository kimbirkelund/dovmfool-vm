using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public static class ExtGet {
		public static Word Get( this ObjectBase objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this AppObject objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this AppObjectSet objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this Class objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this ClassManager objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this MessageHandlerBase objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this VMILMessageHandler objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this DelegateMessageHandler objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this String objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}

		public static Word Get( this Integer objectBase, int offset ) {
			return VirtualMachine.MemoryManager[objectBase + offset];
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract class MemoryManagerBase : IEnumerable<Handle<VMObject>> {
		public abstract IVirtualMachine VirtualMachine { get; }
		public abstract uint SizeInWords { get; }
		public abstract uint FreeSizeInWords { get; }
		public abstract uint AllocatedSizeInWords { get; }
		public abstract IEnumerator<Handle<VMObject>> GetEnumerator();

		internal abstract Handle<T> Allocate<T>( uint size ) where T : VMObject;
		internal abstract Handle<T> Retrieve<T>( uint position ) where T : VMObject;
		internal abstract uint this[uint index] { get; set; }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	}
}

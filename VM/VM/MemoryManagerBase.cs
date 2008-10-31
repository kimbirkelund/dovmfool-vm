using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract class MemoryManagerBase : IEnumerable<Handles.ObjectBase> {
		public abstract IVirtualMachine VirtualMachine { get; }
		public abstract uint SizeInWords { get; }
		public abstract uint FreeSizeInWords { get; }
		public abstract uint AllocatedSizeInWords { get; }
		public abstract IEnumerator<Handles.ObjectBase> GetEnumerator();

		internal abstract T Allocate<T>( uint size ) where T : Handles.ObjectBase;
		internal abstract T Retrieve<T>( uint position ) where T : Handles.ObjectBase;
		internal abstract uint this[uint index] { get; set; }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	}
}

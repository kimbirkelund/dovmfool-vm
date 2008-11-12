using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract class MemoryManagerBase : IEnumerable<Handle> {
		public abstract uint SizeInWords { get; }
		public abstract uint FreeSizeInWords { get; }
		public abstract uint AllocatedSizeInWords { get; }
		public abstract IEnumerator<Handle> GetEnumerator();

		internal abstract uint Allocate( uint size );
		internal abstract uint Retrieve( uint position );
		internal abstract uint this[uint index] { get; set; }

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return this.GetEnumerator(); }
	}
}

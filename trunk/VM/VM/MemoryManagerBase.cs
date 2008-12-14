using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using System.Threading;

namespace VM {
	public abstract partial class MemoryManagerBase {
		public abstract int SizeInWords { get; }
		public abstract int FreeSizeInWords { get; }
		public abstract int AllocatedSizeInWords { get; }

		internal abstract T Allocate<T>( int size ) where T : struct, IVMObject<T>;
		internal virtual T Allocate<T>( int size, bool writeZeroes ) where T : struct, IVMObject<T> {
			var obj = Allocate<T>( size );
			if (writeZeroes)
				for (var i = 0; i < size; i++)
					obj[i] = 0;

			return obj;
		}
		protected abstract void RelocateHere( int obj, int size );

		internal abstract void NewMemory( Word[] memory, int start, int size );
	}
}

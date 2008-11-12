using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract class Handle : IEnumerable<uint> {
		protected uint[] memory;
		/// <summary>
		/// For a given VM configuration the start position must uniquely identify the
		/// position in the heap regardless of the number of memory managers.
		/// </summary>
		protected uint start;
		public uint Start { get { return start; } }
		protected uint size;
		public uint Size { get { return size; } }

		public uint this[uint index] {
			get {
				if (index < size)
					return memory[start + size];
				else
					throw new ArgumentOutOfRangeException( "Index must satisfy start <= index < start + size." );
			}
			set {
				if (index < size)
					memory[start + index] = value;
				else
					throw new ArgumentOutOfRangeException( "Index must satisfy start <= index < start + size." );
			}
		}

		public IEnumerator<uint> GetEnumerator() {
			for (uint i = 0; i < size; i++)
				yield return this[i];
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

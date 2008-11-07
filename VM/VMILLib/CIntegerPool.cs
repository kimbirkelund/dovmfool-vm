using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class CIntegerPool : IEnumerable<CInteger> {
		List<CInteger> integers;

		public readonly int Count;

		public CInteger this[int index] {
			get { return integers[index]; }
		}

		public CIntegerPool( IEnumerable<CInteger> integers ) {
			this.integers = new List<CInteger>( integers.OrderBy( c => c.Index ) );
			this.Count = this.integers.Count;
		}

		public CIntegerPool( IEnumerable<int> integers )
			: this( integers.Select( ( s, i ) => new CInteger( i, s ) ) ) { }

		public IEnumerator<CInteger> GetEnumerator() {
			return integers.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class CStringPool : IEnumerable<CString> {
		List<CString> strings;

		public readonly int Count;

		public CString this[int index] {
			get { return strings[index]; }
		}

		public CStringPool( IEnumerable<CString> strings ) {
			this.strings = new List<CString>( strings.OrderBy( c => c.Index ) );
			this.Count = this.strings.Count;
		}

		public CStringPool( IEnumerable<string> strings )
			: this( strings.Select( ( s, i ) => new CString( i, s ) ) ) { }

		public IEnumerator<CString> GetEnumerator() {
			return strings.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

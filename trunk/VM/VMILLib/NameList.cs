using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class NameList : IEnumerable<CString> {
		public readonly int Count;
		List<CString> vars;

		public CString this[int index] {
			get { return vars[index]; }
		}

		public NameList( IEnumerable<CString> fields ) {
			this.vars = fields.ToList();
			this.Count = this.vars.Count;
		}

		public int IndexOf( CString var ) {
			return vars.IndexOf( var );
		}

		public IEnumerator<CString> GetEnumerator() {
			return vars.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

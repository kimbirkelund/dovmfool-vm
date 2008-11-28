using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class NameList : IEnumerable<string> {
		public readonly int Count;
		List<string> vars;

		public string this[int index] {
			get { return vars[index]; }
		}

		public NameList( IEnumerable<string> fields ) {
			this.vars = fields.ToList();
			this.Count = this.vars.Count;
		}

		public int IndexOf( string var ) {
			return vars.IndexOf( var );
		}

		public IEnumerator<string> GetEnumerator() {
			return vars.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

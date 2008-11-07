using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class ClassList : IEnumerable<Class> {
		public readonly int Count;
		Dictionary<CString, Class> classes;

		public Class this[CString name] {
			get {
				if (classes.ContainsKey( name ))
					return classes[name];
				return null;
			}
		}

		public ClassList( IEnumerable<Class> classes ) {
			this.classes = classes.ToDictionary( c => c.Name );
			this.Count = this.classes.Count;
		}

		public IEnumerator<Class> GetEnumerator() {
			return classes.Values.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

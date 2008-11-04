using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILAssembler {
	class List<T> : ASTNode, IEnumerable<T> where T : ASTNode {
		T[] elements;

		public int Count { get; private set; }

		public List( params T[] elements )
			: base( new LexLocation() ) {
			this.elements = new T[elements.Length];
			elements.CopyTo( this.elements, 0 );
			this.Count = elements.Length;
		}

		List( T[] elements, bool intern )
			: base( new LexLocation() ) {
			this.Count = elements.Length;
			this.elements = elements;
		}

		public IEnumerator<T> GetEnumerator() {
			foreach (var elem in elements)
				yield return elem;
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public static List<T> operator +( List<T> l1, List<T> l2 ) {
			var arr = new T[l1.Count + l2.Count];
			Array.Copy( l1.elements, 0, arr, 0, l1.Count );
			Array.Copy( l2.elements, 0, arr, l1.Count, l2.Count );
			return new List<T>( arr, true );
		}

		public static List<T> operator +( T e, List<T> l ) {
			var arr = new T[l.Count + 1];
			arr[0] = e;
			Array.Copy( l.elements, 0, arr, 1, l.Count );
			return new List<T>( arr, true );
		}

		public static List<T> operator +( List<T> l, T e ) {
			var arr = new T[l.Count + 1];
			Array.Copy( l.elements, 0, arr, 0, l.Count );
			arr[l.Count] = e;
			return new List<T>( arr, true );
		}
	}
}

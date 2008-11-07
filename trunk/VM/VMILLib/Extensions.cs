using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	static class Extensions {
		public static IEnumerable<byte> ToByteStream( this IEnumerable<uint> arr ) {
			foreach (var i in arr) {
				yield return (byte) (i >> 24);
				yield return (byte) ((i >> 16) & 0x000000FF);
				yield return (byte) ((i >> 8) & 0x000000FF);
				yield return (byte) (i & 0x000000FF);
			}
		}

		public static IEnumerable<byte> ToByteStream( this IEnumerable<int> arr ) {
			foreach (var i in arr) {
				yield return (byte) (i >> 24);
				yield return (byte) ((i >> 16) & 0x000000FF);
				yield return (byte) ((i >> 8) & 0x000000FF);
				yield return (byte) (i & 0x000000FF);
			}
		}

		public static IEnumerable<uint> ToUIntStream( this IEnumerable<byte> arr ) {
			if (arr.Count() % 4 != 0)
				throw new ArgumentException( "Array length must be divisible with 4." );

			var e = arr.GetEnumerator();
			while (e.MoveNext()) {

				uint r = e.Current; e.MoveNext();
				r = (r << 8) | e.Current; e.MoveNext();
				r = (r << 8) | e.Current; e.MoveNext();
				r = (r << 8) | e.Current;
				yield return r;
			}
		}

		public static IEnumerable<int> ToIntStream( this IEnumerable<byte> arr ) {
			if (arr.Count() % 4 != 0)
				throw new ArgumentException( "Array length must be divisible with 4." );

			var e = arr.GetEnumerator();
			while (e.MoveNext()) {
				int r = e.Current; e.MoveNext();
				r = (r << 8) | e.Current; e.MoveNext();
				r = (r << 8) | e.Current; e.MoveNext();
				r = (r << 8) | e.Current;
				yield return r;
			}
		}
	}
}

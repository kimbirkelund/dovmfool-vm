using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet {
	/// <summary>
	/// Extension methods for working with sequences of bytes (and (unsigned) integers).
	/// </summary>
	public static class ByteArrays {
		/// <summary>
		/// Converts each input unsigned integer into 4 bytes.
		/// </summary>
		/// <param name="arr">The sequence of unsigned integers to convert.</param>
		/// <returns>The sequence of bytes corresponding to the input sequence.</returns>
		public static IEnumerable<byte> ToByteStream( this IEnumerable<uint> arr ) {
			foreach (var i in arr) {
				yield return (byte) (i >> 24);
				yield return (byte) ((i >> 16) & 0x000000FF);
				yield return (byte) ((i >> 8) & 0x000000FF);
				yield return (byte) (i & 0x000000FF);
			}
		}

		/// <summary>
		/// Converts each input integer into 4 bytes.
		/// </summary>
		/// <param name="arr">The sequence of integers to convert.</param>
		/// <returns>The sequence of bytes corresponding to the input sequence.</returns>
		public static IEnumerable<byte> ToByteStream( this IEnumerable<int> arr ) {
			foreach (var i in arr) {
				yield return (byte) (i >> 24);
				yield return (byte) ((i >> 16) & 0x000000FF);
				yield return (byte) ((i >> 8) & 0x000000FF);
				yield return (byte) (i & 0x000000FF);
			}
		}

		/// <summary>
		/// Converts each set of 4 input bytes into 1 unsigned integer. Sequence length must be divisible by 4.
		/// </summary>
		/// <param name="arr">The sequence of bytes to convert.</param>
		/// <returns>The sequence of unsigned integers corresponding to the input sequence.</returns>
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

		/// <summary>
		/// Converts each set of 4 input bytes into 1 integer. Sequence length must be divisible by 4.
		/// </summary>
		/// <param name="arr">The sequence of bytes to convert.</param>
		/// <returns>The sequence of integers corresponding to the input sequence.</returns>
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

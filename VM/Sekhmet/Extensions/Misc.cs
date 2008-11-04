using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;

namespace System {
    /// <summary>
    /// 
    /// </summary>
    [DebuggerStepThrough]
    public static class Misc {
        /// <summary>
        /// Determines whether the specified value is null or empty.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if the specified value is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty( this string value ) {
            return string.IsNullOrEmpty( value );
        }

        /// <summary>
        /// Performs <c>action</c> of each element of <c>sequence</c>.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <c>sequence</c>.</typeparam>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="action">The action to be performed for each element.</param>
        public static void ForEach<T>( this IEnumerable<T> sequence, Action<T> action ) {
            foreach (var value in sequence)
                action( value );
        }

        /// <summary>
        /// Performs <c>action</c> of each element of <c>sequence</c>.
        /// </summary>
        /// <param name="sequence">The sequence of elements.</param>
        /// <param name="action">The action to be performed for each element.</param>
        /// <typeparam name="TEnum">The type of the sequence, which must be less than or equals to <see cref="IEnumerable&lt;TElem&gt;"/>.</typeparam>
        /// <typeparam name="TElem">The type of the elements of the sequence.</typeparam>
        /// <returns></returns>
        public static TEnum ForEach<TEnum, TElem>( this TEnum sequence, Action<TElem> action ) where TEnum : IEnumerable<TElem> {
            foreach (var value in sequence)
                action( value );
            return sequence;
        }

        /// <summary>
        /// Computes whether <c>instance</c> is not-null and starts with the specified value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The intended prefix.</param>
        /// <returns><c>true</c> if the <c>instance</c> not-null and starts with <c>value</c>; <c>false</c> otherwise.</returns>
        public static bool StartsWith( this string instance, char value ) {
            return !instance.IsNullOrEmpty() && instance[0] == value;
        }

        /// <summary>
        /// Computes whether <c>instance</c> is not-null and ends with the specified value.
        /// </summary>
        /// <param name="instance">The instance.</param>
        /// <param name="value">The intended suffix.</param>
        /// <returns><c>true</c> if the <c>instance</c> not-null and ends with <c>value</c>; <c>false</c> otherwise.</returns>
        public static bool EndsWith( this string instance, char value ) {
            return !instance.IsNullOrEmpty() && instance[instance.Length - 1] == value;
        }

        /// <summary>
        /// Concatenates <c>sequence</c> and <c>elem</c>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="elem">The elem to be added to <c>sequence</c>.</param>
        /// <returns>An <see cref="IEnumerable&lt;T&gt;"/> containing the elements of <c>sequence</c> and the element <c>elem</c>.</returns>
        public static IEnumerable<T> Concat<T>( this IEnumerable<T> sequence, T elem ) {
            foreach (var e in sequence)
                yield return e;
            yield return elem;
        }

        /// <summary>
        /// Converts the elements of <c>sequence</c> to <see cref="T:string"/>s and concatenates them seperated by <c>seperator</c>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns>A <see cref="T:string"/> containing the string representation of each element seperated by <c>seperator</c>.</returns>
        public static string Join<T>( this IEnumerable<T> sequence, string seperator ) {
            return string.Join( seperator, sequence.Select( e => e.ToString() ).ToArray() );
        }

        /// <summary>
        /// For each element of <c>sequence</c> perform <c>elementAction</c> and between each two elements perform <c>interAction</c>.
        /// </summary>
        /// <typeparam name="T">The type of the elements.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="elementAction">The action to be performed on each element.</param>
        /// <param name="interAction">The action to be performed between two elements.</param>
        /// <remarks>
        /// Given a sequence <c>l</c> of elements and a string <c>s</c>, the following to are equivalent:
        /// 
        /// string s1 = l.Join( s );
        /// 
        /// and
        /// 
        /// StringBuilder sb = new StringBuilder();
        /// l.Join( e => sb.Append(e.ToString()), () => sb.Append(s) );
        /// string s1 = sb.ToString();
        /// </remarks>
        public static void Join<T>( this IEnumerable<T> sequence, Action<T> elementAction, Action interAction ) {
            bool isFirst = true;
            foreach (var elem in sequence) {
                if (!isFirst)
                    interAction();
                isFirst = false;
                elementAction( elem );
            }
        }

        /// <summary>
        /// Converts the sequence to an array and performs <see cref="M:System.String.Format"/> on it and <c>format</c>.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <c>sequence</c>.</typeparam>
        /// <param name="sequence">The sequence.</param>
        /// <param name="format">The format.</param>
        /// <returns>A string formatted in accordance with <c>format</c> and with the elements of <c>sequence</c> as values.</returns>
        public static string Format<T>( this IEnumerable<T> sequence, string format ) {
            return string.Format( format, sequence.ToArray() );
        }

        /// <summary>
        /// Converts the first letter to upper case.
        /// </summary>
        /// <param name="value">The value to capitalize.</param>
        /// <returns>A capitalized version of <c>value</c> or the empty string if <c>value</c> is null or empty.</returns>
        public static string Capitalize( this string value ) {
            if (string.IsNullOrEmpty( value ))
                return "";
            return value[0].ToString().ToUpper() + value.Substring( 1 );
        }

        /// <summary>
        /// Splits the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="seperator">The seperator.</param>
        /// <returns></returns>
        public static string[] Split( this string value, params string[] seperator ) {
            return value.Split( seperator, StringSplitOptions.RemoveEmptyEntries );
        }

        /// <summary>
        /// Creates an enumerable from a single object.
        /// </summary>
        /// <typeparam name="T">Type of the object.</typeparam>
        /// <param name="value">The object.</param>
        /// <returns>An enumerable containing the single object <c>value</c>.</returns>
        public static IEnumerable<T> AsEnumerable<T>( this T value ) {
            yield return value;
        }

        /// <summary>
        /// Create an empty <see cref="T:IEnumerable"/> of type <see cref="T:T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enumerable to create.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An empty <see cref="T:IEnumerable"/> of type <see cref="T:T"/></returns>
        public static IEnumerable<T> EmptyEnumerable<T>( this T value ) {
            return new T[0];
        }
    }
}

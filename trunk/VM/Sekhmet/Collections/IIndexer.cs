using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Collections {
    /// <summary>
    /// Interface for indexable objects. Makes it easier to create named indexes.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IIndexer<TKey, TValue> {
        /// <summary>
        /// Gets or sets the <see cref="T:TValue"/> with the specified name.
        /// </summary>
        /// <value>The value with the specified name.</value>
        TValue this[TKey name] { get; set; }
        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        bool IsReadOnly { get; }
    }
}

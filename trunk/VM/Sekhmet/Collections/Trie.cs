using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents a collection of strings that support quick lookup of all nodes with a given prefix.
    /// </summary>
    /// <typeparam name="TValue">The type of the values stored in the trie.</typeparam>
    public sealed partial class Trie<TValue> {
        TrieNode root;

        /// <summary>
        /// Initializes a new instance of the <see cref="Trie&lt;TValue&gt;"/> class.
        /// </summary>
        public Trie() {
            root = new Trie<TValue>.TrieNode( null, "" );
        }

        /// <summary>
        /// Inserts the specified key, value pair.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        public void Insert( string key, TValue value ) {
            this.root.Insert( key, value );
        }

        /// <summary>
        /// Finds the node corresponding to the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>The value corresponding to the specified key.</returns>
        public TValue Find( string key ) {
            return this.root.Find( key );
        }

        /// <summary>
        /// Finds all nodes whose key has the specified prefix.
        /// </summary>
        /// <param name="prefix">The prefix to search for.</param>
        /// <returns>A collection of values whose corresponding key has the specified prefix.</returns>
        public IEnumerable<TValue> FindAll( string prefix ) {
            return this.root.FindAll( prefix );
        }

#if DEBUG
        /// <summary>
        /// Prints the trie to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public void Print( System.IO.TextWriter writer ) {
            root.Print( writer, "" );
        }
#endif
    }
}

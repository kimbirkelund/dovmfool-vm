using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Collections {
    partial class Trie<TValue> {
        class TrieNode {
            public string Key { get { return (parent != null ? parent.Key : "") + keyPart; } }
            public TValue Value { get; private set; }

            string keyPart;
            TrieNode parent;
            Dictionary<char, TrieNode> children = new Dictionary<char, TrieNode>();
            bool hasValue;

            public TrieNode( TrieNode parent, string keyPart, TValue value ) {
                this.keyPart = keyPart;
                this.Value = value;
                this.hasValue = true;
                this.parent = parent;
            }

            public TrieNode( TrieNode parent, string keyPart ) {
                this.keyPart = keyPart;
                this.hasValue = false;
                this.parent = parent;
            }

            IEnumerable<TValue> GetKeyNodes() {
                var queue = new Queue<TrieNode>();
                queue.Enqueue( this );
                while (queue.Count > 0) {
                    var e = queue.Dequeue();
                    yield return e.Value;
                    e.children.Values.ForEach( c => queue.Enqueue( c ) );
                }
            }

            public TValue Find( string keyPart ) {
                if (keyPart.Length == 0)
                    return this.Value;

                if (!children.ContainsKey( keyPart[0] ))
                    return default( TValue );

                var node = children[keyPart[0]];
                if (keyPart.Length < node.keyPart.Length)
                    return default( TValue );
                if (!keyPart.StartsWith( node.keyPart ))
                    return default( TValue );

                return node.Find( keyPart.Substring( node.keyPart.Length ) );
            }

            public IEnumerable<TValue> FindAll( string keyPart ) {
                if (keyPart.Length == 0)
                    return GetKeyNodes();

                if (!children.ContainsKey( keyPart[0] ))
                    return new TValue[0];

                var node = children[keyPart[0]];
                if (keyPart.Length < node.keyPart.Length && node.keyPart.StartsWith( keyPart ))
                    return node.GetKeyNodes();
                if (!keyPart.StartsWith( node.keyPart ))
                    return new TValue[0];

                return node.FindAll( keyPart.Substring( node.keyPart.Length ) );
            }

            public void Insert( string keyPart, TValue value ) {
                if (keyPart.Length == 0) {
                    if (this.hasValue)
                        throw new ArgumentException( "Key '" + Key + "' already present in trie." );
                    this.Value = value;
                    this.hasValue = true;
                } else if (children.ContainsKey( keyPart[0] )) {
                    var node = children[keyPart[0]];
                    if (keyPart.StartsWith( node.keyPart ))
                        node.Insert( keyPart.Substring( node.keyPart.Length ), value );
                    else {
                        int commonPrefixLength = 1;
                        for (; commonPrefixLength < keyPart.Length; commonPrefixLength++)
                            if (keyPart[commonPrefixLength] != node.keyPart[commonPrefixLength])
                                break;
                        var newNode = node.Split( commonPrefixLength );
                        newNode.Insert( keyPart.Substring( newNode.keyPart.Length ), value );
                    }
                } else {
                    var newNode = new TrieNode( this, keyPart, value );
                    children.Add( keyPart[0], newNode );
                }
            }

            TrieNode Split( int index ) {
                var key1 = keyPart.Substring( 0, index );
                var key2 = keyPart.Substring( index );

                var newNode = new TrieNode( this.parent, key1 );
                newNode.children.Add( key2[0], this );
                newNode.parent.children[key1[0]] = newNode;

                this.parent = newNode;
                this.keyPart = key2;


                return newNode;
            }

#if DEBUG
            public void Print( System.IO.TextWriter writer, string indent ) {
                writer.WriteLine( indent + keyPart + (hasValue ? ": (" + Key + ", " + Value + ")" : "") );
                foreach (var child in children.Values)
                    child.Print( writer, indent + " " );
            }
#endif
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet {
    /// <summary>
    /// Helper class for managing singletons.
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public class SingletonManager<TValue> {
        Dictionary<Key, TValue> dict = new Dictionary<Key, TValue>( EqualityComparer.Instance );

        /// <summary>
        /// Stores the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="keys">The keys.</param>
        public void Store( TValue value, IEnumerable<object> keys ) {
            dict.Add( new Key( keys ), value );
        }

        /// <summary>
        /// Stores the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="keys">The keys.</param>
        public void Store( TValue value, params object[] keys ) {
            Store( value, (IEnumerable<object>) keys );
        }

        /// <summary>
        /// Retrieves the specified keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public TValue Retrieve( IEnumerable<object> keys ) {
            var key = new Key( keys );
            if (dict.ContainsKey( key ))
                return dict[key];
            return default( TValue );
        }

        /// <summary>
        /// Retrieves the specified keys.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns></returns>
        public TValue Retrieve( params object[] keys ) {
            return Retrieve( (IEnumerable<object>) keys );
        }

        class Key {
            IEnumerable<object> keys;

            public Key( IEnumerable<object> keys ) {
                this.keys = keys;
            }

            public override bool Equals( object obj ) {
                if (!(obj is Key))
                    return false;
                var key = (Key) obj;

                return keys.SequenceEqual( key.keys, EqualityComparer.Instance );
            }

            public override int GetHashCode() {
                return keys.Sum( k => (long) EqualityComparer.Instance.GetHashCode( k ) ).GetHashCode();
            }

            class EqualityComparer : IEqualityComparer<object> {
                public static readonly EqualityComparer Instance = new EqualityComparer();

                public new bool Equals( object x, object y ) {
                    return object.ReferenceEquals( x, y );
                }

                public int GetHashCode( object obj ) {
                    if (obj == null)
                        return -1;
                    return obj.GetHashCode();
                }
            }
        }

        class EqualityComparer : IEqualityComparer<Key> {
            public static readonly EqualityComparer Instance = new EqualityComparer();

            public bool Equals( Key x, Key y ) {
                return x.Equals( y );
            }

            public int GetHashCode( Key obj ) {
                return obj.GetHashCode();
            }
        }
    }
}

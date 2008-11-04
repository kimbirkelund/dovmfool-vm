using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a property of a tag.
    /// </summary>
    public struct Property {
        string name;
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get { return name; }
        }

        string value;
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public string Value {
            get { return value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> struct.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public Property( string name, string value ) {
            this.name = name;
            this.value = value;
        }

        /// <summary>
        /// Compares two properties.
        /// </summary>
        public static bool operator ==( Property prop1, Property prop2 ) {
            return prop1.Name == prop2.Name && prop1.Value == prop2.Value;
        }

        /// <summary>
        /// Compares two properties.
        /// </summary>
        public static bool operator !=( Property prop1, Property prop2 ) {
            return !(prop1 == prop2);
        }

        /// <summary>
        /// Indicates whether this instance and a specified object are equal.
        /// </summary>
        /// <param name="obj">Another object to compare to.</param>
        /// <returns>
        /// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
        /// </returns>
        public override bool Equals( object obj ) {
            if (!(obj is Property))
                return false;
            return this == ((Property) obj);
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer that is the hash code for this instance.
        /// </returns>
        public override int GetHashCode() {
            return (this.Name + this.Value).GetHashCode();
        }
    }
}

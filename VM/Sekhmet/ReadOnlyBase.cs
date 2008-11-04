using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet {
    /// <summary>
    /// Represents the base for all classes supporting a read only state.
    /// </summary>
    public abstract class ReadOnlyBase : IReadOnly {
        bool isReadOnly;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsReadOnly {
            get { return isReadOnly; }
            set {
                if (value && isReadOnly)
                    return;
                AssertReadOnly();
                isReadOnly = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReadOnlyBase"/> class.
        /// </summary>
        /// <param name="isReadOnly">if set to <c>true</c> [is read only].</param>
        protected ReadOnlyBase( bool isReadOnly ) {
            this.IsReadOnly = isReadOnly;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ReadOnlyBase"/> class.
        /// </summary>
        protected ReadOnlyBase()
            : this( false ) {
        }

        /// <summary>
        /// Asserts whether this instance is read only and throws an exception if it is.
        /// </summary>
        protected virtual void AssertReadOnly() {
            if (this.IsReadOnly)
                throw new InvalidOperationException( "Object is read only" );
        }

        void IReadOnly.AssertReadOnly() {
            this.AssertReadOnly();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet {
    /// <summary>
    /// Defines the methods and properties for all types supporting a read only state.
    /// </summary>
	public interface IReadOnly {
        /// <summary>
        /// Gets or sets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is read only; otherwise, <c>false</c>.
        /// </value>
		bool IsReadOnly {
			get;
			set;
		}

        /// <summary>
        /// Asserts whether this instance is read only and throws an exception if it is.
        /// </summary>
		void AssertReadOnly();
	}
}

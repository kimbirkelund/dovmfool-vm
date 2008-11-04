#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents the different changes a collection can inform its users of.
    /// </summary>
    public enum Change {
        /// <summary>
        /// Specifies that an item was added or is about to be added
        /// </summary>
        Add,
        /// <summary>
        /// Specifies that an item was removed or is about to be removed
        /// </summary>
        Remove,
        /// <summary>
        /// Specifies that an item has been replaced by another or is about to be replaced
        /// </summary>
        Replace,
        /// <summary>
        /// Specifies that the entire list is being emtied
        /// </summary>
        Clear,
        /// <summary>
        /// Specified that that some operation (possibly) has reordered or is about to reorder the items
        /// </summary>
        Rearrange
    }
}

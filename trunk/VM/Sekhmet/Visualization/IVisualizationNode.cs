using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents the interface for a visualization node.
    /// </summary>
    public interface IVisualizationNode {
        /// <summary>
        /// Gets the text describing this instance.
        /// </summary>
        /// <value>The textual description of this instance.</value>
        string Text { get; }
        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        /// <value>The properties of this instance.</value>
        IDictionary<string, string> Properties { get; }
        /// <summary>
        /// Gets the children of this instance.
        /// </summary>
        /// <value>The children.</value>
        IEnumerable<IVisualizationNode> Children { get; }
    }
}

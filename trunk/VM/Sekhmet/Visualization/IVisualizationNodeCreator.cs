using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents the interface for a class capable of creating visualization nodes for objects of a specific type.
    /// </summary>
    public interface IVisualizationNodeCreator {
        /// <summary>
        /// Determines whether this instance can handle the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified value; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle( object value );
        /// <summary>
        /// Creates a node for the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>An <see cref="T:IVisualizationNode"/> object.</returns>
        IVisualizationNode Create( object value );
        /// <summary>
        /// Gets or sets the <see cref="T:VisualizationManager"/> using this creator.
        /// </summary>
        /// <value>The manager.</value>
        VisualizationManager Manager { get; set; }
    }
}

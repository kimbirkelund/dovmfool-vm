using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents the interface for a class capable of visualizing <see cref="T:IVisualizationNode"/>s.
    /// </summary>
    public interface IVisualizer {
        /// <summary>
        /// Visualizes the specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        void Visualize( IEnumerable<IVisualizationNode> nodes );
    }
}

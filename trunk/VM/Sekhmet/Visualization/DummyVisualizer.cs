using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Repersents a visualizer that does nothing.
    /// </summary>
    public sealed class DummyVisualizer : IVisualizer {
        /// <summary>
        /// Visualizes the specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        public void Visualize( IEnumerable<IVisualizationNode> nodes ) {
        }
    }
}

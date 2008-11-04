using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents a default implementation of the <see cref="T:IVisualizationNode"/> interface.
    /// </summary>
    public class VisualizationNode : IVisualizationNode {
        /// <summary>
        /// Gets the text describing this instance.
        /// </summary>
        /// <value>The textual description of this instance.</value>
        public string Text { get; private set; }
        /// <summary>
        /// Gets the properties associated with this instance.
        /// </summary>
        /// <value>The properties of this instance.</value>
        public IDictionary<string, string> Properties { get; private set; }
        /// <summary>
        /// Gets the children of this instance.
        /// </summary>
        /// <value>The children.</value>
        public IEnumerable<IVisualizationNode> Children { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="children">The children.</param>
        public VisualizationNode( string text, IDictionary<string, string> properties, IEnumerable<IVisualizationNode> children ) {
            this.Text = text;
            this.Properties = properties;
            this.Children = children;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="children">The children.</param>
        public VisualizationNode( string text, IDictionary<string, string> properties, params IVisualizationNode[] children )
            : this( text, properties, (IEnumerable<IVisualizationNode>) children ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="properties">The properties.</param>
        public VisualizationNode( string text, IDictionary<string, string> properties )
            : this( text, properties, new IVisualizationNode[0] ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="children">The children.</param>
        public VisualizationNode( string text, IEnumerable<IVisualizationNode> children )
            : this( text, new Dictionary<string, string>(), children ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="children">The children.</param>
        public VisualizationNode( string text, params IVisualizationNode[] children )
            : this( text, new Dictionary<string, string>(), children ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationNode"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        public VisualizationNode( string text )
            : this( text, new Dictionary<string, string>(), new IVisualizationNode[0] ) {
        }
    }
}

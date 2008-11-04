using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents a visualizer outputting in XML format.
    /// </summary>
    public sealed class XmlVisualizer : IVisualizer {
        XDocument document = new XDocument();
        string fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlVisualizer"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public XmlVisualizer( XDocument document ) {
            this.document = document;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlVisualizer"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public XmlVisualizer( string fileName ) {
            this.fileName = fileName;
        }

        /// <summary>
        /// Visualizes the specified nodes.
        /// </summary>
        /// <param name="nodes">The nodes.</param>
        public void Visualize( IEnumerable<IVisualizationNode> nodes ) {
            document.Add( new XElement( "root", nodes.Select( n => CreateElement( n ) ) ) );
            document.Save( fileName );
        }

        XElement CreateElement( IVisualizationNode node ) {
            var elem = new XElement( "node", new XAttribute( "text", node.Text ) );
            if (node.Properties.Count != 0)
                elem.Add( new XElement( "properties", node.Properties.Select( p => new XElement( "property", new XAttribute( "name", p.Key ), new XAttribute( "value", p.Value ?? "" ) ) ) ) );
            if (node.Children.Count() != 0)
                elem.Add( new XElement( "children", node.Children.Select( c => CreateElement( c ) ) ) );

            return elem;
        }
    }
}

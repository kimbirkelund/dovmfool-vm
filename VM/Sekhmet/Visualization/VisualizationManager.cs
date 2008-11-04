using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents a manager for visualizing objects.
    /// </summary>
    public class VisualizationManager {
        /// <summary>
        /// Gets or sets the visualizer.
        /// </summary>
        /// <value>The visualizer.</value>
        public IVisualizer Visualizer { get; set; }
        /// <summary>
        /// Gets the node creators.
        /// </summary>
        /// <value>The node creators.</value>
        public IList<IVisualizationNodeCreator> NodeCreators { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationManager"/> class.
        /// </summary>
        /// <param name="visualizer">The visualizer.</param>
        public VisualizationManager( IVisualizer visualizer ) {
            this.NodeCreators = new List<IVisualizationNodeCreator>();

            this.Visualizer = visualizer ?? new DummyVisualizer();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VisualizationManager"/> class.
        /// </summary>
        public VisualizationManager()
            : this( new DummyVisualizer() ) {
        }

        /// <summary>
        /// Creates a visualization node for the specified value.
        /// </summary>
        /// <param name="value">The value to visualize.</param>
        /// <returns>An <see cref="T:IVisualizationNode"/> object represents <c>value</c>.</returns>
        public IVisualizationNode Create( object value ) {
            foreach (var c in NodeCreators.Concat( new EnumerableNodeCreator() ).Concat( new DefaultNodeCreator() ))
                if (c.CanHandle( value ))
                    return c.Create( value );
            throw new ArgumentException( "No node creator specified for objects of type: '" + value.GetType() + "'.", "value" );
        }

        /// <summary>
        /// Creates a <see cref="T:IVisualizationNodeCreator"/> from a function.
        /// </summary>
        /// <typeparam name="TType">The type of the objects to create nodes from.</typeparam>
        /// <param name="fun">The function creating the node.</param>
        /// <returns>A <see cref="T:IVisualizationNodeCreator"/> that uses the specifying function.</returns>
        public IVisualizationNodeCreator FromFunction<TType>( Func<TType, IVisualizationNode> fun ) {
            return new FunctionalNodeCreator<TType>( fun );
        }

        #region FunctionalNodeCreator
        class FunctionalNodeCreator<TType> : VisualizationNodeCreatorBase<TType> {
            Func<TType, IVisualizationNode> fun;

            public FunctionalNodeCreator( Func<TType, IVisualizationNode> fun ) {
                this.fun = fun;
            }

            public override IVisualizationNode CreateNode( TType value ) {
                return fun( (TType) value );
            }
        }
        #endregion
    }
}

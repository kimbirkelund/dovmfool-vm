using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents a base class for <see cref="T:IVisualizationNodeCreator"/> classes, to ease their construction.
    /// </summary>
    /// <typeparam name="TType">The type of the objects this instance creates nodes for.</typeparam>
    public abstract class VisualizationNodeCreatorBase<TType> : IVisualizationNodeCreator {
        /// <summary>
        /// Gets or sets the <see cref="T:VisualizationManager"/> using this creator.
        /// </summary>
        /// <value>The manager.</value>
        public virtual VisualizationManager Manager { get; set; }
        
        /// <summary>
        /// Determines whether this instance can handle the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified value; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool CanHandle( object value ) {
            return value is TType;
        }

        /// <summary>
        /// Creates a node for the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// An <see cref="T:IVisualizationNode"/> object.
        /// </returns>
        public IVisualizationNode Create( object value ) {
            if (!(value is TType))
                throw new ArgumentException( "Value must be of type TType.", "value" );

            return CreateNode( (TType) value );
        }

        /// <summary>
        /// Creates a node for the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// An <see cref="T:IVisualizationNode"/> object.
        /// </returns>
        public abstract IVisualizationNode CreateNode( TType value );
    }
}

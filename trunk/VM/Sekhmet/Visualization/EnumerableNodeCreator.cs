using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents a node creator for any object of type <see cref="T:TType"/>, which is at least <see cref="T:IEnumerable"/>.
    /// </summary>
    /// <typeparam name="TType">The type of the objects this instance creates nodes for.</typeparam>
    public class EnumerableNodeCreator<TType> : VisualizationNodeCreatorBase<TType> where TType : IEnumerable {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumerableNodeCreator&lt;TType&gt;"/> class.
        /// </summary>
        public EnumerableNodeCreator() {
        }

        /// <summary>
        /// Determines whether this instance can handle the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can handle the specified value; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanHandle( object value ) {
            return !(value is string) && base.CanHandle( value );
        }

        /// <summary>
        /// Creates the text.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual string CreateText( TType value ) {
            return value.ToString();
        }

        /// <summary>
        /// Creates the properties.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public virtual IDictionary<string, string> CreateProperties( TType value ) {
            return new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates a node for the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// An <see cref="T:IVisualizationNode"/> object.
        /// </returns>
        public override IVisualizationNode CreateNode( TType value ) {
            var text = CreateText( value );
            var props = CreateProperties( value );
            var children = value.OfType<object>().Select( o => Manager.Create( o ) ).ToList();

            return new VisualizationNode( text, props, children );
        }
    }

    /// <summary>
    /// Represents a node creator for any object of type <see cref="T:IEnumerable"/>.
    /// </summary>
    public class EnumerableNodeCreator : EnumerableNodeCreator<IEnumerable> {
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Sekhmet.Visualization {
    /// <summary>
    /// Represents the default node creator for objects.
    /// </summary>
    public sealed class DefaultNodeCreator : VisualizationNodeCreatorBase<object> {
        Type[] basicTypes = new[] { typeof( string ), typeof( bool ), typeof( char ), typeof( double ), typeof( short ), typeof( int ), typeof( long ), typeof( float ), typeof( ushort ), typeof( uint ), typeof( ulong ) };

        /// <summary>
        /// Creates a node for the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// An <see cref="T:IVisualizationNode"/> object.
        /// </returns>
        public override IVisualizationNode CreateNode( object value ) {
            if (basicTypes.Contains( value.GetType() ))
                return new VisualizationNode( value.ToString(), new Dictionary<string, string>(), new IVisualizationNode[0] );

            return new VisualizationNode( value.ToString(), CreateProperties( value ), new IVisualizationNode[0] );
        }

        IDictionary<string, string> CreateProperties( object value ) {
            var props = value.GetType().GetProperties().Where( p => p.CanRead && p.GetIndexParameters().Length == 0 && basicTypes.Contains( p.PropertyType ) );
            return props.ToDictionary( p => p.Name, p => (p.GetValue( value, null ) ?? "").ToString() );
        }
    }
}

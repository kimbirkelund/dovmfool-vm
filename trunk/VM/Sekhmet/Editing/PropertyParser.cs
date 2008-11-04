using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a common way of parsing properties specified in tags.
    /// </summary>
    public class PropertyParser {
        /// <summary>
        /// Parses the specified properties.
        /// </summary>
        /// <param name="props">The string of properties.</param>
        /// <returns>A sequence of <see cref="Property"/> objects.</returns>
        public static IEnumerable<Property> Parse( string props ) {
            props = props.Trim();
            while (props.Length != 0) {
                int nameEnd = props.IndexOf( "=" );
                int nextSpace = props.IndexOf( " " );
                if (nameEnd == -1 && nextSpace == -1) {
                    yield return new Property( props.Trim().ToLower(), "true" );
                    yield break;
                } else if (nextSpace != -1 && (nextSpace < nameEnd || nameEnd == -1)) {
                    string prop = props.Substring( 0, nextSpace );
                    props = props.Substring( nextSpace + 1 ).Trim();
                    yield return new Property( prop.Trim().ToLower(), "true" );
                } else {
                    string name = props.Substring( 0, nameEnd );
                    props = props.Substring( nameEnd + 1 ).Trim();
                    int index;
                    string value = ScanValue( props, out index );
                    props = index >= props.Length ? "" : props.Substring( index ).Trim();
                    yield return new Property( name.Trim().ToLower(), value );
                }
            }
        }

        static string ScanValue( string input, out int index ) {
            int curIndex = 0;
            char delimiter;
            if (input[curIndex] == '\'') {
                delimiter = '\'';
                curIndex++;
            } else if (input[curIndex] == '\"') {
                delimiter = '\"';
                curIndex++;
            } else
                delimiter = ' ';
            int start = curIndex;
            while (curIndex < input.Length && input[curIndex] != delimiter)
                curIndex++;
            index = curIndex + 1;
            return input.Substring( start, curIndex - start );
        }
    }
}

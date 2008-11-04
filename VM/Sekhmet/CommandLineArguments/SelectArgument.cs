using System;
using System.Linq;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents a command-line argument that can take on a small number of values.
    /// </summary>
    public class SelectArgument : ArgumentBase<object> {
        string[] values;
        Type enumeration;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="enumeration">The enumeration.</param>
        public SelectArgument( string name, Type enumeration )
            : base( "option", name ) {
            this.enumeration = enumeration;
            if (!enumeration.IsEnum)
                throw new ArgumentException( "Type must be an enumeration.", "enumeration" );

            values = enumeration.GetFields().Select( f => f.Name.ToLower() ).Where( n => n != "value__" ).ToArray();
        }

        /// <summary>
        /// Attempts to set the value of the arguments; on failing the reason is written to <c>failureReason</c>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="failureReason">The failure reason.</param>
        /// <returns>
        /// A value indicating whether setting the value was succesfully; if <c>false</c>
        /// 	<c>failureReason</c> contains the reason for the faiure.
        /// </returns>
        public override bool TrySetValue( string value, out string failureReason ) {
            int index = Array.IndexOf<string>( values, value.ToLower() );

            if (index != -1) {
                this.Value = enumeration.GetField( values[index], System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public ).GetValue( null );

                failureReason = null;
                return true;
            } else {
                string valuesStr = string.Join( ", ", values );

                failureReason = "value must be one of these values: " + valuesStr;
                return false;
            }
        }
    }
}
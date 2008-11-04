using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents a argument that takes a value satisfying a specified regular expression.
    /// </summary>
    public class RegexStringArgument : ArgumentBase<string> {
        /// <summary>
        /// Gets the regular expression the value of this argument must satisfy.
        /// </summary>
        public string Regex { get; private set; }
        Regex regexp;

        /// <summary>
        /// Initializes a new instance of the <see cref="RegexStringArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="regex">The regular expression the value of this argument must satisfy.</param>
        public RegexStringArgument( string name, string regex )
            : base( "string", name ) {
            this.Regex = regex;
            regexp = new Regex( "^" + regex + "$" );
            UsageInformation.Add( "Value specification", "The value specified for this argument must satisfy the regular expression '" + Regex + "'." );
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
            failureReason = null;

            if (value == null || !regexp.Match( value.Trim() ).Success) {
                failureReason = "Specified value doesn't satisfy regular expression '" + Regex + "'";
                return false;
            }

            Value = value;
            return true;
        }
    }
}

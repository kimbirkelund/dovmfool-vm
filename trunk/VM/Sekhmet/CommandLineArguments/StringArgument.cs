namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents an argument of type string.
    /// </summary>
    public class StringArgument : ArgumentBase<string> {
        /// <summary>
        /// Initializes a new instance of the <see cref="StringArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public StringArgument( string name )
            : base( "string", name ) {
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

            if (value == null || value.Length == 0) {
                failureReason = "value must be a non-empty string";
                return false;
            }

            Value = value;
            return true;
        }
    }
}
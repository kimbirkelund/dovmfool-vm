namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents a flag or switch command-line argument.
    /// </summary>
    public class FlagArgument : ArgumentBase<bool> {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlagArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public FlagArgument( string name )
            : base( null, name ) {
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
            if (value != null) {
                failureReason = "No value should be passed to a flag argument.";
                return false;
            }

            Value = true;

            failureReason = null;
            return true;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Sekhmet.CommandLineArguments.FlagArgument"/> to <see cref="System.Boolean"/>.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <returns>The result of the conversion.</returns>
        public static implicit operator bool( FlagArgument arg ) {
            return arg.Value;
        }
    }
}
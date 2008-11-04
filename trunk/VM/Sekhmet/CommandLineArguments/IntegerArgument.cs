using System;
namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents an integer command-line argument.
    /// </summary>
    public class IntegerArgument : ArgumentBase<int> {
        /// <summary>
        /// Gets or sets the minimum value.
        /// </summary>
        /// <value>The minimum value.</value>
        public int? MinimumValue { get; set; }
        /// <summary>
        /// Gets or sets the maximum value.
        /// </summary>
        /// <value>The maximum value.</value>
        public int? MaximumValue { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        public IntegerArgument( string name, int? minimumValue, int? maximumValue )
            : base( "integer", name ) {
            if (minimumValue.HasValue && maximumValue.HasValue && minimumValue > maximumValue)
                throw new ArgumentException( "The specified maximum value must be greater than or equal to the minimum value.", "maximumValue" );

            this.MinimumValue = minimumValue;
            this.MaximumValue = maximumValue;

            if (MinimumValue.HasValue)
                UsageInformation.Add( "Minumum value", MinimumValue.Value.ToString() );
            if (MaximumValue.HasValue)
                UsageInformation.Add( "Maximum value", MaximumValue.Value.ToString() );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="minValue">The min value.</param>
        public IntegerArgument( string name, int? minValue )
            : this( name, minValue, null ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegerArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public IntegerArgument( string name )
            : this( name, null, null ) {
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
            int intValue;

            if (int.TryParse( value, out intValue )) {
                if (MinimumValue.HasValue && intValue < MinimumValue.Value || MaximumValue.HasValue && intValue > MaximumValue.Value) {
                    failureReason = "Specified value must be between " + MinimumValue + " and " + MaximumValue + " (both inclusive).";
                    return false;
                }
                this.Value = intValue;
                failureReason = null;
                return true;
            } else {
                failureReason = "value must be an integer";
                return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Commen interface for all command-line arguments.
    /// </summary>
    public interface IArgument {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        object Value { get; }
        /// <summary>
        /// Gets the name.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is present.
        /// </summary>
        bool IsPresent { get; }
        /// <summary>
        /// Gets the position this argument can be specified in on the command line without using its name.
        /// </summary>
        /// <value>The position; or -1 if not a positional argument.</value>
        int Position { get; }
        /// <summary>
        /// Gets a value indicating whether this instance is required. Parsing a set of arguments not containing this argument will fail.
        /// </summary>
        bool IsRequired { get; }
        /// <summary>
        /// Gets a textual human-readable description of the purpose of this argument.
        /// </summary>
        string Description { get; }
        /// <summary>
        /// Gets additional information to be output to the user about the usage of this specific argument.
        /// </summary>
        IDictionary<string, string> UsageInformation { get; }
        /// <summary>
        /// Gets a short description (less than 10 chars preferably) of the value type this argument expects.
        /// </summary>
        string ShortValueTypeDescription { get; }
        /// <summary>
        /// Gets or sets the argument group.
        /// </summary>
        /// <value>The argument group.</value>
        int ArgumentGroup { get; }

        /// <summary>
        /// Attempts to set the value of the arguments; on failing the reason is written to <c>failureReason</c>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="failureReason">The failure reason.</param>
        /// <returns>A value indicating whether setting the value was succesfully; if <c>false</c> <c>failureReason</c> contains the reason for the faiure.</returns>
        bool TrySetValue( string value, out string failureReason );
    }
}

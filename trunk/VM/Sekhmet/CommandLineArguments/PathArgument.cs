using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents an argument that takes a path as its value.
    /// </summary>
    public class PathArgument : ArgumentBase<string> {
        static Regex pathRegex;

        /// <summary>
        /// Gets or sets a value indicating whether the specified path must exist.
        /// </summary>
        public bool MustExist { get; private set; }
        /// <summary>
        /// Gets a value indicating whether the value of this argument must point to a file.
        /// </summary>
        public bool? MustBeFile { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PathArgument"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="mustExist">if set to <c>true</c> [must exist].</param>
        /// <param name="mustBeFile">The must be file.</param>
        public PathArgument( string name, bool mustExist, bool? mustBeFile )
            : base( mustBeFile.HasValue ? (mustBeFile.Value ? "filePath" : "directoryPath") : "path", name ) {
            this.MustExist = mustExist;
            this.MustBeFile = mustBeFile;
            pathRegex = new Regex( @"^(([a-zA-Z]\:|\\)\\)?([^\\]+\\)*[^\/:*?""<>|]+?$" );
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
            bool success = pathRegex.Match( value ).Success;
            if (!success)
                failureReason = "Specified value must a valid Windows or UNC path.";
            else if (MustExist) {
                if (!MustBeFile.HasValue) {
                    success = File.Exists( value ) || Directory.Exists( value );
                    if (success)
                        Value = value;
                    else
                        failureReason = "Specified value must be an existing path.";
                } else {
                    success = MustBeFile.Value ? File.Exists( value ) : Directory.Exists( value );
                    if (success)
                        Value = value;
                    else
                        failureReason = "Specified value must be an existing " + (MustBeFile.Value ? "file" : "directory") + ".";
                }
            } else if (MustBeFile.HasValue) {
                success = MustBeFile.Value && !Directory.Exists( value ) || !MustBeFile.Value && !File.Exists( value );
                if (!success)
                    failureReason = "Specified value must point to a " + (MustBeFile.Value ? "file" : "directory") + ".";
                else
                    Value = value;
            }

            return success;
        }
    }
}

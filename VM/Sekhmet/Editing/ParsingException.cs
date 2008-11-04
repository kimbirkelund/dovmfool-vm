using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// The exception thrown when an error occures during parsing.
    /// </summary>
    public class ParsingException : Exception {
        ParsingError error;
        /// <summary>
        /// Gets the error.
        /// </summary>
        /// <value>The error.</value>
        public ParsingError Error {
            get { return error; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingException"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public ParsingException( ParsingError error )
            : base( error.ToString()) {
            this.error = error;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// The exception thrown when an error is logged and the settings of the logger requires it to throw an exception of errors.
    /// </summary>
	public class ErrorException : Exception {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
		public ErrorException() {
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
		public ErrorException( string message )
			: base( message ) {
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
		public ErrorException( string message, Exception inner )
			: base( message, inner ) {
		}
	}
}

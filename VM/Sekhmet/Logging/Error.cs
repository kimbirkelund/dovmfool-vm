using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sekhmet.Logging {
    /// <summary>
    /// Represents an error message.
    /// </summary>
    public class Error {
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; private set; }
        /// <summary>
        /// Gets the exception if one has occured.
        /// </summary>
        /// <value>The exception.</value>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public Error( string message ) {
            this.Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Error"/> class.
        /// </summary>
        /// <param name="exception">The exception that caused the error.</param>
        public Error( Exception exception )
            : this( exception.Message ) {
            this.Exception = exception;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.Error"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.Error"/>.
        /// </returns>
        public override string ToString() {
            return Message;
        }
    }
}

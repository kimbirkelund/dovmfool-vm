using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents an exception specifying that an error occured during the matching of a command line argument to a registered argument.
    /// </summary>
    [global::System.Serializable]
    public class ArgumentMatchException : ArgumentException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMatchException"/> class.
        /// </summary>
        public ArgumentMatchException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMatchException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ArgumentMatchException( string message ) : base( message ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMatchException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public ArgumentMatchException( string message, Exception inner ) : base( message, inner ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentMatchException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected ArgumentMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context )
            : base( info, context ) { }

        /// <summary>
        /// Prints the exception to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public virtual void Print( TextWriter writer ) {
            writer.WriteLine( Message );
        }
    }
}

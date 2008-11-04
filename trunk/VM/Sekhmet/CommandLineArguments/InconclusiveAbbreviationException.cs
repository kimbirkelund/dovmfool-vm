using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Represents an exception specifying that an abbreviation matched more than one registered argument.
    /// </summary>
    [global::System.Serializable]
    public class InconclusiveMatchException : ArgumentMatchException {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        /// <summary>
        /// Gets the argument name specified.
        /// </summary>
        public string ArgumentName { get; private set; }
        /// <summary>
        /// Gets the possible matches.
        /// </summary>
        public IEnumerable<IArgument> PossibleMatches { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InconclusiveMatchException"/> class.
        /// </summary>
        public InconclusiveMatchException() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InconclusiveMatchException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public InconclusiveMatchException( string message ) : base( message ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InconclusiveMatchException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public InconclusiveMatchException( string message, Exception inner ) : base( message, inner ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InconclusiveMatchException"/> class.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected InconclusiveMatchException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context )
            : base( info, context ) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="InconclusiveMatchException"/> class.
        /// </summary>
        /// <param name="argumentName">Name of the argument.</param>
        /// <param name="matches">The matches.</param>
        public InconclusiveMatchException( string argumentName, IEnumerable<IArgument> matches )
            : this( "Inconclusive abbreviation" ) {
            this.ArgumentName = argumentName;
            this.PossibleMatches = matches;
        }

        /// <summary>
        /// Prints the exception to the specified writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public override void Print( System.IO.TextWriter writer ) {
            writer.WriteLine( "Inconclusive match for argument '" + ArgumentName + "'. Possible matches:" );
            PossibleMatches.ForEach( m => writer.WriteLine( "  " + m.Name ) );
            writer.WriteLine();
            writer.WriteLine( "If the name was supposed to be an abbreviation of an argument consiting of the upper-case letters in that argument, write the argument in all upper-cases to avoid this error." );
        }
    }
}

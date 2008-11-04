using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents an error that has occured during the parsing of a source file.
    /// </summary>
    public class ParsingError {
        SourceSpan sourceLocation;
        /// <summary>
        /// Gets the source location of the error.
        /// </summary>
        public SourceSpan SourceLocation {
            get { return sourceLocation; }
        }

        bool isWarning = false;
        /// <summary>
        /// Gets a value indicating whether this instance is warning.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is warning; otherwise, <c>false</c>.
        /// </value>
        public bool IsWarning {
            get { return isWarning; }
        }

        Token token;
        /// <summary>
        /// Gets the token.
        /// </summary>
        /// <value>The token.</value>
        public Token Token {
            get { return token; }
        }

        string message;
        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        public string Message {
            get { return message; }
        }

        ParsingError innerError;
        /// <summary>
        /// Gets the inner error if present.
        /// </summary>
        /// <value>The inner error.</value>
        public ParsingError InnerError {
            get { return innerError; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerError">The inner error.</param>
        public ParsingError( string message, ParsingError innerError ) {
            this.message = message;
            this.innerError = innerError;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="innerError">The inner error.</param>
        public ParsingError( string message, SourceSpan sourceLocation, ParsingError innerError )
            : this( message, innerError ) {
            this.sourceLocation = sourceLocation;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="token">The token.</param>
        /// <param name="isWarning">if set to <c>true</c> [is warning].</param>
        /// <param name="innerError">The inner error.</param>
        public ParsingError( string message, SourceSpan sourceLocation, Token token, bool isWarning, ParsingError innerError )
            : this( message, sourceLocation, innerError ) {
            this.token = token;
            this.isWarning = isWarning;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="token">The token.</param>
        /// <param name="innerError">The inner error.</param>
        public ParsingError( string message, SourceSpan sourceLocation, Token token, ParsingError innerError )
            : this( message, sourceLocation, token, false, innerError ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="isWarning">if set to <c>true</c> [is warning].</param>
        /// <param name="innerError">The inner error.</param>
        public ParsingError( string message, SourceSpan sourceLocation, bool isWarning, ParsingError innerError )
            : this( message, sourceLocation, null, isWarning, innerError ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public ParsingError( string message )
            : this( message, null ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        public ParsingError( string message, SourceSpan sourceLocation )
            : this( message, sourceLocation, null, null ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="token">The token.</param>
        /// <param name="isWarning">if set to <c>true</c> [is warning].</param>
        public ParsingError( string message, SourceSpan sourceLocation, Token token, bool isWarning )
            : this( message, sourceLocation, isWarning, null ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="token">The token.</param>
        public ParsingError( string message, SourceSpan sourceLocation, Token token )
            : this( message, sourceLocation, token, false, null ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParsingError"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="sourceLocation">The source location.</param>
        /// <param name="isWarning">if set to <c>true</c> [is warning].</param>
        public ParsingError( string message, SourceSpan sourceLocation, bool isWarning )
            : this( message, sourceLocation, null, isWarning, null ) {
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Editing.ParsingError"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Editing.ParsingError"/>.
        /// </returns>
        public override string ToString() {
            return (IsWarning ? "Warning" : "Error") + " at " + SourceLocation.Start + ": " + Message;
        }
    }
}

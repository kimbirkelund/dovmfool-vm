using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.Logging;

namespace VMILLib.Parser {
	/// <summary>
	/// Represents an error with a position associated.
	/// </summary>
	class PositionedError : Error {
		/// <summary>
		/// Gets the position associated with the error.
		/// </summary>
		/// <value>The position.</value>
		public LexLocation Location { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="PositionedError"/> class.
		/// </summary>
		/// <param name="message">The error message.</param>
		/// <param name="sourceSpan">The error position.</param>
		public PositionedError( string message, LexLocation location )
			: base( message ) {
			this.Location = location;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PositionedError"/> class.
		/// </summary>
		/// <param name="exception">The exception that caused the error.</param>
		/// <param name="sourceSpan">The error position.</param>
		public PositionedError( Exception exception, LexLocation location )
			: base( exception ) {
			this.Location = location;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.PositionedError"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.PositionedError"/>.
		/// </returns>
		public override string ToString() {
			return Location + ": " + Message;
		}
	}
}

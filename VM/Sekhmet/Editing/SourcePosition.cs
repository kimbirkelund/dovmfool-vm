using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a position in a source file.
    /// </summary>
    public struct SourcePosition {
        int index;
        /// <summary>
        /// Gets the overall character index.
        /// </summary>
        public int Index {
            get { return index; }
        }

        int line;
        /// <summary>
        /// Gets the line number of this position.
        /// </summary>
        public int Line {
            get { return line; }
        }

        int column;
        /// <summary>
        /// Gets the column number for this position.
        /// </summary>
        public int Column {
            get { return column; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourcePosition"/> struct.
        /// </summary>
        /// <param name="index">The overall character index.</param>
        /// <param name="line">The line number.</param>
        /// <param name="column">The column number.</param>
        public SourcePosition( int index, int line, int column ) {
            this.index = index;
            this.line = line;
            this.column = column;
        }

        /// <summary>
        /// Returns a <see cref="T:string"/> representing this position.
        /// </summary>
        public override string ToString() {
            return "[" + line + ":" + column + "]";
        }

        /// <summary>
        /// Computes the position <c>columns</c> characters to the left of the current position.
        /// </summary>
        /// <param name="p">The start position.</param>
        /// <param name="columns">The number characters to move to the left - must be less than the column number of <c>p</c>.</param>
        public static SourcePosition operator -( SourcePosition p, int columns ) {
            if (p.Column - columns > 0)
                throw new ArgumentOutOfRangeException( "Argument must be less than the column number.", "caret" );

            return new SourcePosition( p.Index - columns, p.Line, p.Column - columns );
        }

        /// <summary>
        /// Computes the position <c>columns</c> characters to the right of the current position.
        /// </summary>
        /// <param name="p">The start position.</param>
        /// <param name="caret">The number of characters to move to the right.</param>
        /// <returns>The location <c>columns</c> characters to the right of <c>p</c> - no validation is performed to ensure the existance of this position.</returns>
        public static SourcePosition operator +( SourcePosition p, int caret ) {
            return new SourcePosition( p.Index + caret, p.Line, p.Column + caret );
        }
    }
}

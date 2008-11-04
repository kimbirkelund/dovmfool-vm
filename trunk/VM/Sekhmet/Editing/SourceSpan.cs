using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a span of a source file.
    /// </summary>
    public struct SourceSpan {
        SourcePosition start;
        /// <summary>
        /// Gets the start position.
        /// </summary>
        /// <value>The start.</value>
        public SourcePosition Start {
            get { return start; }
        }

        SourcePosition end;
        /// <summary>
        /// Gets the end position.
        /// </summary>
        /// <value>The end.</value>
        public SourcePosition End {
            get { return end; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceSpan"/> struct.
        /// </summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        public SourceSpan( SourcePosition start, SourcePosition end ) {
            this.start = start;
            this.end = end;
        }

        /// <summary>
        /// Creates a span from a specified a position and the length of the span.
        /// </summary>
        /// <param name="pos">The pos.</param>
        /// <param name="length">The length.</param>
        /// <returns>A <see cref="SourceSpan"/> starting at <c>pos</c> and ending at <c>pos</c> + <c>length</c>.</returns>
        public static SourceSpan FromPosition( SourcePosition pos, int length ) {
            return new SourceSpan( pos, pos + length );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a table of contents tag. The presence of the this indicates where a TOC should be inserted.
    /// </summary>
    public class TOCTag : Tag {
        /// <summary>
        /// Gets a value indicating whether this instance is a table of contents tag.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is table of contents tag; otherwise, <c>false</c>.
        /// </value>
        public override bool IsTOC {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TOCTag"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="properties">The properties.</param>
        public TOCTag( SourceSpan position, IEnumerable<Property> properties )
            : base( position, "toc", properties ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TOCTag"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="properties">The properties.</param>
        public TOCTag( SourceSpan position, params Property[] properties )
            : base( position, "toc", (IEnumerable<Property>) properties ) {
        }
    }
}

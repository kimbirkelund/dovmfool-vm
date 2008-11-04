using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a text paragraph.
    /// </summary>
    public class ParagraphTag : Tag {
        /// <summary>
        /// Gets the singleton instance of this tag.
        /// </summary>
        public static readonly ParagraphTag Instance = new ParagraphTag();

        /// <summary>
        /// Gets a value indicating whether this instance is a paragraph.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a paragraph; otherwise, <c>false</c>.
        /// </value>
        public override bool IsParagraph {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ParagraphTag"/> class.
        /// </summary>
        ParagraphTag() {
        }
    }
}

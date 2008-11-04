using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a linebreak tag.
    /// </summary>
    public class LinebreakTag : Tag {
        /// <summary>
        /// The single instance used for linebreaks.
        /// </summary>
        public static readonly LinebreakTag Instance = new LinebreakTag();

        /// <summary>
        /// Gets a value indicating whether this instance is a linebreak.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a linebreak; otherwise, <c>false</c>.
        /// </value>
        public override bool IsLinebreak {
            get { return true; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LinebreakTag"/> class.
        /// </summary>
        LinebreakTag() {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a tag for headings.
    /// </summary>
    public class HeadingTag : Tag {
        /// <summary>
        /// Gets a value indicating whether this instance is a heading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a heading; otherwise, <c>false</c>.
        /// </value>
        public override bool IsHeading {
            get { return true; }
        }

        int level;
        /// <summary>
        /// Gets the heading level.
        /// </summary>
        /// <value>The level.</value>
        public int Level {
            get { return level; }
        }

        bool numbered;
        /// <summary>
        /// Gets a value indicating whether this <see cref="HeadingTag"/> is numbered.
        /// </summary>
        /// <value><c>true</c> if numbered; otherwise, <c>false</c>.</value>
        public bool Numbered {
            get { return numbered; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HeadingTag"/> class.
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="numbered">if set to <c>true</c> [numbered].</param>
        public HeadingTag( int level, bool numbered ) {
            this.level = level;
            this.numbered = numbered;
        }
    }
}

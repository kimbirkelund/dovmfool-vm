using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a tag found in the source.
    /// </summary>
    public class Tag {
        /// <summary>
        /// Gets a value indicating whether this instance is a heading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a heading; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsHeading {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a linebreak.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a linebreak; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsLinebreak {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a paragraph.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a paragraph; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsParagraph {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is whitespace.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is whitespace; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsWhitespace {
            get { return IsLinebreak || IsParagraph; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is a table of contents tag.
        /// </summary>
        /// <value><c>true</c> if this instance is table of contents tag; otherwise, <c>false</c>.</value>
        public virtual bool IsTOC {
            get { return false; }
        }

        string name;
        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <value>The name.</value>
        public string Name {
            get { return name; }
        }

        /// <summary>
        /// Gets the location of this instance in the source file.
        /// </summary>
        public SourceSpan SourceSpan { get; private set; }

        PropertyEnumerable properties;
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public PropertyEnumerable Properties {
            get { return properties; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        protected Tag() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="sourceSpan">The source span.</param>
        /// <param name="name">The name.</param>
        /// <param name="properties">The properties.</param>
        public Tag( SourceSpan sourceSpan, string name, IEnumerable<Property> properties ) {
            this.name = name;
            this.properties = new PropertyEnumerable( properties );
            this.SourceSpan = sourceSpan;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="name">The name.</param>
        /// <param name="properties">The properties.</param>
        public Tag( SourceSpan position, string name, params Property[] properties )
            : this( position, name, (IEnumerable<Property>) properties ) {
        }

        /// <summary>
        /// Creates a tag form a given token.
        /// </summary>
        internal static Tag FromToken( Token token ) {
            if (token.IsLinebreak)
                return LinebreakTag.Instance;
            if (token.IsParagraph)
                return ParagraphTag.Instance;
            if (token.IsText)
                throw new ArgumentException( "Can't make tag from this token type" );
            if (token.IsHeading) {
                bool numbered = bool.Parse( token.Properties["numbered"] );
                int level = int.Parse( token.Properties["level"] );

                return new HeadingTag( level, numbered );
            }
            if (token.IsTOC)
                return new TOCTag( token.SourceSpan, token.Properties );

            return new Tag( token.SourceSpan, token.TagName, token.Properties );
        }
    }
}

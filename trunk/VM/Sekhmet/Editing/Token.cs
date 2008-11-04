using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a token found in a source file.
    /// </summary>
    public class Token {
        /// <summary>
        /// Gets the location of the token.
        /// </summary>
        public SourceSpan SourceSpan { get; private set; }

        string text;
        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text {
            get { return text; }
        }

        bool isWhitespace = false;
        /// <summary>
        /// Gets a value indicating whether this instance is whitespace.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is white space; otherwise, <c>false</c>.
        /// </value>
        public bool IsWhitespace {
            get { return isWhitespace || IsLinebreak || IsParagraph; }
        }

        string tagName = null;
        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <value>The name of the tag.</value>
        public string TagName {
            get { return tagName; }
        }

        bool isOpeningTag = false;
        /// <summary>
        /// Gets a value indicating whether this instance is an opening tag.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is an opening tag; otherwise, <c>false</c>.
        /// </value>
        public bool IsOpeningTag {
            get { return isOpeningTag; }
        }

        bool isClosingTag = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a closing tag.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a closing tag; otherwise, <c>false</c>.
        /// </value>
        public bool IsClosingTag {
            get { return isClosingTag; }
        }

        bool isText = false;
        /// <summary>
        /// Gets a value indicating whether this instance is text.
        /// </summary>
        /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
        public bool IsText {
            get { return isText; }
        }

        PropertyEnumerable properties;
        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <value>The properties.</value>
        public PropertyEnumerable Properties {
            get { return properties; }
        }

        bool isHeading = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a heading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a heading; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeading {
            get { return isHeading; }
        }

        bool isTag = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a tag.
        /// </summary>
        /// <value><c>true</c> if this instance is a tag; otherwise, <c>false</c>.</value>
        public bool IsTag {
            get { return isTag; }
        }

        bool isLinebreak = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a linebreak.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a linebreak; otherwise, <c>false</c>.
        /// </value>
        public bool IsLinebreak {
            get { return isLinebreak; }
        }

        bool isParagraph = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a paragraph.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a paragraph; otherwise, <c>false</c>.
        /// </value>
        public bool IsParagraph {
            get { return isParagraph; }
        }

        bool isTOC = false;
        /// <summary>
        /// Gets a value indicating whether this instance is a table of contents tag.
        /// </summary>
        /// <value><c>true</c> if this instance is a table of contents tag; otherwise, <c>false</c>.</value>
        public bool IsTOC {
            get { return isTOC; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="sourceSpan">The source span.</param>
        Token( SourceSpan sourceSpan ) {
            this.SourceSpan = sourceSpan;
        }

        /// <summary>
        /// Creates a text token.
        /// </summary>
        internal static Token CreateText( SourceSpan sourceSpan, string text ) {
            Token token = new Token( sourceSpan );
            foreach (char c in text) {
                token.isWhitespace = char.IsWhiteSpace( c );
                if (!token.isWhitespace)
                    break;
            }

            token.isText = true;
            token.text = text;

            return token;
        }

        /// <summary>
        /// Creates a tag token.
        /// </summary>
        internal static Token CreateTag( SourceSpan sourceSpan, string tagName, bool isOpeningTag, bool isClosingTag, IEnumerable<Property> properties ) {
            Token token = new Token( sourceSpan );
            if (tagName.ToLower() == "toc")
                token.isTOC = true;
            else {
                token.isTag = true;

                token.tagName = tagName;
                token.isOpeningTag = isOpeningTag;
                token.isClosingTag = isClosingTag;
            }

            token.properties = new PropertyEnumerable( properties );
            return token;
        }

        /// <summary>
        /// Creates a heading token.
        /// </summary>
        internal static Token CreateHeading( SourceSpan sourceSpan, int level, bool numbered ) {
            Token token = new Token( sourceSpan );
            token.isHeading = true;

            Property pLevel = new Property( "level", level.ToString() );
            Property pNumbered = new Property( "numbered", numbered.ToString() );
            token.properties = new PropertyEnumerable( new Property[] { pLevel, pNumbered } );

            return token;
        }

        /// <summary>
        /// Creates a linebreak token.
        /// </summary>
        internal static Token CreateLinebreak( SourceSpan sourceSpan ) {
            Token token = new Token( sourceSpan );
            token.isLinebreak = true;

            return token;
        }

        /// <summary>
        /// Creates a paragraph token.
        /// </summary>
        internal static Token CreateParagraph( SourceSpan sourceSpan ) {
            Token token = new Token( sourceSpan );
            token.isParagraph = true;

            return token;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Editing.Token"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Editing.Token"/>.
        /// </returns>
        public override string ToString() {
            if (this.IsText)
                if (this.Text.Length > 10)
                    return "[TEXT]" + this.Text.Substring( 0, 5 ) + "..." + this.Text.Substring( this.Text.Length - 5 ) + "[/TEXT]";
                else
                    return "[TEXT]" + this.Text + "[/TEXT]";
            else if (this.IsTag) {
                StringBuilder sb = new StringBuilder();
                sb.Append( "[" + (this.IsHeading ? "!" : (this.IsClosingTag && !this.IsOpeningTag ? "/" : "")) + this.TagName );
                foreach (Property pair in this.Properties)
                    sb.Append( " " + pair.Name + "=\"" + pair.Value + "\"" );
                sb.Append( (this.IsClosingTag && this.IsOpeningTag ? " /" : "") + "]" );
                return sb.ToString();
            } else if (this.IsLinebreak)
                return "[LINEBREAK /]";
            else if (this.IsParagraph)
                return "[PARAGRAPGBREAK /]";
            else if (this.IsHeading)
                return "!" + this.Properties["level"] + (this.Properties["numbered"] != null ? "*" : "") + " ";

            throw new InvalidOperationException( "Invalid token" );
        }
    }

    /// <summary>
    /// Represents a indexable enumerable set of properties.
    /// </summary>
    public class PropertyEnumerable : IEnumerable<Property> {
        IEnumerable<Property> sequence;

        /// <summary>
        /// Gets the <see cref="System.String"/> with the specified name.
        /// </summary>
        /// <value></value>
        public string this[string name] {
            get {
                Property attr = sequence.FirstOrDefault( ( a ) => a.Name == name );
                if (attr != default( Property ))
                    return attr.Value;
                return null;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyEnumerable"/> class.
        /// </summary>
        /// <param name="sequence">The sequence.</param>
        public PropertyEnumerable( IEnumerable<Property> sequence ) {
            if (sequence == null)
                sequence = new Property[0];
            this.sequence = sequence;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Property> GetEnumerator() {
            return sequence.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}

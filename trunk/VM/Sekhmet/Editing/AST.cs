using System;
using gen = System.Collections.Generic;
using System.Text;
using System.IO;
using Sekhmet.Collections;
using System.Linq;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents an abstract syntax tree generated from a source file.
    /// </summary>
    public sealed class AST : Tree<AST>, ICloneable {
        bool rendered;
        /// <summary>
        /// Gets or sets a value indicating whether this instance has been rendered.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance hass been rendered; otherwise, <c>false</c>.
        /// </value>
        public bool IsRendered {
            get { return rendered || !(IsText || IsLayout); }
            set { rendered = value; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is text.
        /// </summary>
        /// <value><c>true</c> if this instance is text; otherwise, <c>false</c>.</value>
        public bool IsText {
            get { return Text != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is layout.
        /// </summary>
        /// <value><c>true</c> if this instance is layout; otherwise, <c>false</c>.</value>
        public bool IsLayout {
            get { return InputTag != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is linebreak.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is linebreak; otherwise, <c>false</c>.
        /// </value>
        public bool IsLinebreak {
            get { return IsLayout && InputTag.IsLinebreak; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is paragraph.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is paragraph; otherwise, <c>false</c>.
        /// </value>
        public bool IsParagraph {
            get { return IsLayout && InputTag.IsParagraph; }
        }

        bool textIsWS;
        /// <summary>
        /// Gets a value indicating whether this instance is whitespace.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is whitespace; otherwise, <c>false</c>.
        /// </value>
        public bool IsWhitespace {
            get { return IsLayout && InputTag.IsWhitespace || textIsWS; }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is heading.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is heading; otherwise, <c>false</c>.
        /// </value>
        public bool IsHeading {
            get { return IsLayout && InputTag.IsHeading; }
        }

        string text;
        /// <summary>
        /// Gets or sets the text.
        /// </summary>
        /// <value>The text.</value>
        public string Text {
            get { return text; }
            set {
                text = value;
                textIsWS = string.IsNullOrEmpty( text ) || text.All( delegate( char c ) { return char.IsWhiteSpace( c ); } );
            }
        }

        gen.IList<IRenderTag> outputTags = new gen.List<IRenderTag>();
        /// <summary>
        /// Gets the output tags.
        /// </summary>
        /// <value>The output tags.</value>
        public gen.IList<IRenderTag> OutputTags {
            get { return outputTags; }
        }

        Tag inputTag;
        /// <summary>
        /// Gets the input tag.
        /// </summary>
        /// <value>The input tag.</value>
        public Tag InputTag {
            get { return inputTag; }
        }

        #region Cons
        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        public AST() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="outputTags">The output tags.</param>
        public AST( gen.IEnumerable<IRenderTag> outputTags )
            : this() {
            if (outputTags != null)
                outputTags.ForEach( delegate( IRenderTag tag ) { this.outputTags.Add( tag ); } );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="outputTags">The output tags.</param>
        public AST( params IRenderTag[] outputTags )
            : this( (gen.IEnumerable<IRenderTag>) outputTags ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="outputTags">The output tags.</param>
        public AST( string text, gen.IEnumerable<IRenderTag> outputTags )
            : this( outputTags ) {
            this.text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="outputTags">The output tags.</param>
        public AST( string text, params IRenderTag[] outputTags )
            : this( text, (gen.IEnumerable<IRenderTag>) outputTags ) {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="inputTag">The input tag.</param>
        /// <param name="outputTags">The output tags.</param>
        public AST( Tag inputTag, gen.IEnumerable<IRenderTag> outputTags )
            : this( outputTags ) {
            this.inputTag = inputTag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AST"/> class.
        /// </summary>
        /// <param name="inputTag">The input tag.</param>
        /// <param name="outputTags">The output tags.</param>
        public AST( Tag inputTag, params IRenderTag[] outputTags )
            : this( inputTag, (gen.IEnumerable<IRenderTag>) outputTags ) {
        }
        #endregion

        /// <summary>
        /// Renders this instance to the specified output.
        /// </summary>
        /// <param name="output">The output.</param>
        public void Render( TextWriter output ) {
            foreach (var tag in OutputTags)
                tag.RenderOpening( output );

            foreach (AST child in Children)
                child.Render( output );

            for (int i = OutputTags.Count - 1; i >= 0; i--)
                OutputTags[i].RenderClosing( output );
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        public AST Clone() {
            AST node;
            if (this.IsText)
                node = new AST( this.Text, this.OutputTags );
            else
                node = new AST( this.InputTag, this.OutputTags );

            foreach (AST child in Children)
                node.Children.Add( child.Clone() );

            return node;
        }

        #region Hidden members
        object ICloneable.Clone() { return Clone(); }
        #endregion
    }
}

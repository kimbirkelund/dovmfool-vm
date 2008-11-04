using System;
using System.Collections.Generic;
using System.Text;
using Sekhmet.Collections;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a node in a hierachy of nodes. Hierachy is a bit of a misnomer since this actually forms a graph.
    /// </summary>
    public sealed class TagHierachyNode : Graph<TagHierachyNode> {
        /// <summary>
        /// Gets the common node representing a heading.
        /// </summary>
        public static TagHierachyNode Heading {
            get { return new TagHierachyNode( "heading", true, true, true ); }
        }

        /// <summary>
        /// Gets the TOC node.
        /// </summary>
        public static TagHierachyNode TOC {
            get { return new TagHierachyNode( "toc", true, false, false ); }
        }

        bool acceptAny;
        /// <summary>
        /// Gets a value indicating whether any tag can be nested within this tag.
        /// </summary>
        public bool AcceptAny {
            get { return acceptAny; }
        }

        bool acceptNone;
        /// <summary>
        /// Gets a value indicating whether no tags are allowed within this one.
        /// </summary>
        public bool AcceptNone {
            get { return acceptNone; }
        }

        bool allowText;
        /// <summary>
        /// Gets a value indicating whether text is allowed within this tag.
        /// </summary>
        public bool AllowText {
            get { return allowText; }
        }

        string tagName;
        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        public string TagName {
            get { return tagName; }
        }

        bool removeWSAround;
        /// <summary>
        /// Gets a value indicating whether whitespace around this tag should be removed.
        /// </summary>
        public bool RemoveWSAround {
            get { return removeWSAround; }
        }

        bool removeWSWithin;
        /// <summary>
        /// Gets a value indicating whether whitespace should be removed within this tag.
        /// </summary>
        public bool RemoveWSWithin {
            get { return removeWSWithin; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHierachyNode"/> class.
        /// </summary>
        TagHierachyNode() {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHierachyNode"/> class.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="removeWSWithin">if set to <c>true</c> [remove WS within].</param>
        /// <param name="removeWSAround">if set to <c>true</c> [remove WS around].</param>
        /// <param name="allowText">if set to <c>true</c> [allow text].</param>
        /// <param name="neighborTags">The neighbor tags.</param>
        public TagHierachyNode( string tagName, bool removeWSWithin, bool removeWSAround, bool allowText, params TagHierachyNode[] neighborTags ) {
            this.tagName = tagName.ToLower();
            this.allowText = allowText;
            this.removeWSWithin = removeWSWithin;
            this.removeWSAround = removeWSAround;
            neighborTags.ForEach( delegate( TagHierachyNode n ) { this.Neighbors.Add( n ); } );
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHierachyNode"/> class.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="removeWSWithin">if set to <c>true</c> [remove WS within].</param>
        /// <param name="removeWSAround">if set to <c>true</c> [remove WS around].</param>
        /// <param name="allowText">if set to <c>true</c> [allow text].</param>
        /// <param name="acceptAny">if set to <c>true</c> [accept any].</param>
        /// <param name="acceptNone">if set to <c>true</c> [accept none].</param>
        public TagHierachyNode( string tagName, bool removeWSWithin, bool removeWSAround, bool allowText, bool acceptAny, bool acceptNone )
            : this( tagName, removeWSWithin, removeWSAround, allowText ) {
            this.acceptAny = acceptAny;
            this.acceptNone = acceptNone;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagHierachyNode"/> class.
        /// </summary>
        /// <param name="tagName">Name of the tag.</param>
        /// <param name="removeWSWithin">if set to <c>true</c> [remove WS within].</param>
        /// <param name="removeWSAround">if set to <c>true</c> [remove WS around].</param>
        /// <param name="allowText">if set to <c>true</c> [allow text].</param>
        /// <param name="acceptAny">if set to <c>true</c> [accept any].</param>
        public TagHierachyNode( string tagName, bool removeWSWithin, bool removeWSAround, bool allowText, bool acceptAny )
            : this( tagName, removeWSWithin, removeWSAround, allowText, acceptAny, !acceptAny ) {
        }
    }
}

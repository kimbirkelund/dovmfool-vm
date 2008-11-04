using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sekhmet.Editing {
    /// <summary>
    /// A render for a tag that marks that a seperat file should be inserted at this position.
    /// </summary>
    public class IncludeRender : IRender {
        TagHierachyNode tags = new TagHierachyNode( "include", false, true, true, true );
        /// <summary>
        /// Gets the tag hierachy.
        /// </summary>
        /// <value>The tag hierachy.</value>
        public IEnumerable<TagHierachyNode> TagHierachy {
            get { yield return tags; }
        }

        /// <summary>
        /// Determines whether this instance can process the specified tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can process the specified tag; otherwise, <c>false</c>.
        /// </returns>
        public bool CanProcess( Tag tag ) {
            return tag.Name == "include";
        }

        IRenderFactory factory;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncludeRender"/> class.
        /// </summary>
        /// <param name="renderFactory">The render factory.</param>
        public IncludeRender( IRenderFactory renderFactory ) {
            this.factory = renderFactory;
        }

        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        public void Process( Errors errors, AST node ) {
            using (TextReader reader = new StreamReader( node.InputTag.Properties["file"] )) {
                AST newNode = Parser.Parse( errors, reader, factory );

                node.Replace( newNode );
            }
        }
    }
}

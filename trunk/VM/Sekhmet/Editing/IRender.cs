using System;
using System.Collections.Generic;
using System.Text;
using Sekhmet.Collections;

namespace Sekhmet.Editing {
    /// <summary>
    /// Specifies that the implementing class can render tags for which <see cref="M:CanProcess"/> returns <c>true</c>.
    /// </summary>
    public interface IRender {
        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        void Process( Errors errors, AST node );
        /// <summary>
        /// Determines whether this instance can process the specified tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can process the specified tag; otherwise, <c>false</c>.
        /// </returns>
        bool CanProcess( Tag tag );
        /// <summary>
        /// Gets the tag hierachy.
        /// </summary>
        /// <value>The tag hierachy.</value>
        IEnumerable<TagHierachyNode> TagHierachy { get; }
    }
}

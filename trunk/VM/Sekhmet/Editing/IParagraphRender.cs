using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// A render for paragraphs.
    /// </summary>
    public interface IParagraphRender {
        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        void Process( Errors errors, AST node );
    }
}

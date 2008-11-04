using System;
using System.Collections.Generic;
using System.Text;
using Sekhmet.Collections;

namespace Sekhmet.Editing {
    /// <summary>
    /// Specifies that the implementing class can render headings.
    /// </summary>
    public interface IHeadingRender {
        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        void Process( Errors errors, AST node );
    }
}

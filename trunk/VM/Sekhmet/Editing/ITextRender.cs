using System;
using System.Collections.Generic;
using System.Text;
using Sekhmet.Collections;

namespace Sekhmet.Editing {
    /// <summary>
    /// Specifies that the implementing class can render plain text.
    /// </summary>
    public interface ITextRender {
        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        void Process( Errors errors, AST node );
    }
}

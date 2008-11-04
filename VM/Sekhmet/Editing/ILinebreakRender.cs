using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Specifies that the implementing class is capable of rendering a linebreak.
    /// </summary>
    public interface ILinebreakRender {
        /// <summary>
        /// Processes the specified node.
        /// </summary>
        /// <param name="errors">The object to report errors to.</param>
        /// <param name="node">The node.</param>
        void Process( Errors errors, AST node );
    }
}

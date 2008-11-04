using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sekhmet.Editing {
    /// <summary>
    /// Specifies the interface for tags output by renderes. The use of this serves to postpone the conversion to text to lastest possible point.
    /// </summary>
    public interface IRenderTag : ICloneable {
        /// <summary>
        /// Opens the tag.
        /// </summary>
        /// <param name="output">The output.</param>
        void RenderOpening( TextWriter output );
        /// <summary>
        /// Closes the tag.
        /// </summary>
        /// <param name="output">The output.</param>
        void RenderClosing( TextWriter output );
    }
}

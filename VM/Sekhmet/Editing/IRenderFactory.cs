using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a factory for a set of renderes. Usually one such factory will exist for each output format (i.e. HTML, LaTeX etc.).
    /// </summary>
    public interface IRenderFactory {
        /// <summary>
        /// Gets the paragraph render.
        /// </summary>
        /// <value>The paragraph render.</value>
        IParagraphRender ParagraphRender { get;}
        /// <summary>
        /// Gets the linebreak render.
        /// </summary>
        /// <value>The linebreak render.</value>
        ILinebreakRender LinebreakRender { get;}
        /// <summary>
        /// Gets the heading render.
        /// </summary>
        /// <value>The heading render.</value>
        IHeadingRender HeadingRender { get;}
        /// <summary>
        /// Gets the TOC render.
        /// </summary>
        /// <value>The TOC render.</value>
        IRender TOCRender { get;}
        /// <summary>
        /// Gets the text render.
        /// </summary>
        /// <value>The text render.</value>
        ITextRender TextRender { get;}
        /// <summary>
        /// Gets the tag renderes.
        /// </summary>
        /// <value>The tag renderes.</value>
        IEnumerable<IRender> TagRenderes { get;}

        /// <summary>
        /// Wraps the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <returns>A new <see cref="TextWriter"/> wrapped for a specific purpose as defined by the factory.</returns>
        TextWriter WrapWriter( TextWriter writer );
    }
}

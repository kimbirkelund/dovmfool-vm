using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents a render factory that composes two or more render factories.
    /// </summary>
    public class CompositeRenderFactory : IRenderFactory {
        IRenderFactory primarySource;
        IEnumerable<IRender> additionalRenderes;

        IParagraphRender paragraphRender;
        /// <summary>
        /// Gets the paragraph render.
        /// </summary>
        /// <value>The paragraph render.</value>
        public IParagraphRender ParagraphRender { get { return paragraphRender; } }

        ILinebreakRender linebreakRender;
        /// <summary>
        /// Gets the linebreak render.
        /// </summary>
        /// <value>The linebreak render.</value>
        public ILinebreakRender LinebreakRender { get { return linebreakRender; } }

        IHeadingRender headingRender;
        /// <summary>
        /// Gets the heading render.
        /// </summary>
        /// <value>The heading render.</value>
        public IHeadingRender HeadingRender { get { return headingRender; } }

        IRender tocRender;
        /// <summary>
        /// Gets the TOC render.
        /// </summary>
        /// <value>The TOC render.</value>
        public IRender TOCRender { get { return tocRender; } }

        ITextRender textRender;
        /// <summary>
        /// Gets the text render.
        /// </summary>
        /// <value>The text render.</value>
        public ITextRender TextRender { get { return textRender; } }

        /// <summary>
        /// Gets the tag renderes.
        /// </summary>
        /// <value>The tag renderes.</value>
        public IEnumerable<IRender> TagRenderes {
            get {
                foreach (IRender render in primarySource.TagRenderes)
                    yield return render;
                foreach (IRender render in additionalRenderes)
                    yield return render;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeRenderFactory"/> class.
        /// </summary>
        /// <param name="primarySource">The primary source.</param>
        /// <param name="secondarySource">The secondary source.</param>
        public CompositeRenderFactory( IRenderFactory primarySource, IRenderFactory secondarySource ) {
            this.primarySource = primarySource;
            this.additionalRenderes = secondarySource.TagRenderes;

            this.paragraphRender = primarySource.ParagraphRender != null ? primarySource.ParagraphRender : secondarySource.ParagraphRender;
            this.linebreakRender = primarySource.LinebreakRender != null ? primarySource.LinebreakRender : secondarySource.LinebreakRender;
            this.headingRender = primarySource.HeadingRender != null ? primarySource.HeadingRender : secondarySource.HeadingRender;
            this.tocRender = primarySource.TOCRender != null ? primarySource.TOCRender : secondarySource.TOCRender;
            this.textRender = primarySource.TextRender != null ? primarySource.TextRender : secondarySource.TextRender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeRenderFactory"/> class.
        /// </summary>
        /// <param name="primarySource">The primary source.</param>
        /// <param name="additionalRenderes">The additional renderes.</param>
        public CompositeRenderFactory( IRenderFactory primarySource, IEnumerable<IRender> additionalRenderes ) {
            this.primarySource = primarySource;
            this.additionalRenderes = additionalRenderes;

            this.paragraphRender = primarySource.ParagraphRender;
            this.linebreakRender = primarySource.LinebreakRender;
            this.headingRender = primarySource.HeadingRender;
            this.tocRender = primarySource.TOCRender;
            this.textRender = primarySource.TextRender;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeRenderFactory"/> class.
        /// </summary>
        /// <param name="primarySource">The primary source.</param>
        /// <param name="additionalRenderes">The additional renderes.</param>
        public CompositeRenderFactory( IRenderFactory primarySource, params IRender[] additionalRenderes )
            : this( primarySource, (IEnumerable<IRender>) additionalRenderes ) {
        }

        /// <summary>
        /// Wraps the writer.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <returns>
        /// A new <see cref="TextWriter"/> wrapped for a specific purpose as defined by the factory.
        /// </returns>
        public TextWriter WrapWriter( TextWriter writer ) {
            return primarySource.WrapWriter( writer );
        }
    }
}

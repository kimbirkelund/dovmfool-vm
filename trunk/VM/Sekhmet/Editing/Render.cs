using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sekhmet.Collections;
using System.Linq;

namespace Sekhmet.Editing {
    /// <summary>
    /// This class handles the overall rendering task, guided by the different tag renderes.
    /// </summary>
    public static class Render {
        /// <summary>
        /// Processes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="renderFactory">The render factory.</param>
        /// <returns></returns>
        public static Errors Process( TextReader input, TextWriter output, IRenderFactory renderFactory ) {
            Errors errors = new Errors();
            renderFactory = new CompositeRenderFactory( renderFactory, new IncludeRender( renderFactory ) );
            AST root = Parser.Parse( errors, input, renderFactory );


            Queue<AST> queue = new Queue<AST>();
            Action<AST> addNodes = delegate( AST n ) {
                if (n.Parent != null && !n.IsRendered)
                    queue.Enqueue( n );
            };
            queue.Enqueue( root );

            while (queue.Count > 0) {
                while (queue.Count > 0) {
                    AST node = queue.Dequeue();

                    if (node.IsRendered)
                        continue;

                    if (node.IsText)
                        renderFactory.TextRender.Process( errors, node );
                    else {
                        if (node.InputTag.IsHeading) {
                            renderFactory.HeadingRender.Process( errors, node );
                        } else if (node.InputTag.IsLinebreak)
                            renderFactory.LinebreakRender.Process( errors, node );
                        else if (node.InputTag.IsParagraph)
                            renderFactory.ParagraphRender.Process( errors, node );
                        else {
                            IRender render = renderFactory.TagRenderes.FirstOrDefault( ( r ) => r.CanProcess( node.InputTag ) );
                            if (render != null)
                                render.Process( errors, node );
                            else
                                node.IsRendered = true;
                        }
                    }
                }
                queue.Clear();
                root.PreorderWalk( addNodes );
            }

            output = renderFactory.WrapWriter( output );
            root.Render( output );

            return errors;
        }

        /// <summary>
        /// Processes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output.</param>
        /// <param name="renderFactory">The render factory.</param>
        public static void Process( string input, TextWriter output, IRenderFactory renderFactory ) {
            using (StringReader reader = new StringReader( input ))
                Process( reader, output, renderFactory );
        }

        /// <summary>
        /// Processes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="renderFactory">The render factory.</param>
        /// <returns></returns>
        public static string Process( string input, IRenderFactory renderFactory ) {
            using (StringReader reader = new StringReader( input )) {
                using (StringWriter writer = new StringWriter()) {
                    Process( reader, writer, renderFactory );
                    return writer.ToString();
                }
            }
        }

        /// <summary>
        /// Processes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="renderFactory">The render factory.</param>
        /// <returns></returns>
        public static string Process( TextReader input, IRenderFactory renderFactory ) {
            using (StringWriter writer = new StringWriter()) {
                Process( input, writer, renderFactory );
                return writer.ToString();
            }
        }
    }
}

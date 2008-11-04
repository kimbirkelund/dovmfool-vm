using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Sekhmet.Collections;
using System.Linq;

namespace Sekhmet.Editing {
    /// <summary>
    /// Parser for the editing framework.
    /// </summary>
    class Parser {
        AST root;
        AST currentNode;
        TextReader reader;
        Tokenizer tokenizer;
        IRenderFactory renderFactory;
        TagHierachyNode rootTags;
        Stack<TagHierachyNode> tagsStack = new Stack<TagHierachyNode>();
        TagHierachyNode CurrentTags {
            get { return tagsStack.Peek(); }
        }
        bool lastRemoveWSAround = false;
        Errors errors;

        Parser( Errors errors, TextReader reader, IRenderFactory renderFactory ) {
            this.errors = errors;
            this.reader = reader;
            //this.tokenizer = new Lexer( reader );
            this.tokenizer = new Tokenizer( errors, reader );
            this.renderFactory = renderFactory;

            rootTags = new TagHierachyNode( "root", false, false, true );

            renderFactory.TagRenderes.ForEach(
                delegate( IRender render ) {
                    render.TagHierachy.ForEach(
                        delegate( TagHierachyNode node ) {
                            rootTags.Neighbors.Add( node );
                        } );
                } );
            tagsStack.Push( rootTags );
        }

        void ParseRoot() {
            root = new AST();
            currentNode = root;

            while (tokenizer.MoveNext()) {
                Token token = tokenizer.Current;

                if (token.IsHeading) {
                    AddAtRoot( new AST( Tag.FromToken( token ) ) );
                    if (!tokenizer.MoveNext())
                        continue;
                    Parse( tokenizer );
                    StepUp();
                } else if (token.IsTOC)
                    AddAtRoot( new AST( Tag.FromToken( token ) ) );
                else if (token.IsLinebreak || token.IsParagraph) {
                    if (currentNode.IsHeading)
                        currentNode = currentNode.Parent;
                } else if (currentNode.IsHeading && token.IsText)
                    currentNode.Children.Add( new AST( token.Text ) );
                else if (currentNode == root) {
                    AddAtRoot( new AST( ParagraphTag.Instance ) );

                    Parse( tokenizer );
                    SkipWS( tokenizer );

                    StepUp();
                } else
                    throw new ParsingException( new ParsingError( "Unexpected token: " + token, token.SourceSpan, token ) );
            }
            if (currentNode != root) {
                AST node = currentNode;
                while (node != root) {
                    if (node.IsLayout)
                        errors.Add( new ParsingError( "Unclosed tag: " + node.InputTag.Name, node.InputTag.SourceSpan ) );
                    node = node.Parent;
                }
            }
        }

        void Parse( Tokenizer lexer ) {
            do {
                Token token = lexer.Current;

                if (token.IsParagraph) {
                    if (currentNode.IsParagraph || currentNode.IsHeading)
                        return;

                    currentNode.Children.Add( new AST( LinebreakTag.Instance ) );
                    currentNode.Children.Add( new AST( LinebreakTag.Instance ) );

                    if (lexer.Peek != null && lexer.Peek.IsHeading)
                        return;
                } else if (token.IsText) {
                    lastRemoveWSAround = false;
                    if (token.IsWhitespace && CurrentTags.RemoveWSWithin)
                        continue;

                    if (!AssertTag( token ))
                        continue;
                    AST node = new AST( CurrentTags.RemoveWSWithin ? token.Text.Trim() : token.Text );
                    currentNode.Children.Add( node );
                } else if (token.IsTag) {
                    lastRemoveWSAround = false;
                    if (token.IsOpeningTag) {
                        if (!AssertTag( token )) {
                            AST node = new AST( token.ToString() );
                            currentNode.Children.Add( node );
                            continue;
                        }
                        AddAtCurrent( new AST( Tag.FromToken( token ) ) );
                    }

                    if (!AssertClosingTag( token, currentNode )) {
                        AST node = new AST( token.ToString() );
                        currentNode.Children.Add( node );
                        continue;
                    } else if (token.IsClosingTag) {
                        RemoveWSAround( currentNode );
                        StepUp();

                        tagsStack.Pop();
                    }
                } else if (token.IsLinebreak) {
                    if (lexer.Peek != null && lexer.Peek.IsHeading || currentNode.IsHeading)
                        return;

                    if (!lastRemoveWSAround && !CurrentTags.RemoveWSWithin)
                        currentNode.Children.Add( new AST( Tag.FromToken( token ) ) );
                } else
                    errors.Add( new ParsingError( "Unexpected token: " + token, token.SourceSpan, token ) );
            } while (lexer.MoveNext());
        }

        void SkipWS( Tokenizer lexer ) {
            while (lexer.Peek != null && lexer.Peek.IsWhitespace && lexer.MoveNext())
                ;
        }

        bool AssertTag( Token token ) {
            if (token.IsText)
                if (CurrentTags.AllowText)
                    return true;
                else {
                    errors.Add( new ParsingError( "Unexpted token: " + token, token.SourceSpan, token ) );
                    return false;
                }

            if (CurrentTags.AcceptNone) {
                errors.Add( new ParsingError( "Unexpted token: " + token, token.SourceSpan, token ) );
                return false;
            }

            TagHierachyNode tag;
            if (CurrentTags.AcceptAny)
                tag = rootTags.Neighbors.FirstOrDefault( delegate( TagHierachyNode node ) { return node.TagName == token.TagName.ToLower(); } );
            else
                tag = CurrentTags.Neighbors.FirstOrDefault( delegate( TagHierachyNode node ) { return node.TagName == token.TagName.ToLower(); } );

            if (tag == null) {
                errors.Add( new ParsingError( "Unexpted token: " + token, token.SourceSpan, token ) );
                return false;
            }
            tagsStack.Push( tag );
            return true;
        }

        bool AssertClosingTag( Token token, AST node ) {
            if (!token.IsClosingTag)
                return true;

            if (!node.IsLayout || node.InputTag.Name != token.TagName) {
                errors.Add( new ParsingError( "Unexpected closing tag: " + token, token.SourceSpan, token ) );
                return false;
            }

            return true;
        }

        bool IsHeading( AST node ) {
            return node.IsLayout && node.InputTag.IsHeading;
        }

        void RemoveWSAround( AST node ) {
            if (!CurrentTags.RemoveWSAround)
                return;

            lastRemoveWSAround = true;
            AST sibling;
            while ((sibling = node.LeftSibling) != null && sibling.IsWhitespace)
                sibling.Remove();
        }

        void AddAtRoot( AST node ) {
            if (currentNode != root)
                currentNode = currentNode.Parent;

            if (currentNode != root)
                throw new InvalidOperationException( "AddAtRoot should only be called when at most one step away from root." );

            currentNode.Children.Add( node );
            currentNode = node;
        }

        void AddAtCurrent( AST node ) {
            currentNode.Children.Add( node );
            currentNode = node;
        }

        void StepUp() {
            currentNode = currentNode.Parent;
        }

        public static AST Parse( Errors errors, TextReader reader, IRenderFactory renderFactory ) {
            Parser p = new Parser( errors, reader, renderFactory );
            p.ParseRoot();
            return p.root;
        }
    }
}

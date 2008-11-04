using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents the tokenizer of the editing framework.
    /// </summary>
    partial class Tokenizer {
        public const char TagBegin = '[';
        public const char TagEnd = ']';
        public const char FullLineTag = '!';
        public const string TOCTagName = "toc";
        public const char Space = ' ';
        public const char Tab = '\t';
        public const char NewLine = '\n';
        public const char EscapeChar = '\\';
        public const char Star = '*';
        public const char Slash = '/';
        public static readonly char[] Whitespace = new char[] { Space, Tab };

        /// <summary>
        /// Tokenizer helper class.
        /// </summary>
        class Helper : IEnumerable<Token> {
            TextReader input;
            string currentLine;
            int currentLineIndex = 0;
            Errors errors;

            int line = 0;
            int caret = 1;
            int overallPosition = 1;
            SourcePosition Position {
                get { return new SourcePosition( overallPosition, line, caret ); }
            }

            public Helper( Errors errors, TextReader input ) {
                this.errors = errors;
                this.input = input;
            }

            bool ReadLine() {
                currentLine = input.ReadLine();
                currentLineIndex = 0;
                if (currentLine != null) {
                    line++;
                    caret = 1;
                }
                return currentLine != null;
            }

            char Peek() {
                return currentLine[currentLineIndex];
            }

            char Next() {
                char c = Peek();
                currentLineIndex++;
                caret++;
                overallPosition++;
                return c;
            }

            string ReadUntil( char[] acceptSet, char[] failSet, out ParsingError error ) {
                error = null;
                StringBuilder sb = new StringBuilder();

                while (true) {
                    if (EOL())
                        return sb.ToString();
                    char c = Peek();

                    if (c == EscapeChar) {
                        //sb.Append( c );
                        Next();
                        if (EOL() || !new char[] { EscapeChar, FullLineTag, TagBegin, TagEnd }.Contains( Peek() ))
                            errors.Add( new ParsingError( "Invalid escape sequence: " + c + (EOL() ? "" : Peek().ToString()), SourceSpan.FromPosition( Position - 1, 2 ), true ) );
                        if (!EOL())
                            sb.Append( Next() );
                    } else if (acceptSet.Contains( c ))
                        return sb.ToString();
                    else if (failSet.Contains( c )) {
                        error = new ParsingError( "Unexpected character '" + c + "'. Expected one of: " + acceptSet.Join( ", " ), SourceSpan.FromPosition( Position, 1 ) );
                        sb.Append( Next() );
                        return sb.ToString();
                    } else
                        sb.Append( Next() );
                }
            }

            bool EOL() {
                return currentLine.IsNullOrEmpty() || currentLineIndex >= currentLine.Length;
            }

            bool EOF() {
                return input.Peek() == -1;
            }

            IEnumerable<Token> ReadToken() {
                while (true) {
                    ParsingError error;
                    if (EOL())
                        yield break;
                    SourcePosition startPos = Position;

                    if (currentLineIndex == 0 && Peek() == FullLineTag) {
                        string token = ReadUntil( Whitespace, new char[] { NewLine }, out error );
                        if (error != null) {
                            errors.Add( new ParsingError( "Malformed fullline tag.", SourceSpan.FromPosition( startPos, token.Length ), error ) );
                            yield return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), token );
                        } else {
                            int level;
                            bool numbered;
                            ParseHeadingProperties( token.Substring( 1 ), out level, out numbered, out error );
                            if (error != null) {
                                errors.Add( new ParsingError( "Malformed fullline tag.", SourceSpan.FromPosition( startPos, token.Length ), error ) );
                                yield return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), token );
                            } else
                                yield return Token.CreateHeading( SourceSpan.FromPosition( startPos, token.Length ), level, numbered );
                        }
                    } else {
                        string token = ReadUntil( new char[] { TagBegin }, new char[] { TagEnd }, out error );
                        if (error != null)
                            errors.Add( error );
                        if (!token.IsNullOrEmpty())
                            yield return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), token );

                        if (EOL())
                            continue;

                        token = Next() + ReadUntil( new char[] { TagEnd }, new char[] { TagBegin }, out error );
                        if (error != null || EOL()) {
                            errors.Add( new ParsingError( "Malformed tag: " + token, SourceSpan.FromPosition( startPos, token.Length ), error ) );
                            yield return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), token );
                        } else {
                            token += Next();
                            yield return ParseTag( token, startPos, out error );
                        }
                    }
                }
            }

            #region IEnumerable<Token> Members

            public IEnumerator<Token> GetEnumerator() {
                while (ReadLine()) {
                    foreach (var token in ReadToken())
                        yield return token;

                    if (!EOF())
                        yield return Token.CreateLinebreak( SourceSpan.FromPosition( Position, 1 ) );
                }
            }

            #endregion

            #region IEnumerable Members

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            #endregion

            void ParseHeadingProperties( string input, out int level, out bool numbered, out ParsingError error ) {
                error = null;
                numbered = true;
                string orgInput = input;
                if (input.EndsWith( Star.ToString() )) {
                    numbered = false;
                    input = input.Substring( 0, input.Length - 1 );
                }

                if (!int.TryParse( input, out level ))
                    error = new ParsingError( "Malformed heading properties: " + orgInput, SourceSpan.FromPosition( Position, orgInput.Length ) );
            }

            Token ParseTag( string token, SourcePosition startPos, out ParsingError error ) {
                error = null;
                string orgToken = token;
                if (!token.StartsWith( TagBegin ) || !token.EndsWith( TagEnd )) {
                    error = new ParsingError( "Malformed tag: '" + orgToken + "'", SourceSpan.FromPosition( startPos, token.Length ) );
                    return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), orgToken );
                }
                token = token.Substring( 1, token.Length - 2 ).Trim();
                if (token.IsNullOrEmpty()) {
                    errors.Add( new ParsingError( "Malformed tag: '" + orgToken + "'", SourceSpan.FromPosition( startPos, token.Length ) ) );
                    return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), orgToken );
                }

                bool tagOpen = false, tagClose = false;
                if (token.StartsWith( Slash )) {
                    tagClose = true;
                    token = token.Substring( 1 );
                } else
                    tagOpen = true;
                if (token.IsNullOrEmpty()) {
                    errors.Add( new ParsingError( "Malformed tag: '" + orgToken + "'", SourceSpan.FromPosition( startPos, orgToken.Length ) ) );
                    return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), orgToken );
                }
                if (token.EndsWith( Slash )) {
                    tagClose = true;
                    token = token.Substring( 0, token.Length - 1 );
                }
                if (token.IsNullOrEmpty()) {
                    errors.Add( new ParsingError( "Malformed tag: '" + orgToken + "'", SourceSpan.FromPosition( startPos, orgToken.Length ) ) );
                    return Token.CreateText( SourceSpan.FromPosition( startPos, token.Length ), orgToken );
                }

                int nameEnd = token.IndexOfAny( Whitespace );
                string name = token.Substring( 0, nameEnd == -1 ? token.Length : nameEnd );
                token = token.Substring( nameEnd == -1 ? token.Length : nameEnd );

                IEnumerable<Property> props = null;
                if (tagOpen)
                    props = PropertyParser.Parse( token );

                return Token.CreateTag( SourceSpan.FromPosition( startPos, orgToken.Length ), name, tagOpen, tagClose, props );
            }
        }
    }
}
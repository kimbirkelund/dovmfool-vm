using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sekhmet.Editing {
    /// <summary>
    /// Represents the tokenizer of the editing framework.
    /// </summary>
    partial class Tokenizer {
        IEnumerator<Token> helper;

        Token current, peek;

        public Token Current {
            get { return current; }
        }

        public Token Peek {
            get { return peek; }
        }

        public Tokenizer( Errors errors, TextReader input ) {
            helper = new Helper( errors, input ).GetEnumerator();
            if (helper.MoveNext())
                peek = helper.Current;
        }

        public bool MoveNext() {
            if (peek == null)
                return false;

            current = peek;

            peek = null;
            if (helper.MoveNext())
                peek = helper.Current;

            if (!current.IsLinebreak || peek == null || !peek.IsLinebreak)
                return true;

            current = Token.CreateParagraph( current.SourceSpan );
            peek = null;
            if (helper.MoveNext())
                peek = helper.Current;

            return true;
        }
    }
}

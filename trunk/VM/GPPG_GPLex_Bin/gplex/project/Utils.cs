// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;

namespace GPLEX.Parser
{

    /// <summary>
    /// This class supplies character escape 
    /// utilities for project.
    /// </summary>
    public static class Utils
    {
        #region Character escape utilities
        static int symLimit = 255; // default is 8-bit
        public static void SetUnicode() { symLimit = 0xffff; }
        //
        // Utility procedures for handling numeric character escapes
        // and mapping the usual ANSI C escapes.
        //
        public static bool IsOctDigit(char ch) { return ch >= '0' && ch <= '7'; }

        public static char OctChar(char a, char b, char c)
        {
            int x = (int)a - (int)'0';
            int y = (int)b - (int)'0';
            int z = (int)c - (int)'0';
            return (char)(((x * 8) + y) * 8 + z);
        }

        public static char HexChar(char a, char b)
        {
            int x = (char.IsDigit(a) ? (int)a - (int)'0' : (int)char.ToLower(a) + (10 - (int)'a'));
            int y = (char.IsDigit(b) ? (int)b - (int)'0' : (int)char.ToLower(b) + (10 - (int)'a'));
            return (char)(x * 16 + y);
        }

        public static int ValOfHex(char x)
        {
            return ((x >= '0' && x <= '9') ? (int)x - (int)'0' : (int)char.ToLower(x) + (10 - (int)'a'));
        }

        public static bool IsHexDigit(char ch)
        {
            ch = char.ToLower(ch);
            return ch >= '0' && ch <= '9' || ch >= 'a' && ch <= 'f';
        }
        /// <summary>
        /// Get a substring of length len from the input string 
        /// input, starting from the index ix. If there are not 
        /// len characters left return rest of string.
        /// </summary>
        /// <param name="input">the input string</param>
        /// <param name="ix">the starting index</param>
        /// <param name="len">the requested length</param>
        /// <returns></returns>
        public static string GetSubstring(string input, int ix, int len)
        {
            if (input.Length - ix >= len)
                return input.Substring(ix, len);
            else
                return input.Substring(ix);
        }

        /// <summary>
        /// Assert: special case of '\0' is already filtered out.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static int GetOctalChar(string str)
        {
            if (str.Length != 3)
                return -1;
            if (IsOctDigit(str[0]) && IsOctDigit(str[1]) && IsOctDigit(str[2]))
                return OctChar(str[0], str[1], str[2]);
            else
                return -1;
        }

        public static int GetHexadecimalChar(string str)
        {
            if (str.Length != 2)
                return -1;
            if (IsHexDigit(str[0]) && IsHexDigit(str[1]))
                return HexChar(str[0], str[1]);
            else
                return -1;
        }

        public static int GetUnicodeChar(string str)
        {
            int rslt = 0;
            if (str.Length < 4)
                return -1;
            for (int i = 0; i < 4; i++)
            {
                if (!IsHexDigit(str[i]))
                    return -1;
                rslt = rslt * 16 + ValOfHex(str[i]);
            }
            return rslt;
        }

        /// <summary>
        /// This static method expands characters in a literal 
        /// string, returning a modified string with character
        /// escapes replaced by the character which they denote.
        /// </summary>
        /// <param name="str">the input string</param>
        /// <returns>interpreted version of the string</returns>
        public static string InterpretCharacterEscapes(string str)
        {
            int sLen = str.Length;
            if (sLen == 0)
                return str;
            char[] arr = new char[sLen];
            int sNxt = 0;
            int aNxt = 0;
            char chr = str[sNxt++];
            for (; ; )
            {
                if (chr == '\\')
                    arr[aNxt++] = EscapedChar(str, ref sNxt);
                else
                    arr[aNxt++] = chr;
                if (sNxt == sLen)
                    return new String(arr, 0, aNxt);
                chr = str[sNxt++];
            }
        }

        /// <summary>
        /// Find the character denoted by the character escape
        /// starting with the backslash at position (index - 1).
        /// Postcondition: str[index] is the first character
        /// beyond the character escape denotation.
        /// </summary>
        /// <param name="str">the string to parse</param>
        /// <param name="index">the in-out index</param>
        /// <returns>the character denoted by the escape</returns>
        public static char EscapedChar(string str, ref int index)
        {
            char chr = str[index++];
            int valu;
            switch (chr)
            {
                case '\\': return '\\';
                case 'a': return '\a';
                case 'b': return '\b';
                case 'f': return '\f';
                case 'n': return '\n';
                case 'r': return '\r';
                case 't': return '\t';
                case 'v': return '\v';

                case '0':
                case '1':
                case '2':
                case '3':
                    if (chr == '0' && !IsOctDigit(str[index]))
                        return '\0';
                    valu = GetOctalChar(GetSubstring(str, index - 1, 3)); index += 2;
                    if (valu >= 0)
                        return (char)valu;
                    else
                        throw new RegExException(86, index-4, 4, GetSubstring(str, index - 4, 4));
                case 'x': case 'X':   // Just being nice here.
                    valu = GetHexadecimalChar(GetSubstring(str, index, 2)); index += 2;
                    if (valu >= 0)
                        return (char)valu;
                    else
                        throw new RegExException(87, index - 4, 4, GetSubstring(str, index - 4, 4));
                case 'u': case 'U':   // Just being nice here.
                    valu = GetUnicodeChar(GetSubstring(str, index, 4)); index += 4;
                    if (valu < 0)
                        throw new RegExException(88, index - 6, 6, GetSubstring(str, index - 6, 6));
                    else if (valu > symLimit)
                        throw new RegExException(85, index - 6, 6, GetSubstring(str, index - 6, 6));
                    else
                        return (char)valu;
                default:
                    break;
            }
            return chr;
        }

        /// <summary>
        /// map string characters into the display form 
        /// </summary>
        /// <param name="chr">the character to encode</param>
        /// <returns>the string denoting chr</returns>
        public static string Map(int chr)
        {
            if (chr > (int)' ' && chr < 127) return new String((char)chr, 1);
            switch (chr)
            {
                case (int)'\0': return "\\0";
                case (int)'\n': return "\\n";
                case (int)'\t': return "\\t";
                case (int)'\r': return "\\r";
                case (int)'\\': return "\\\\";
                default:
                    if (chr < 256)
                        return "\\" +
                            (char)((int)'0' + chr / 64) +
                            (char)((int)'0' + chr / 8 % 8) +
                            (char)((int)'0' + chr % 8);
                    else // use unicode literal
                        return String.Format("\\u{0:X4}", (ushort)chr);
            }
        }

        public static string Map(string str)
        {
            string rslt = "";
            if (str != null)
                for (int i = 0; i < str.Length; i++)
                    rslt += Map(str[i]);
            return rslt;
        }

        #endregion Character escape utilities
 
    }
}

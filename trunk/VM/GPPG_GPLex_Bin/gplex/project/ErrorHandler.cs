// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace GPLEX.Parser
{
    public class Error : IComparable<Error>
    {
        internal const int minErr = 50;
        internal const int minWrn = 100;

        public bool isWarn;
        public string message;
        public LexSpan span;


        internal Error(string msg, LexSpan spn, bool wrn)
        {
            isWarn = wrn;
            message = msg;
            span = spn;
        }

        internal int Length { get { return span.eCol - span.sCol; } }

        public int CompareTo(Error r)
        {
            if (span.sLin < r.span.sLin) return -1;
            else if (span.sLin > r.span.sLin) return 1;
            else if (span.sCol < r.span.sCol) return -1;
            else if (span.sCol > r.span.sCol) return 1;
            else return 0;
        }

        public bool Equals(Error r)
        {
            return (span.Equals(r.span));
        }

        //public void Report()
        //{
        //    Console.WriteLine("Line {0}, column {2} : {3}", span.sLin, span.sCol, message);
        //}

    }
    
    
    public class ErrorHandler
    {
        List<Error> errors;
        int errNum = 0;
        int wrnNum = 0; 

        public bool Errors { get { return errNum > 0; } }
        public bool Warnings { get { return wrnNum > 0; } }


        public int ErrNum { get { return errNum; } }
        public int WrnNum { get { return wrnNum; } }

        public ErrorHandler()
        {
            errors = new List<Error>(8);
        }
        // -----------------------------------------------------
        //   Public utility methods
        // -----------------------------------------------------

 
        public List<Error> SortedErrorList()
        {
            if (errors.Count > 1) errors.Sort();
            return errors;
        }

        public void AddError(string msg, LexSpan spn)
        {
            errors.Add(new Error(msg, spn, false)); errNum++;
        }

        public void AddWarning(string msg, LexSpan spn)
        {
            errors.Add(new Error(msg, spn, true)); wrnNum++;
        }

        /// <summary>
        /// Add this error to the error buffer.
        /// </summary>
        /// <param name="spn">The span to which the error is attached</param>
        /// <param name="num">The error number</param>
        /// <param name="key">The featured string</param>
        public void ListError(LexSpan spn, int num, string key)
        {
            string prefix, suffix, message;
            switch (num)
            {
                case   1: prefix = "Parser error"; suffix = "";
                    break;
                case  50: prefix = "Start state"; suffix = "already defined";
                    break;
                case  51: prefix = "Start state"; suffix = "undefined";
                    break;
                case  52: prefix = "Lexical category"; suffix = "already defined";
                    break;
                case  53: prefix = "Expected character"; suffix = "";
                    break;
                case 55: prefix = "Unknown lexical category"; suffix = "";
                    break;
                case 61: prefix = "Missing matching construct"; suffix = "";
                    break;
                case 62: prefix = "Unexpected symbol, skipping to "; suffix = "";
                    break;
                case 70: prefix = "Illegal character escape "; suffix = "";
                    break;
                //case 71: prefix = "Illegal numeric character escape "; suffix = "";
                //    break;
                case 72: prefix = "Illegal name for start condition "; suffix = "";
                    break;
                case 74: prefix = "Unrecognized \"%option\" command "; suffix = "";
                    break;
                case 76: prefix = "Unknown character predicate"; suffix = "";
                    break;
                case 83: prefix = "Cannot set /unicode option inconsistently"; suffix = "";
                    break;
                case 84: prefix = "Inconsistent \"%option\" command "; suffix = "";
                    break;
                case 85: prefix = "Unicode literal too large:"; suffix = "use %option unicode";
                    break;
                case 86: prefix = "Illegal octal character escape "; suffix = "";
                    break;
                case 87: prefix = "Illegal hexadecimal character escape "; suffix = "";
                    break;
                case 88: prefix = "Illegal unicode character escape "; suffix = "";
                    break;
                case 111: prefix = "This char"; suffix = "does not need escape in character class";
                    break;
                case 113: prefix = "Special case:"; suffix = "included as set class member";
                    break;
                case 114: prefix = "No upper bound to range,"; suffix = "included as set class members";
                    break;
                default: prefix = "Error " + Convert.ToString(num); suffix = "";
                    break;
            }
            message = prefix + " <" + key + "> " + suffix;
            errors.Add(new Error(message, spn, num >= Error.minWrn)); 
            if (num < Error.minWrn) errNum++; else wrnNum++;
        }

        public void ListError(LexSpan spn, int num)
        {
            string message;
            switch (num)
            {
                case 54: message = "Invalid character range: lower bound > upper bound"; break;
                case 56: message = "\"using\" is illegal, use \"%using\" instead"; break;
                case 57: message = "\"namespace\" is illegal, use \"%namespace\" instead"; break;
                case 58: message = "Type declarations impossible in this context"; break;
                case 59: message = "\"next\" action '|' cannot be used on last pattern"; break;
                case 60: message = "Unterminated block comment"; break;
                case 63: message = "Invalid single-line action"; break;
                case 64: message = "This token unexpected"; break;
                case 65: message = "Invalid action"; break;
                case 66: message = "Missing comma in namelist"; break;
                case 67: message = "Invalid or empty namelist"; break;
                case 68: message = "Invalid production rule"; break;
                case 69: message = "Symbols '^' and '$' can only occur at the ends of patterns"; break;
                // case 71: message = " "; break;
                case 73: message = "No namespace has been defined"; break;
                case 75: message = "Context must have fixed right length or fixed left length"; break;
                case 77: message = "Unknown LEX tag name"; break;
                case 78: message = "Expected space here"; break;
                case 79: message = "Illegal character in this context"; break;
                case 80: message = "Expected end-of-line here"; break;
                case 81: message = "Invalid character range, no upper bound character"; break;
                case 82: message = "Invalid class character: '-' must be \\escaped"; break;
                case 110: message = "Code between rules, ignored"; break;
                case 112: message = "/babel option is unsafe without /unicode option"; break;
                default:  message = "Error " + Convert.ToString(num); break;
            }
            errors.Add(new Error(message, spn, num >= Error.minWrn));
            if (num < Error.minWrn) errNum++; else wrnNum++;
        }
 
        
        // -----------------------------------------------------
        //   Error Listfile Reporting Method
        // -----------------------------------------------------

        public void MakeListing(GPLEX.Lexer.ScanBuff buff, StreamWriter sWrtr, string name, string version)
        {
            int line = 1;
            int eNum = 0;
            int eLin = 0;

            int nxtC = (int)'\n';
            int groupFirst;
            int currentCol;
            int currentLine;

            //
            //  Errors are sorted by line number
            //
            errors = SortedErrorList();
            //
            //  Reset the source file buffer to the start
            //
            buff.Pos = 0;
            sWrtr.WriteLine(); 
            ListDivider(sWrtr);
            sWrtr.WriteLine("//  GPLEX error listing for lex source file <"
                                                           + name + ">");
            ListDivider(sWrtr);
            sWrtr.WriteLine("//  Version:  " + version);
            sWrtr.WriteLine("//  Machine:  " + Environment.MachineName);
            sWrtr.WriteLine("//  DateTime: " + DateTime.Now.ToString());
            sWrtr.WriteLine("//  UserName: " + Environment.UserName);
            ListDivider(sWrtr); sWrtr.WriteLine(); sWrtr.WriteLine();
            //
            //  Initialize the error group
            //
            groupFirst = 0;
            currentCol = 0;
            currentLine = 0;
            //
            //  Now, for each error do
            //
            for (eNum = 0; eNum < errors.Count; eNum++)
            {
                Error errN = errors[eNum];
                eLin = errN.span.sLin;
                if (eLin > currentLine)
                {
                    //
                    // Spill all the waiting messages
                    //
                    if (currentCol > 0)
                    {
                        sWrtr.WriteLine();
                        currentCol = 0;
                    }
                    for (int i = groupFirst; i < eNum; i++)
                    {
                        sWrtr.Write("// Error: ");
                        sWrtr.Write(errors[i].message);
                        sWrtr.WriteLine();
                    }
                    if (groupFirst < eNum)
                    {
                        Spaces(sWrtr, errors[eNum - 1].message.Length + 10);
                        sWrtr.WriteLine();
                    }
                    currentLine = eLin;
                    groupFirst = eNum;
                }
                //
                //  Emit lines up to *and including* the error line
                //
                while (line <= eLin)
                {
                    nxtC = buff.Read();
                    if (nxtC == (int)'\n')
                        line++;
                    else if (nxtC == GPLEX.Lexer.ScanBuff.EOF)
                        break;
                    sWrtr.Write((char)nxtC);
                }
                //
                //  Now emit the error message(s)
                //
                if (errN.span.sCol > 2 && errN.span.sCol < 80)
                {
                    if (currentCol == 0)
                    {
                        sWrtr.Write("//");
                        currentCol = 3;
                    }
                    Spaces(sWrtr, errN.span.sCol - currentCol + 1);
                    for (int j = 0; j < errN.Length && j + currentCol < 75; j++)
                        sWrtr.Write('^');
                    currentCol = errN.span.sCol + 1;
                }
            }
            //
            //  Clean up after last message listing
            //  Spill all the waiting messages
            //
            if (currentCol > 0)
            {
                sWrtr.WriteLine();
            }
            for (int i = groupFirst; i < errors.Count; i++)
            {
                sWrtr.Write("// Error: ");
                sWrtr.Write(errors[i].message);
                sWrtr.WriteLine();
            }
            if (groupFirst < errors.Count)
            {
                Spaces(sWrtr, errors[errors.Count - 1].message.Length + 10);
                sWrtr.WriteLine();
            }
            //
            //  And dump the tail of the file
            //
            nxtC = buff.Read();
            while (nxtC != GPLEX.Lexer.ScanBuff.EOF)
            {
                sWrtr.Write((char)nxtC);
                nxtC = buff.Read();
            }
            ListDivider(sWrtr); sWrtr.WriteLine();
            sWrtr.Flush();
            // sWrtr.Close();
        }

        public void ListDivider(StreamWriter wtr)
        {
            wtr.WriteLine(
            "// =========================================================================="
            );
        }

        public void Spaces(StreamWriter wtr, int len)
        {
            for (int i = 0; i < len; i++) wtr.Write('-');
        }


        // -----------------------------------------------------
        //   Console Error Reporting Method
        // -----------------------------------------------------

        public void DumpAll(GPLEX.Lexer.ScanBuff buff, TextWriter wrtr) {
            int  line = 1;
            int  eNum = 0;
            int  eLin = 0;
            int nxtC = (int)'\n'; 
            //
            //  Initialize the error group
            //
            int groupFirst = 0;
            int currentCol = 0;
            int currentLine = 0;
            //
            //  Reset the source file buffer to the start
            //
            buff.Pos = 0;
            wrtr.WriteLine("Error Summary --- ");
            //
            //  Initialize the error group
            //
            groupFirst = 0;
            currentCol = 0;
            currentLine = 0;
            //
            //  Now, for each error do
            //
            for (eNum = 0; eNum < errors.Count; eNum++) {
                eLin = errors[eNum].span.sLin;
                if (eLin > currentLine) {
                    //
                    // Spill all the waiting messages
                    //
                    if (currentCol > 0) {
                        wrtr.WriteLine();
                        currentCol = 0;
                    }
                    for (int i = groupFirst; i < eNum; i++) {
                        Error err = errors[i];
                        wrtr.Write((err.isWarn ? "Warning: " : "Error: "));
                        wrtr.Write(err.message);    
                        wrtr.WriteLine();    
                    }
                    currentLine = eLin;
                    groupFirst  = eNum;
                } 
                //
                //  Skip lines up to *but not including* the error line
                //
                while (line < eLin) {
                    nxtC = buff.Read();
                    if (nxtC == (int)'\n') line++;
                    else if (nxtC == GPLEX.Lexer.ScanBuff.EOF) break;
                } 
                //
                //  Emit the error line
                //
                if (line <= eLin) {
                    wrtr.Write((char)((eLin/1000)%10+(int)'0'));
                    wrtr.Write((char)((eLin/100)%10+(int)'0'));
                    wrtr.Write((char)((eLin/10)%10+(int)'0'));
                    wrtr.Write((char)((eLin)%10+(int)'0'));
                    wrtr.Write(' ');
                    while (line <= eLin) {
                        nxtC = buff.Read();
                        if (nxtC == (int)'\n') line++;
                        else if (nxtC == GPLEX.Lexer.ScanBuff.EOF) break;
                        wrtr.Write((char)nxtC);
                    } 
                } 
                //
                //  Now emit the error message(s)
                //
                if (errors[eNum].span.sCol > 0 && errors[eNum].span.sCol < 75) {
                    if (currentCol == 0) {
                        wrtr.Write("----");
                    }
                    for (int i = currentCol; i < errors[eNum].span.sCol; i++) {
                        wrtr.Write('-');
                    } 
                    wrtr.Write(' ');
                    for (int j = 0; j < errors[eNum].Length && j + currentCol < 75; j++)
                        wrtr.Write('^');
                    currentCol = errors[eNum].span.sCol+1;
                }
            }
            //
            //  Clean up after last message listing
            //  Spill all the waiting messages
            //
            if (currentCol > 0) {
                wrtr.WriteLine();
            }
            for (int i = groupFirst; i < errors.Count; i++) {
                Error err = errors[i];
                wrtr.Write((err.isWarn ? "Warning: " : "Error: "));
                wrtr.Write(errors[i].message);    
                wrtr.WriteLine();    
            }
        } 
    }
}
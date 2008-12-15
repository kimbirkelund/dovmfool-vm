// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf.

//  Parse helper for bootstrap version of gplex
//  kjg 17 June 2006
//

using System;
using System.IO;
using System.Collections.Generic;
using GPLEX.Lexer;

namespace GPLEX.Parser
{
    public delegate GPLEX.Automaton.OptionState OptionParser2(string s);

    public class LexSpan : gppg.IMerge<LexSpan>
    {
        public int sLin;       // start line of span
        public int sCol;       // start column of span
        public int eLin;       // end line of span
        public int eCol;       // end column of span
        public int sPos;       // start position in the buffer
        public int ePos;       // end position in the buffer
        public ScanBuff buff;  // reference to the buffer

        public LexSpan() { }
        public LexSpan(int sl, int sc, int el, int ec, int sp, int ep, ScanBuff bf)
        { sLin = sl; sCol = sc; eLin = el; eCol = ec; sPos = sp; ePos = ep; buff = bf; }

        /// <summary>
        /// This method implements the IMerge interface
        /// </summary>
        /// <param name="end">The last span to be merged</param>
        /// <returns>A span from the start of 'this' to the end of 'end'</returns>
        public LexSpan Merge(LexSpan end)
        {
            return new LexSpan(sLin, sCol, end.eLin, end.eCol, sPos, end.ePos, buff); 
        }

        /// <summary>
        /// Get a short span from the first line of this span.
        /// </summary>
        /// <param name="idx">Starting index</param>
        /// <param name="len">Length of span</param>
        /// <returns></returns>
        public LexSpan FirstLineSubSpan(int idx, int len)
        {
            if (this.eLin != this.sLin) throw new Exception("Cannot index into multiline span");
            return new LexSpan(
                this.sLin, this.sCol + idx, this.sLin, this.sCol + idx + len,
                this.sPos, this.ePos, this.buff);
        }

        public bool IsInitialized { get { return buff != null; } }

        public void StreamDump(TextWriter sWtr)
        {
            int savePos = buff.Pos;
            string str = buff.GetString(sPos, ePos);
            sWtr.WriteLine(str);
            buff.Pos = savePos;
            sWtr.Flush();
        }

        public void ConsoleDump()
        {
            int savePos = buff.Pos;
            string str = buff.GetString(sPos, ePos);
            Console.WriteLine(str);
            buff.Pos = savePos; 
        }
    }

    public partial class Parser
    {
        static LexSpan blank = new LexSpan();  // marked by buff == null
        public LexSpan Blank { get { return blank; } }
 
        ErrorHandler handler = null;

        AAST aast;
        public AAST Aast { get { return aast; } }

        OptionParser2 processOption2;

        RuleBuffer rb = new RuleBuffer();
        bool typedeclOK = true;
        bool isBar = false;

        /// <summary>
        /// The runtime parser support expects the scanner to be of 
        /// abstract IScanner type. The abstract syntax tree object
        /// has a handle on the concrete objects so that semantic
        /// actions can get the extra functionality without a cast.
        /// </summary>
        /// <param name="scnr"></param>
        /// <param name="hdlr"></param>
        public void Initialize(GPLEX.Automaton.TaskState t, Scanner scnr, ErrorHandler hdlr, OptionParser2 dlgt)
        {
            this.handler = hdlr;
            this.scanner = scnr;
            this.aast = new AAST(t);
            this.aast.hdlr = hdlr;
            this.aast.parser = this;
            this.aast.scanner = scnr;
            this.processOption2 = dlgt;
        }

        public AAST.Destination Dest
        {
            get { // only the first declaration can go in the usingDcl group
                return AAST.Destination.codeIncl;
            }
        }

        List<LexSpan> nameLocs = new List<LexSpan>();
        List<string> nameList = new List<string>();
        List<StartState> scList;

        internal void AddName(LexSpan l)
        {
            nameLocs.Add(l);
            nameList.Add(aast.scanner.buffer.GetString(l.sPos, l.ePos));
        }

        /// <summary>
        /// This method adds newly defined start condition names to the
        /// table of start conditions. 
        /// </summary>
        /// <param name="isExcl">True iff the start condition is exclusive</param>
        internal void AddNames(bool isExcl)
        {
            for (int i = 0; i < nameList.Count; i++)
            {
                string s = nameList[i];
                LexSpan l = nameLocs[i];
                if (Char.IsDigit(s[0])) handler.ListError(l, 72, s); 
                else if (!aast.AddState(isExcl, s)) handler.ListError(l, 50, s);
            }
            // And now clear the nameList
            nameList.Clear();
            nameLocs.Clear();
        }

        /// <summary>
        /// Parse a line of option commands.  
        /// These may be either whitespace or comma separated
        /// </summary>
        /// <param name="l">The LexSpan of all the commands on this line</param>
        internal void ParseOption(LexSpan l)
        {
            char[] charSeparators = new char[] { ',', ' ', '\t' };
            string strn = aast.scanner.buffer.GetString(l.sPos, l.ePos);
            string[] cmds = strn.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in cmds)
            {
                Automaton.OptionState rslt = this.processOption2(s);
                switch (rslt)
                {
                    case Automaton.OptionState.clear:
                        break;
                    case Automaton.OptionState.errors:
                        handler.ListError(l, 74, s);
                        break;
                    case Automaton.OptionState.inconsistent:
                        handler.ListError(l, 84, s);
                        break;
                    case Automaton.OptionState.alphabetLocked:
                        handler.ListError(l, 83, s);
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Used to validate used occurrences of start condition lists
        /// these may include the number '0' as a synonym for "INITIAL"
        /// </summary>
        /// <param name="lst">The list of supposed start state names</param>
        internal void CheckScList(List<StartState> lst)
        {
            for (int i = 0; i < nameList.Count; i++)
            {
                string s = nameList[i];
                LexSpan l = nameLocs[i];
                if (s.Equals("0")) s = "INITIAL";

                if (Char.IsDigit(s[0])) handler.ListError(l, 72, s); // Illegal name
                else
                {
                    StartState obj = aast.StartStateValue(s);
                    if (obj == null) handler.ListError(l, 51, s);
                    else lst.Add(obj);
                }
            }
            nameList.Clear();
            nameLocs.Clear();
        }

        internal void AddLexCategory(LexSpan nLoc, LexSpan vLoc)
        {
            // string name = aast.scanner.buffer.GetString(nVal.sPos, nVal.ePos + 1);
            string name = aast.scanner.buffer.GetString(nLoc.sPos, nLoc.ePos);
            string verb = aast.scanner.buffer.GetString(vLoc.sPos, vLoc.ePos);
            if (!aast.AddLexCategory(name, verb, vLoc))
                handler.AddError("Error: name " + name + " already defined", nLoc);
        }

    }

} // end of namespace

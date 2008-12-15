// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace GPLEX.Parser
{
    /// <summary>
    /// This class represents the Attributed Abstract Syntax Tree
    /// corresponding to an input LEX file.
    /// </summary>
	public sealed class AAST
	{
        internal Parser parser;
        internal GPLEX.Lexer.Scanner scanner;
        internal ErrorHandler hdlr;

        public List<LexSpan> prolog   = new List<LexSpan>();   // Verbatim declarations for scanning routine
        public List<LexSpan> epilog   = new List<LexSpan>();   // Epilog code for the scanning routine
        public List<LexSpan> codeIncl = new List<LexSpan>();   // Text to copy verbatim into output file
        public List<LexSpan> usingStrs = new List<LexSpan>();  // "using" dotted names
        public LexSpan nameString;                             // Namespace dotted name
        public LexSpan userCode;                               // Text from the user code section
        public List<RuleDesc> ruleList = new List<RuleDesc>();
        Dictionary<string, LexCategory> lexCategories = new Dictionary<string, LexCategory>();
        public Dictionary<string, StartState> startStates = new Dictionary<string, StartState>();
        private List<StartState> inclStates = new List<StartState>();

        GPLEX.Automaton.TaskState task;

        public enum Destination {scanProlog, scanEpilog, codeIncl}

		public AAST(GPLEX.Automaton.TaskState t) {
            task = t;
            startStates.Add(StartState.initState.Name, StartState.initState);
            startStates.Add(StartState.allState.Name, StartState.allState);
		}

        public LexSpan UserCode
        {
            get { return userCode; }
            set { userCode = value; }
        }

        public void AddCodeSpan(Destination dest, LexSpan span)
        {
            if (!span.IsInitialized) return;
            switch (dest)
            {
                case Destination.codeIncl: codeIncl.Add(span); break;
                case Destination.scanProlog: prolog.Add(span); break;
                case Destination.scanEpilog: epilog.Add(span); break;
            }
        }
       

        public bool AddLexCategory(string name, string verb, LexSpan spn)
        {
            if (lexCategories.ContainsKey(name))
                return false;
            else
            {
                LexCategory cls = new LexCategory(name, verb, spn);
                lexCategories.Add(name, cls);
                cls.ParseRE(this);
                return true;
            }
        }

        public bool LookupLexCategory(string name)
        { return lexCategories.ContainsKey(name); }

        public bool AddState(bool isX, string name)
        {
            if (name != null)
                if (startStates.ContainsKey(name))
                    return false;
                else
                {
                    StartState state = new StartState(isX, name);
                    startStates.Add(name, state);
                    if (!isX)
                        inclStates.Add(state);
                }
            return true;
        }

        public StartState StartStateValue(string name)
        {
            StartState state;
            return (startStates.TryGetValue(name, out state) ? state : null);
        }

        public int StartStateCount { get { return startStates.Count; } }

        public void AddToAllStates(RuleDesc rule)
        {
            foreach (KeyValuePair<string, StartState> p in startStates)
            {
                StartState s = p.Value;
                if (!s.IsAll) s.AddRule(rule);
            }
        }

        public void FixupBarActions()
        {
            LexSpan lastSpan = parser.Blank;
            for (int i = ruleList.Count-1; i >= 0; i--)
            {
                RuleDesc rule = ruleList[i];
                if (!rule.isBarAction) lastSpan = rule.aSpan;
                else if (!lastSpan.IsInitialized)
                    hdlr.ListError(rule.pSpan, 59);
                else rule.aSpan = lastSpan;
                AddRuleToList(rule);
            }
        }

        internal void AddRuleToList(RuleDesc rule)
        {
            //
            // Versions before 0.4.2.* had incorrect semantics
            // for the handling of inclusive start states.
            // Correct semantics for inclusive start states:
            // If a rule has no explicit start state(s) then it
            // should be added to *every* inclusive start state.
            //
            // For version 0.5.1+ the semantics follow those of
            // FLEX, which distinguishes between rules that are
            // *explicitly* attached to INITIAL, and those which
            // have an empty start state list.  Only those without
            // a start state list are added to inclusive states.
            //

            if (rule.list == null)
            {
                StartState.initState.AddRule(rule);       // Add to initial state
                foreach (StartState inclS in inclStates)  // Add to inclusive states
                    inclS.AddRule(rule);
            }
            else if (rule.list[0].IsAll)
                AddToAllStates(rule);
            else
                foreach (StartState state in rule.list)
                    state.AddRule(rule);
        }

        public LexSpan AtStart
        { get { LexSpan tmp = new LexSpan(1,1,1,1,0,0,scanner.buffer); return tmp; } }

        /// <summary>
        /// NESTED CLASS
        /// Regular expression parser -- no error recovery attempted
        /// just throw an exception and abandon the whole pattern.
        /// This is a hand-written, recursive descent parser.
        /// </summary>
        public sealed class ReParser
        {
            BitArray prStart;
            Dictionary<string, Leaf> cats = null; // Allocated on demand

            const char NUL = '\0';
            int symCard;
            int index = 0;          // index of the *next* character to be read
            char chr;               // the last character to be read.
            bool esc;               // the last character was backslash-escaped
            AAST parent;
            LexSpan span;
            string pat;

            /// <summary>
            /// Defines the character set special to regular expressions
            /// and the valid characters to start syntatic category "Primary"
            /// </summary>
            /// <param name="crd">host alphabet cardinality</param>
            void InitReParser() {
                prStart = new BitArray(symCard, true);
                prStart[(int)')'] = false;
                prStart[(int)'|'] = false;
                prStart[(int)'*'] = false;
                prStart[(int)'+'] = false;
                prStart[(int)'?'] = false;
                prStart[(int)'}'] = false;
                prStart[(int)'/'] = false;
                prStart[(int)')'] = false;
                prStart[(int)'$'] = false;
                prStart[(int)'/'] = false;                
                prStart[(int)'\0'] = false;
            }

            /// <summary>
            /// This method lazily constructs the dictionary for the
            /// character predicates.  Beware however, that this just
            /// maps the first "crd" characters of the unicode value set.
            /// </summary>
            /// <param name="crd">target alphabet cardinality</param>
            private void InitCharCats()
            {
                cats = new Dictionary<string, Leaf>();
                cats.Add("IsControl", new Leaf(RegOp.charClass));
                cats.Add("IsDigit", new Leaf(RegOp.charClass));
                cats.Add("IsLetter", new Leaf(RegOp.charClass));
                cats.Add("IsLetterOrDigit", new Leaf(RegOp.charClass));
                cats.Add("IsLower", new Leaf(RegOp.charClass));
                cats.Add("IsNumber", new Leaf(RegOp.charClass));
                cats.Add("IsPunctuation", new Leaf(RegOp.charClass));
                cats.Add("IsSeparator", new Leaf(RegOp.charClass));
                cats.Add("IsSymbol", new Leaf(RegOp.charClass));
                cats.Add("IsUpper", new Leaf(RegOp.charClass));
                cats.Add("IsWhiteSpace", new Leaf(RegOp.charClass));
            }

            public ReParser(string str, LexSpan spn, AAST parent) {
                if (parent.task.Unicode)
                    Utils.SetUnicode();
                symCard = parent.task.HostSymCardinality;
                pat = str;
                span = spn;
                InitReParser();
                this.parent = parent;
            }

            public RegExTree Parse()
            {
                try {
                    RegExTree tmp;
                    scan();
                    tmp = RegEx();
                    return tmp;
                } catch (RegExException x) {
                    x.ListError(parent.hdlr, this.span);
                    return null;
                }
            }

            internal void scan() {
                int len = pat.Length;
                chr = (index == len ? NUL : pat[index++]);
                esc = (chr == '\\');
                if (esc) chr = (index == len ? NUL : pat[index++]);
            }

            /// <summary>
            /// Do lookahead one position in string buffer
            /// </summary>
            /// <returns>lookahead character or NUL if at end of string</returns>
            internal char peek() {
                return (index == pat.Length ? NUL : pat[index]);
            }

            /// <summary>
            /// Do lookahead two positions in string buffer
            /// </summary>
            /// <returns>lookahead character or NUL if at end of string</returns>
            internal char peek2()
            {
                return (index + 1 >= pat.Length ? NUL : pat[index+1]);
            }

            internal bool isEofString() {
                // The EOF string must be exactly "<<EOF>>"
                return (pat.Length >= 7 && pat[0] == '<' && pat.Substring(0, 7).Equals("<<EOF>>"));
            }

            internal int GetInt()
            {
                int val = (int)chr - (int)'0';
                scan();
                while (Char.IsDigit(chr))
                {
                    checked { val = val * 10 + (int)chr - (int)'0'; }
                    scan();
                }
                return val;
            }

            void Error(int num, int idx, int len, string str)
            { throw new RegExException(num, idx, len, str); }

            void Warn(int num, int idx, int len, string str)
            { parent.hdlr.ListError(span.FirstLineSubSpan(idx, len), num, str); }

            internal void checkAndScan(char ex)
            {
                if (chr == ex) 
                    scan(); 
                else 
                    Error(53, index-1, 1, "'" + ex + "'");
            }

            internal void check(char ex)
            {
                if (chr != ex) 
                    Error(53, index - 1, 1, "'" + ex + "'");
            }

            internal RegExTree RegEx()
            {
                if (isEofString())
                    return new Leaf(RegOp.eof);
                else
                    return Expr();
            }

            internal RegExTree Expr()
            {
                if (!esc && chr == '^')
                {
                    scan();
                    return new Unary(RegOp.leftAnchor, Simple());
                }
                else return Simple();
            }

            internal RegExTree Simple()
            {
                RegExTree tmp = Term();
                if (!esc && chr == '/')
                {
                    scan();
                    return new Binary(RegOp.context, tmp, Term());
                }
                else if (!esc && chr == '$')
                {
                    scan();
                    return new Unary(RegOp.rightAnchor, tmp);
                }
                return tmp;
            }

            internal RegExTree Term()
            {
                RegExTree tmp = Factor();
                while (!esc && chr == '|')
                {
                    scan();
                    tmp = new Binary(RegOp.alt, tmp, Factor());
                }
                return tmp;
            }

            internal RegExTree Factor()
            {
                RegExTree tmp = Primary();
                while (prStart[(int)chr] || esc)
                    tmp = new Binary(RegOp.concat, tmp, Primary());
                return tmp;
            }

            internal RegExTree LitString()
            {
                int pos = index;
                int len;
                string str;
                scan();                 // get past '"'
                while (esc || (chr != '"' && chr != NUL))
                    scan();
                len = index - 1 - pos;
                checkAndScan('"');
                str = pat.Substring(pos, len);
                try
                {
                    str = Utils.InterpretCharacterEscapes(str);
                }
                catch (RegExException x)
                {
                    // InterpretCharacterEscapes takes only a
                    // substring of "this.pat". RegExExceptions
                    // that are thrown will have an index value
                    // relative to this substring, so the index
                    // is transformed relative to "this.pat".
                    x.AdjustIndex(pos);
                    throw x;
                }
                return new Leaf(str);
            }

            internal RegExTree Primary()
            {
                RegExTree tmp;
                Unary     pls;
                if (!esc && chr == '"')
                    tmp = LitString();
                else if (!esc && chr == '(')
                {
                    scan(); tmp = Term(); checkAndScan(')');
                }
                else 
                    tmp = Primitive();

                if (!esc && chr == '*')
                {
                    scan();
                    tmp = new Unary(RegOp.closure, tmp);
                }
                else if (!esc && chr == '+')
                {
                    pls = new Unary(RegOp.closure, tmp);
                    pls.minRep = 1;
                    scan();
                    tmp = pls;
                }
                else if (!esc && chr == '?')
                {
                    pls = new Unary(RegOp.finiteRep, tmp);
                    pls.minRep = 0;
                    pls.maxRep = 1;
                    scan();
                    tmp = pls;
                }
                else if (!esc && chr == '{' && Char.IsDigit(peek()))
                {
                    pls = new Unary(RegOp.finiteRep, tmp);
                    GetRepetitions(pls);
                    tmp = pls;
                }
                return tmp;
            }

            internal void GetRepetitions(Unary tree) 
            {
                scan();          // read past '{'
                tree.minRep = GetInt();
                if (!esc && chr == ',')
                {
                    scan();
                    if (Char.IsDigit(chr))
                        tree.maxRep = GetInt();
                    else
                        tree.op = RegOp.closure;
                }
                else
                    tree.maxRep = tree.minRep;
                checkAndScan('}');
            }

            char EscapedChar()
            {
                index--;
                return Utils.EscapedChar(pat, ref index);
            }

            internal RegExTree Primitive()
            {
                RegExTree tmp;
                if (!esc && chr == '[')
                    tmp = CharClass();
                else if (!esc && chr == '{' && !Char.IsDigit(peek()))
                    tmp = UseLexCat();
                else if (!esc && chr == '.')
                {
                    Leaf leaf = new Leaf(RegOp.charClass);
                    leaf.rangeLit = new RangeLiteral(true);
                    scan();
                    leaf.rangeLit.list.Add(new CharRange('\n'));
                    tmp = leaf;
                }
                else
                {
                    if (esc) 
                        chr = EscapedChar();
                    tmp = new Leaf(chr);
                    scan();
                }
                return tmp;
            }


            internal RegExTree UseLexCat()
            {
                // Assert chr == '{'
                int start;
                string name;
                LexCategory cat;
                scan();                                     // read past '{'
                start = index - 1;
                while (chr != '}' && chr != NUL)
                    scan();
                name = pat.Substring(start, index - start - 1);
                checkAndScan('}');
                if (parent.lexCategories.TryGetValue(name, out cat))
                {
                    Leaf leaf = cat.regX as Leaf;
                    if (leaf != null && leaf.op == RegOp.charClass)
                        leaf.rangeLit.name = name;
                    return cat.regX;
                }
                else
                    Error(55, start, name.Length, name);
                return null;
            }

            internal RegExTree CharClass()
            {
                // Assert chr == '['
                // Need to build a new string taking into account char escapes
                Leaf leaf = new Leaf(RegOp.charClass);
                bool invert = false;
                scan();                           // read past '['
                if (!esc && chr == '^')
                {
                    invert = true;
                    scan();                       // read past '^'
                }
                leaf.rangeLit = new RangeLiteral(invert);
                // Special case of '-' at start, taken as ordinary class member.
                // This is correct for LEX specification, but is undocumented
                // behavior for FLEX. GPLEX gives a friendly warning, just in
                // case this is actually a typographical error.
                if (!esc && chr == '-')
                {
                    Warn(113, index - 1, 1, "'-'");
                    leaf.rangeLit.list.Add(new CharRange(chr));
                    scan();                       // read past -'
                }

                while (chr != NUL && (esc || chr != ']'))
                {
                    char lhCh;
                    int idx = index-1; // save starting index for error reporting
                    lhCh = (esc ? EscapedChar() : chr);
                    if (!esc && lhCh == '-')
                        Error(82, idx, index - idx, null);
                    //
                    // There are three possible elements here:
                    //  * a singleton character
                    //  * a character range
                    //  * a character category like [:IsLetter:]
                    //
                    if (chr == '[' && !esc && peek() == ':') // character category
                    {
                        Leaf rslt = CharCategory();
                        leaf.Merge(rslt);
                    }
                    else
                    {
                        scan();
                        if (!esc && chr == '-')             // character range
                        {
                            scan();
                            if (!esc && chr == ']')
                            {
                                // Special case of '-' at end, taken as ordinary class member.
                                // This is correct for LEX specification, but is undocumented
                                // behavior for FLEX. GPLEX gives a friendly warning, just in
                                // case this is actually a typographical error.
                                leaf.rangeLit.list.Add(new CharRange(lhCh));
                                leaf.rangeLit.list.Add(new CharRange('-'));
                                //Error(81, idx, index - idx - 1);
                                Warn(114, idx, index - idx - 1, String.Format("'{0}','{1}'", Utils.Map(lhCh), '-'));
                            }
                            else
                            {
                                char rhCh = (esc ? EscapedChar() : chr);
                                if ((int)rhCh < (int)lhCh)
                                    Error(54, idx, index - idx, null);
                                scan();
                                leaf.rangeLit.list.Add(new CharRange(lhCh, rhCh));
                            }
                        }
                        else                               // character singleton
                        {
                            leaf.rangeLit.list.Add(new CharRange(lhCh));
                        }
                    }
                }
                checkAndScan(']');
                return leaf;
            }

            private Leaf CharCategory()
            {
                // Assert: chr == '[', next is ':'
                int start;
                string name;
                Leaf rslt;
                scan(); // read past '['
                scan(); // read past ':'
                start = index - 1;
                while (Char.IsLetter(chr))
                    scan();
                name = pat.Substring(start, index - start - 1);
                if (!GetCharCategory(name, out rslt))
                    this.Error(76, start, name.Length, name);
                checkAndScan(':');
                checkAndScan(']');
                return rslt;
            }

            private bool GetCharCategory(string name, out Leaf rslt)
            {
                // lazy allocation of dictionary
                if (cats == null) 
                    InitCharCats();
                bool found = cats.TryGetValue(name, out rslt);
                // lazy population of element range lists
                if (found && rslt.rangeLit == null)
                    rslt.Populate(name, parent, parent.task.TargetSymCardinality);
                return found;
            }  
        }
	}

    /// <summary>
    /// Objects of this class carry exception information
    /// out to the call of Parse() on the regular expression.
    /// </summary>
    public class RegExException : Exception
    {
        int errNo;
        int index;
        int length;
        string text = null;

        public RegExException(int errorNum, int stringIx, int count, string message)
        { errNo = errorNum; index = stringIx; length = count; text = message; }

        public RegExException AdjustIndex(int delta)
        { this.index += delta; return this; }

        public void ListError(ErrorHandler handler, LexSpan span)
        {
            if (text == null)
                handler.ListError(span.FirstLineSubSpan(index, length), errNo);
            else
                handler.ListError(span.FirstLineSubSpan(index, length), errNo, text);
        }
    }

    public sealed class StartState
    {
        static int next = -1;

        int ord;
        bool isExcl;
        bool isInit = false;
        bool isAll = false;
        string name;
        public List<RuleDesc> rules = new List<RuleDesc>();

        public static StartState allState = new StartState(false, "$ALL$", false, true);    // ord = -1
        public static StartState initState = new StartState(false, "INITIAL", true, false); // ord = 0;

        public StartState(bool isX, string str)
        {
            isExcl = isX; name = str; ord = next++;
        }

        StartState(bool isX, string str, bool isInit, bool isAll)
        {
            isExcl = isX; name = str; this.isInit = isInit; this.isAll = isAll; ord = next++ ;
        }

        public string Name { get { return name; } }
        public int Ord { get { return ord; } }
        public bool IsExcl { get { return isExcl; } }
        public bool IsInit { get { return isInit; } }
        public bool IsAll { get { return isAll; } }

        public void AddRule(RuleDesc rule)
        {
            rules.Add(rule);
        }
    }

    public sealed class RuleDesc
    {
        static int next = 1;
        string pattern;
        internal LexSpan pSpan;
        public int ord = 0;
        RegExTree reAST;
        public LexSpan aSpan;
        public bool isBarAction;
        public bool isRightAnchored = false;
        public List<StartState> list;

        public string Pattern { get { return pattern; } }

        public RuleDesc(LexSpan loc, LexSpan act, List<StartState> aList, bool bar)
        {
            pSpan = loc;
            aSpan = act;
            pattern = pSpan.buff.GetString(pSpan.sPos, pSpan.ePos);
            isBarAction = bar;
            list = aList;
            ord = next++;
        }

        public RegExTree Tree { get { return reAST; } }
        public bool hasAction { get { return aSpan.IsInitialized; } }
        public void Dump() { Console.WriteLine(pattern); }

        public void ParseRE(AAST aast)
        {
            reAST = new AAST.ReParser(pattern, pSpan, aast).Parse();
            SemanticCheck(aast);
        }

        /// <summary>
        /// This is the place to perform any semantic checks on the 
        /// trees corresponding to a rule of the LEX grammar,
        /// during a recursive traversal of the tree.  It is hard
        /// to do these on the fly during AST construction, because
        /// of the tree-grafting that happens for lexical categories.
        /// 
        /// First check is that '^' and '$' can only appear 
        /// (logically) at the ends of the pattern.
        /// Later need to check ban on multiple right contexts ...
        /// </summary>
        /// <param name="aast"></param>
        void SemanticCheck(AAST aast)
        {
            RegExTree tree = reAST;
            if (tree != null && tree.op == RegOp.leftAnchor) tree = ((Unary)tree).kid;
            if (tree != null && tree.op == RegOp.rightAnchor) tree = ((Unary)tree).kid;
            Check(aast, tree);
        }

        void Check(AAST aast, RegExTree tree)
        {
            Binary bnryTree;
            Unary unryTree;
            Leaf leafTree;
            if (tree == null) return;
            switch (tree.op)
            {
                case RegOp.charClass:
                case RegOp.primitive:
                case RegOp.litStr:
                case RegOp.eof:
                    leafTree = (Leaf)tree;
                    break;
                case RegOp.context:
                case RegOp.concat:
                case RegOp.alt:
                    bnryTree = (Binary)tree;
                    Check(aast, bnryTree.lKid);
                    Check(aast, bnryTree.rKid);
                    if (tree.op == RegOp.context && 
                        bnryTree.lKid.contextLength() == 0 &&
                        bnryTree.rKid.contextLength() == 0) aast.hdlr.ListError(pSpan, 75);
                    break;
                case RegOp.closure:
                case RegOp.finiteRep:
                    unryTree = (Unary)tree;
                    Check(aast, unryTree.kid);
                    break;
                case RegOp.leftAnchor:
                case RegOp.rightAnchor:
                    aast.hdlr.ListError(pSpan, 69);
                    break;
            }
        }
    }

    public sealed class LexCategory
    {
        string name;
        string verb;
        LexSpan vrbSpan;
        internal RegExTree regX;

        public LexCategory(string nam, string vrb, LexSpan spn)
        {
            vrbSpan = spn;
            verb = vrb;
            name = nam;
        }

        public void ParseRE(AAST aast)
        { regX = new AAST.ReParser(verb, vrbSpan, aast).Parse(); }
    }

    public sealed class RuleBuffer
    {
        List<LexSpan> locs = new List<LexSpan>();
        int fRuleLine, lRuleLine;  // First line of rules, last line of rules.

        public int FLine { get { return fRuleLine; } set { fRuleLine = value; } }
        public int LLine { get { return lRuleLine; } set { lRuleLine = value; } }

        public void AddSpan(LexSpan l) { locs.Add(l); }

        /// <summary>
        /// This method detects the presence of code *between* rules. Such code has
        /// no unambiguous meaning, and is skipped, with a warning message.
        /// </summary>
        /// <param name="aast"></param>
        public void FinalizeCode(AAST aast)
        {
            for (int i = 0; i < locs.Count; i++)
            {
                LexSpan loc = locs[i];

                if (loc.sLin < fRuleLine) aast.AddCodeSpan(AAST.Destination.scanProlog, loc);
                else if (loc.sLin > lRuleLine) aast.AddCodeSpan(AAST.Destination.scanEpilog, loc);
                else // code is between rules
                    aast.hdlr.ListError(loc, 110);
            }
        }
    }

    #region AST for Regular Expressions

    public enum RegOp
    {
        eof,
        context,
        litStr,
        primitive,
        concat,
        alt,
        closure,
        finiteRep,
        charClass,
        leftAnchor,
        rightAnchor
    }

    public abstract class RegExDFS
    {
        public abstract void Op(RegExTree tree);
    }
     
    /// <summary>
    /// Abstract class for AST representing regular expressions.
    /// Concrete subclasses correspond to --- 
    /// binary trees (context, alternation and concatenation)
    /// unary trees (closure, finite repetition and anchored patterns)
    /// leaf nodes (chars, char classes, literal strings and the eof marker)
    /// </summary>
    public abstract class RegExTree
    {
        public RegOp op;
        public RegExTree(RegOp op) { this.op = op; }
        /// <summary>
        /// This is a helper to compute the length of strings
        /// recognized by a regular expression.  This is important
        /// because the right context operator "R1/R2" is efficiently 
        /// implemented if either R1 or R2 produce fixed length strings.
        /// </summary>
        /// <returns>0 if length is variable, otherwise length</returns>
        internal abstract int contextLength();

        /// <summary>
        /// This is the navigation method for running the visitor
        /// over the tree in a depth-first-search visit order.
        /// </summary>
        /// <param name="visitor">visitor.Op(this) is called on each node</param>
        public abstract void Visit(RegExDFS visitor);
    }

    public sealed class Leaf : RegExTree
    {
        public char chVal;     // in case of primitive char
        public string str;
        internal RangeLiteral rangeLit = null;

        public Leaf(string s) : base(RegOp.litStr) { str = s; }
        public Leaf(char chr) : base(RegOp.primitive) { chVal = chr; }
        public Leaf(RegOp op) : base(op) {}

        internal override int contextLength()
        {
            return (op == RegOp.litStr ? str.Length : 1);
        }

        public override void Visit(RegExDFS visitor) { visitor.Op(this); }

        internal void Populate(string name, AAST aast, int max)
        {
            int i;
            BitArray bits = new BitArray(max);
            this.rangeLit = new RangeLiteral(false);
            switch (name)
            {
                case "IsControl":
                    for (i = 0; i < max; i++)
                        if (Char.IsControl((char)i))
                            bits[i] = true;
                    break;
                case "IsDigit":
                    for (i = 0; i < max; i++)
                        if (Char.IsDigit((char)i))
                            bits[i] = true;
                    break;
                case "IsLetter":
                    for (i = 0; i < max; i++)
                        if (Char.IsLetter((char)i))
                            bits[i] = true;
                    break;
                case "IsLetterOrDigit":
                    for (i = 0; i < max; i++)
                        if (Char.IsLetterOrDigit((char)i))
                            bits[i] = true;
                    break;
                case "IsLower":
                    for (i = 0; i < max; i++)
                        if (Char.IsLower((char)i))
                            bits[i] = true;
                    break;
                case "IsNumber":
                    for (i = 0; i < max; i++)
                        if (Char.IsNumber((char)i))
                            bits[i] = true;
                    break;
                case "IsPunctuation":
                    for (i = 0; i < max; i++)
                        if (Char.IsPunctuation((char)i))
                            bits[i] = true;
                    break;
                case "IsSeparator":
                    for (i = 0; i < max; i++)
                        if (Char.IsSeparator((char)i))
                            bits[i] = true;
                    break;
                case "IsSymbol":
                    for (i = 0; i < max; i++)
                        if (Char.IsSymbol((char)i))
                            bits[i] = true;
                    break;
                case "IsUpper":
                    for (i = 0; i < max; i++)
                        if (Char.IsUpper((char)i))
                            bits[i] = true;
                    break;
            }
            int j = 0;
            while (j < max)
            {
                int start;
                while (j < max && !bits[j])
                    j++;
                if (j == max)
                    return;
                start = j;
                while (j < max && bits[j])
                    j++;
                this.rangeLit.list.Add(new CharRange((char)start, (char)(j - 1)));
            }
        }

        public void Merge(Leaf addend)
        {
            foreach (CharRange rng in addend.rangeLit.list.Ranges)
                this.rangeLit.list.Add(rng);
        }
    }

    public sealed class Unary : RegExTree
    {
        public RegExTree kid;
        public int minRep = 0;         // min repetitions for closure/finiteRep
        public int maxRep = 0;         // max repetitions for finiteRep.
        public Unary(RegOp op) : base(op) {}
        public Unary(RegOp op, RegExTree l) : base(op) { kid = l;  }
        internal override int contextLength()
        {
            switch (op)
            {
                case RegOp.closure: return 0;
                case RegOp.finiteRep: return (minRep == maxRep ? kid.contextLength() * minRep : 0);
                case RegOp.leftAnchor: return kid.contextLength();
                case RegOp.rightAnchor: throw new Exception("context cannot be anchored");
                default: throw new Exception("unknown unary RegOp");
            }
        }

        public override void Visit(RegExDFS visitor)
        {
            visitor.Op(this);
            kid.Visit(visitor);
        }
    }

    public sealed class Binary : RegExTree
    {
        public RegExTree lKid, rKid;
        public Binary(RegOp op) : base(op) { }
        public Binary(RegOp op, RegExTree l, RegExTree r) : base(op) { lKid = l; rKid = r; }
        internal override int contextLength()
        {
            if (op == RegOp.context) throw new Exception("multiple context operators");
            else
            {
                int lLen = lKid.contextLength();
                int rLen = rKid.contextLength();
                if (lLen <= 0 || rLen <= 0) return 0;
                else if (op == RegOp.concat) return lLen + rLen;
                else if (lLen == rLen) return lLen;
                else return 0;
            }
        }

        public override void Visit(RegExDFS visitor)
        {
            visitor.Op(this);
            lKid.Visit(visitor);
            rKid.Visit(visitor);
        }
    }
    #endregion
}
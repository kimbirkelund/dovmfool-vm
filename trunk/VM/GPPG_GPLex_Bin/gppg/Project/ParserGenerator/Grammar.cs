// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2007
// (see accompanying GPPGcopyright.rtf)



using System;
using System.IO;
using System.Collections.Generic;


namespace gpcc
{
	public class Grammar
	{
        public const string DefaultValueTypeName = "ValueType";

		public List<Production> productions = new List<Production>();
        public string unionType;
		public int NumActions = 0;
		public string prologCode;	// before first %%
        public string epilogCode;	// after last %%
		public NonTerminal startSymbol;
		public Production rootProduction;
		public Dictionary<string, NonTerminal> nonTerminals = new Dictionary<string, NonTerminal>();
		public Dictionary<string, Terminal> terminals = new Dictionary<string, Terminal>();
        public List<string> usingList = new List<string>();
        public List<Conflict> conflicts = new List<Conflict>();

        public bool IsPartial = false;
        public string OutFName = null;
        public string TokFName = null;
        public string DiagFName = null;
        public string InputFName = null;
        public string Namespace;
        public string Visibility = "public";
        public string ParserName = "Parser";
        public string TokenName = "Tokens";
        public string ValueTypeName = null;
        public string LocationTypeName = "LexLocation";
        public string PartialMark { get { return (IsPartial ? " partial" : ""); } }


        public Grammar()
        {
			LookupTerminal(GrammarToken.Symbol, "error");
			LookupTerminal(GrammarToken.Symbol, "EOF");
        }


		public Terminal LookupTerminal(GrammarToken token, string name)
		{
			if (!terminals.ContainsKey(name))
				terminals[name] = new Terminal(token == GrammarToken.Symbol, name);

			return terminals[name];
		}


		public NonTerminal LookupNonTerminal(string name)
		{
			if (!nonTerminals.ContainsKey(name))
				nonTerminals[name] = new NonTerminal(name);

			return nonTerminals[name];
		}


		public void AddProduction(Production production)
		{
			productions.Add(production);
			production.num = productions.Count;
		}


		public void CreateSpecialProduction(NonTerminal root)
		{
			rootProduction = new Production(LookupNonTerminal("$accept"));
			AddProduction(rootProduction);
			rootProduction.rhs.Add(root);
            rootProduction.rhs.Add(LookupTerminal(GrammarToken.Symbol, "EOF"));
		}

        void MarkReachable()
        {
            Stack<NonTerminal> work = new Stack<NonTerminal>();
            rootProduction.lhs.reached = true; // by definition.
            work.Push(startSymbol);
            startSymbol.reached = true;
            while (work.Count > 0)
            {
                NonTerminal nonT = work.Pop();
                foreach (Production prod in nonT.productions)
                {
                    foreach (Symbol smbl in prod.rhs)
                    {
                        NonTerminal rhNt = smbl as NonTerminal;
                        if (rhNt != null && !rhNt.reached)
                        {
                            rhNt.reached = true;
                            work.Push(rhNt);
                        }
                    }
                }
            }
        }

        public bool CheckGrammar()
        {
            bool ok = true;
            NonTerminal nt;
            MarkReachable();
            foreach (KeyValuePair<string, NonTerminal> pair in nonTerminals)
            {
                nt = pair.Value;
                if (!nt.reached)
                    Console.Error.WriteLine(
                        "WARNING: NonTerminal symbol \"{0}\" is unreachable", pair.Key);

                if (nt.productions.Count == 0)
                {
                    ok = false;
                    Console.Error.WriteLine(
                        "FATAL: NonTerminal symbol \"{0}\" has no productions", pair.Key);
                }
            }
            return ok;    
        }

        public void ReportConflicts(StreamWriter wrtr)
        {
            if (wrtr == null)
                return;
            foreach (Conflict theConflict in conflicts)
                theConflict.Report(wrtr);
        }
    }

    #region Conflict Diagnostics

    public abstract class Conflict
    {
        protected Terminal symbol;
        protected string str1 = null;
        protected string str2 = null;
        public Conflict(Terminal sy, string s1, string s2) { symbol = sy; str1 = s1; str2 = s2; }

        public abstract void Report(StreamWriter w);
    }

    public class ReduceReduceConflict : Conflict
    {
        int chosen;

        public ReduceReduceConflict(Terminal sy, string s1, string s2, int prod) 
            : base(sy, s1, s2)
        { 
            chosen = prod; 
        }

        public override void Report(StreamWriter wrtr)
        {
            wrtr.WriteLine(
                "Reduce/Reduce conflict on symbol \"{0}\", parser will reduce production {1}", 
                symbol.ToString(),
                chosen);
            wrtr.WriteLine(str1);
            wrtr.WriteLine(str2);
            wrtr.WriteLine();
        }
    }

    public class ShiftReduceConflict : Conflict
    {
        State fromState;
        State toState;
        public ShiftReduceConflict(Terminal sy, string s1, string s2, State from, State to)
            : base(sy, s1, s2)
        { 
            fromState = from; toState = to; 
        }

        public override void Report(StreamWriter wrtr)
        {
            wrtr.WriteLine("Shift/Reduce conflict on symbol \"{0}\", parser will shift", symbol.ToString());
            wrtr.WriteLine(str2);
            wrtr.WriteLine(str1);
            wrtr.Write("  Items for From-state ");
            wrtr.WriteLine(fromState.ItemDisplay());
            wrtr.Write("  Items for Next-state ");
            wrtr.WriteLine(toState.ItemDisplay());
            wrtr.WriteLine();
        }
    }

    #endregion
}








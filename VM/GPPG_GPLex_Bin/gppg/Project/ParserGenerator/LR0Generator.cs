// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2007
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Collections.Generic;


namespace gpcc
{
	public class LR0Generator
	{
		protected List<State> states = new List<State>();
		protected Grammar grammar;
		private Dictionary<Symbol, List<State>> accessedBy = new Dictionary<Symbol,List<State>>();


		public LR0Generator(Grammar grammar)
		{
			this.grammar = grammar;
		}


        public List<State> BuildStates()
		{
			// create state for root production and expand recursively
			ExpandState(grammar.rootProduction.lhs, new State(grammar.rootProduction));
            
            return states;
		}


		private void ExpandState(Symbol sym, State newState)
		{
			newState.accessedBy = sym;
			states.Add(newState);

			if (!accessedBy.ContainsKey(sym))
				accessedBy[sym] = new List<State>();
			accessedBy[sym].Add(newState);

			newState.AddClosure();
			ComputeGoto(newState);
		}


		private void ComputeGoto(State state)
		{
			foreach (ProductionItem item in state.all_items)
				if (!item.expanded && !item.isReduction())
				{
					item.expanded = true;
					Symbol s1 = item.production.rhs[item.pos];

					// Create itemset for new state ...
					List<ProductionItem> itemSet = new List<ProductionItem>();
					itemSet.Add(new ProductionItem(item.production, item.pos+1));

					foreach (ProductionItem item2 in state.all_items)
						if (!item2.expanded && !item2.isReduction())
						{
							Symbol s2 = item2.production.rhs[item2.pos];

							if (s1 == s2)
							{
								item2.expanded = true;
								itemSet.Add(new ProductionItem(item2.production, item2.pos+1));
							}
						}

					State existingState = FindExistingState(s1, itemSet);

					if (existingState == null)
					{
						State newState = new State(itemSet);
						state.AddGoto(s1, newState);
						ExpandState(s1, newState);
					}
					else
						state.AddGoto(s1, existingState);
				}
		}


		private State FindExistingState(Symbol sym, List<ProductionItem> itemSet)
		{
			if (accessedBy.ContainsKey(sym))
				foreach (State state in accessedBy[sym])
					if (ProductionItem.SameProductions(state.kernal_items, itemSet))
						return state;

			return null;
		}




		public void BuildParseTable()
		{
			foreach (State state in states)
			{
				// Add shift actions ...
				foreach (Terminal t in state.terminalTransitions)
					state.parseTable[t] = new Shift(state.Goto[t]);

				// Add reduce actions ...
				foreach (ProductionItem item in state.all_items)
					if (item.isReduction())
					{
						// Accept on everything
						if (item.production == grammar.rootProduction)
							foreach (Terminal t in grammar.terminals.Values)
								state.parseTable[t] = new Reduce(item);

						foreach (Terminal t in item.LA)
						{
							// possible conflict with existing action
							if (state.parseTable.ContainsKey(t))
							{
								ParserAction other = state.parseTable[t];
                                Production iProd = item.production;
								if (other is Reduce)
								{
                                    Production oProd = ((Reduce)other).item.production;

									// Choose in favour of production listed first in the grammar
                                    if (oProd.num > iProd.num)
                                        state.parseTable[t] = new Reduce(item);

                                    if (GPCG.VERBOSE)
                                    {
                                        string p1 = String.Format(" Reduce {0}:\t{1}", oProd.num, oProd.ToString());
                                        string p2 = String.Format(" Reduce {0}:\t{1}", iProd.num, iProd.ToString());
                                        int chsn = (oProd.num > iProd.num ? iProd.num : oProd.num);
                                        Console.Error.WriteLine("Reduce/Reduce conflict");
                                        Console.Error.WriteLine(p1);
                                        Console.Error.WriteLine(p2);
                                        grammar.conflicts.Add(new ReduceReduceConflict(t, p1, p2, chsn));
                                    }
                                    else
                                        Console.Error.WriteLine("Reduce/Reduce conflict, state {0}: {1} vs {2} on {3}",
                                            state.num, iProd.num, oProd.num, t);
								}
								else
								{
                                    if (iProd.prec != null && t.prec != null)
                                    {
                                        if (iProd.prec.prec > t.prec.prec ||
                                            (iProd.prec.prec == t.prec.prec &&
                                             iProd.prec.type == PrecType.left))
                                        {
                                            // resolve in favour of reduce (without error)
                                            state.parseTable[t] = new Reduce(item);
                                        }
                                        else
                                        {
                                            // resolve in favour of shift (without error)
                                        }
                                    }
                                    else
                                    {
                                        if (GPCG.VERBOSE)
                                        {
                                            State next = ((Shift)other).next;
                                            string p1 = String.Format(" Shift \"{0}\":\tState-{1} -> State-{2}", t, state.num, next.num);
                                            string p2 = String.Format(" Reduce {0}:\t{1}", iProd.num, iProd.ToString());
                                            Console.Error.WriteLine("Shift/Reduce conflict");
                                            Console.Error.WriteLine(p1);
                                            Console.Error.WriteLine(p2);
                                            grammar.conflicts.Add(new ShiftReduceConflict(t, p1, p2, state, next));
                                        }
                                        else
                                            Console.Error.WriteLine("Shift/Reduce conflict, state {0} on {1}", state.num, t);
                                    }
									// choose in favour of the shift
								}
							}
							else
								state.parseTable[t] = new Reduce(item);
						}
					}
			}
		}


		public void Report()
		{
			Console.WriteLine("Grammar");

			NonTerminal lhs = null;
			foreach (Production production in grammar.productions)
			{
				if (production.lhs != lhs)
				{
					lhs = production.lhs;
					Console.WriteLine();
					Console.Write("{0,5} {1}: ", production.num, lhs);
				}
				else
					Console.Write("{0,5} {1}| ", production.num, new string(' ', lhs.ToString().Length));

				for (int i=0; i<production.rhs.Count-1; i++)
					Console.Write("{0} ", production.rhs[i].ToString());

				if (production.rhs.Count > 0)
					Console.WriteLine("{0}", production.rhs[production.rhs.Count-1]);
				else
					Console.WriteLine("/* empty */");
			}

			Console.WriteLine();

			foreach (State state in states)
				Console.WriteLine(state.ToString());
		}
	}
}
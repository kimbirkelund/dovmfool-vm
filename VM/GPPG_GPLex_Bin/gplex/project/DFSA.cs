// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GPLEX.Parser;

namespace GPLEX.Automaton
{
    /// <summary>
    /// Represents a SET of DFSA. There is a separate DFSA 
    /// instance generated for each separate StartCondition.
    /// That is: there is only one DFSA object with one or
    /// more DfsaInstance objects in the "dfas" field.
    /// </summary>
    internal class DFSA
    {
        public const int unset = -1;
        public const int gotoStart = -1;
        public const int eofNum = 0;

        /// <summary>
        /// The distinguished EOF state.
        /// </summary>
        public DState eofState;

        /// <summary>
        /// The actual count of entries in the transition table.
        /// This takes into account compression (use of default values)
        /// and row-sharing if there are identical rows.
        /// </summary>
        int tranNum = 0;

        /// <summary>
        /// Counter for allocation of final ordinal
        /// numbering to DState objects. 
        /// </summary>
        int globNext = eofNum + 1;           // State zero is "eofState"

        int backupCount = 0;                 // Number of states needing backup
        int maxAccept = 0;
        int copyNum = 0;                     // Number of next state entry aliases
        bool hasLeftAnchors = false;
        bool foundShortestStrings = false;

        int maxSym;                          // Backing field for MaxSym property

        /// <summary>
        /// A reference to the controlling task-state
        /// </summary>
        internal TaskState myTask;

        /// <summary>
        /// Array of DFSA instances
        /// </summary>
        DfsaInstance[] dfas;

        /// <summary>
        /// A list of all created DStates
        /// </summary>
        List<DState> stateList = new List<DState>();
        internal int origLength = 0;

        /// <summary>
        /// "next" is the global next state table
        /// </summary>
        internal Dictionary<uint, DState> next = new Dictionary<uint, DState>();

        /// <summary>
        /// "newNext" is the new dictionary in which the replacement next
        /// state table is built, in the event the the FSA is minimized.
        /// </summary>
        internal Dictionary<uint, DState> newNext = null; 
        void InitNewNext() { newNext = new Dictionary<uint, DState>(); }
        void OverwriteOldNext() { next = newNext; }

        public DFSA(TaskState task)
        {
            myTask = task;
            maxSym = (task.ChrClasses ? task.partition.Length : task.TargetSymCardinality);
            eofState = new DState(this);
            eofState.Num = eofNum;
            stateList.Add(eofState);
        }

        /// <summary>
        /// Cardinality of symbol alphabet in next-state tables.
        /// This could be the real alphabet, or the equivalence class cardinality.
        /// </summary>
        internal int MaxSym { get { return maxSym; } }


        /// <summary>
        /// This method computes the shortest string reaching each state of the automaton
        /// </summary>
        void FindShortestStrings()
        {
            if (foundShortestStrings) return;
            else
            {
                // long start = TaskState.GetTicks();
                Stack<DState> worklist = new Stack<DState>();
                DfsaInstance inst;
                DState elem;
                DState next;
                // Push every start state and anchor state on the worklist
                for (int i = 0; i < dfas.Length; i++)
                {
                    inst = dfas[i];
                    if (inst != null)
                    {
                        inst.start.shortestStr = "";
                        worklist.Push(inst.start);
                        inst.start.listed = true;
                        if (inst.anchor != null)
                        {
                            inst.anchor.shortestStr = "^";
                            worklist.Push(inst.anchor);
                            inst.anchor.listed = true;
                        }
                    }
                }
                // Process every state reachable from the popped state
                while (worklist.Count > 0)
                {
                    elem = worklist.Pop();
                    elem.listed = false;
                    for (int i = 1; i < MaxSym; i++)
                    {
                        int ch = i;
                        next = elem.GetNext(ch);
                        if (next != null && (next.shortestStr == null ||
                                             next.shortestStr.Length > elem.shortestStr.Length + 1))
                        {
                            next.shortestStr = elem.shortestStr +
                                (myTask.ChrClasses ? (char)myTask.partition.InvMap(ch) : (char)ch);
                                // need unmapped characters!
                            if (!next.listed) { worklist.Push(next); next.listed = true; }
                        }
                    }
                }
                foundShortestStrings = true;
            }
        }

        internal string MapSymToStr(int chr)
        {
            if (this.myTask.ChrClasses)
                chr = this.myTask.partition.InvMap(chr);
            return Utils.Map(chr);
        }

        /// <summary>
        /// This class is a factory for the objects that
        /// represent sets of NFSA states.  The sets are arrays 
        /// of bit sets mapped onto a uint32 array.  The length
        /// of the arrays is frozen at the time that the factory
        /// is instantiated, as |NFSA| div 32
        /// </summary>
        internal class NSetFactory
        {
            private int length;
            public NSetFactory(int nfsaCardinality) { length = (nfsaCardinality + 31) / 32; }
            public NSet MkNewSet() { return new NSet(length); }

            /// <summary>
            /// The sets themselves.  The class needs to implement
            /// IEquatable and override GetHashCode if it is to be
            /// used in a dictionary with Hashtable lookup 
            /// </summary>
            public class NSet : IEquatable<NSet>
            {
                private uint[] arr;
                internal NSet(int length) { arr = new uint[length]; }

                public bool Equals(NSet val)
                {
                    // Short-circuit the test if possible, as for string comparisons
                    for (int i = 0; i < arr.Length; i++) if (arr[i] != val.arr[i]) return false;
                    return true;
                }

                public override int GetHashCode()
                {
                    // The hash code is a word-wise XOR
                    uint val = arr[0];
                    for (int i = 1; i < arr.Length; i++) val ^= arr[i];
                    return (int)val;
                }

                public void Insert(int ord) { arr[ord / 32] |= (uint)(1 << ord % 32); }
                public bool Contains(int ord) { return (arr[ord / 32] & (uint)(1 << (ord % 32))) != 0; }
                public NEnum GetEnumerator() { return new NEnum(this.arr); }

                public string Diag()
                {
                    string rslt = "";
                    NEnum iter = this.GetEnumerator();
                    while (iter.MoveNext())
                    {
                        int i = iter.Current;
                        rslt += i.ToString();
                        rslt += ",";
                    }
                    return rslt;
                }
            }

            /// <summary>
            /// This is a custom enumerator.  It does not implement 
            /// IEnumerator, and so cannot be used in foreach statements.
            /// </summary>
            public class NEnum
            {
                uint[] arr;
                uint elem = 0;      // having elem zero is essential both initially and after Reset.
                int current, idx, ord;
                internal NEnum(uint[] dat) { arr = dat; current = -1; idx = -1; }
                public void Reset() { current = -1; idx = -1; elem = 0; }

                public int Current
                { get { if (current < 0) throw new InvalidOperationException(); else return current; } }

                public bool MoveNext()
                {
                    // The essence of the code is to skip quickly over runs
                    // of zeros in sparse sets. The code quickly skips over
                    // whole uint32 words that are empty, and skips over 
                    // 8-bit runs of zeros also.
                    // Post-condition: elem and ord denote the next position
                    //                 that might hold a set element.
                    //                 idx corresponds to the current elem.
                    while (true)
                    {
                        if (elem != 0)  // try to find non-zero bit - there is at least one!
                        {
                            while ((elem & 255) == 0) { elem /= 256; ord += 8; } // discard 8 at a time
                            while ((elem & 1) == 0) { elem /= 2; ord++; }        // now one at a time
                            current = idx * 32 + ord;                            // compute ordinal position
                            elem /= 2; ord++;                                    // establish post condition
                            return true;
                        }
                        else
                        {
                            idx++;                                               // get the next array index
                            if (idx >= arr.Length) return false;                 // check for array ended
                            elem = arr[idx];                                     // else get the next element
                            ord = 0;                                             // establish post condition
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This nested class represents all of the transitions starting
        /// from a particular StartCondition. There is usually one start
        /// state only.  However, if this particular start condition has
        /// one or more left-anchored pattern, then there will be an
        /// anchor start state as well.
        /// </summary>
        public class DfsaInstance
        {
            internal int instNext = 0;                 // number of next state to be allocated
            internal int acceptCount = 0;
            internal DState start = null;
            internal DState anchor = null;
            internal DFSA parent;                      // Parent DFSA reference
            internal LexSpan eofCode;                  // Text span for EOF semantic action.
            internal NFSA.NfsaInstance myNfaInst;      // Corresponding NFSA instance
            NSetFactory factory = null;                // Factory for creating NSet objects

            Dictionary<NSetFactory.NSet, DState> dfaTable = new Dictionary<NSetFactory.NSet, DState>();

            internal Dictionary<uint, DState> instNextState = new Dictionary<uint, DState>();

            public DfsaInstance(NFSA.NfsaInstance nfa, DFSA dfa)
            {
                myNfaInst = nfa;
                parent = dfa;
                eofCode = nfa.eofAction;
                factory = new NSetFactory(myNfaInst.MaxEps);
            }

            public int StartConditionOrd { get { return myNfaInst.myStartCondition.Ord; } }
            public string StartConditionName { get { return myNfaInst.myStartCondition.Name; } }

            /// <summary>
            /// Convert the NFSA for this particular StartCondition.
            /// This uses the classical subset construction algorithm.
            /// </summary>
            internal void Convert()
            {
                Stack<DState> stack = new Stack<DState>();
                NSetFactory.NSet sSet = factory.MkNewSet();
                int symCardinality = parent.MaxSym;
                sSet.Insert(myNfaInst.Entry.ord);
                MkClosure(sSet);
                start = MkNewState(sSet);
                stack.Push(start);
                if (myNfaInst.LeftAnchored)
                {
                    //  The NfsaInst flag shows that the corresponding start
                    //  condition has at least one rule that is left-anchored.
                    //  This means that this DfsaInst must have two start
                    //  states. One is the usual "this.start" plus another
                    //  "this.anchor" for patterns starting in column 0.
                    //  The whole automaton must use a slower next-state loop.
                    parent.hasLeftAnchors = true;
                    sSet = factory.MkNewSet();
                    sSet.Insert(myNfaInst.AnchorState.ord);
                    MkClosure(sSet);
                    anchor = MkNewState(sSet);
                    stack.Push(anchor);
                }
                // Next is the worklist algorithm.  Newly created dfsa states
                // are placed on the stack.  When popped the next states
                // are computed from the nfsa transition information.
                while (stack.Count > 0)
                {
                    DState last = stack.Pop();
                    NSetFactory.NSet pSet = last.nfaSet;
                    //
                    // For this state we are going to consider every
                    // possible transition, each input symbol at a time.
                    //
                    for (int ch = 0; ch < symCardinality; ch++)                 // For every character do
                    {
                        // For each transition out of "last" we
                        // will form a set of NFSA states and find
                        // or create a corresponding DFSA state.
                        NSetFactory.NSet nxSet = null;
                        DState nxState = null;
                        NSetFactory.NEnum inum = pSet.GetEnumerator();
                        while (inum.MoveNext())    // foreach NFSA state contained in "last" 
                        {
                            int i = inum.Current;
                            NFSA.NState nStI = myNfaInst.nStates[i];   // get the nfsa state
                            NFSA.NState nStCh = nStI.GetNext(ch);      // get the nfsa next state
                            if (nStCh != null)                         // ==> we have a transition
                            {
                                // Create next state set on demand, and insert ord in set.
                                if (nxSet == null) 
                                    nxSet = factory.MkNewSet();
                                nxSet.Insert(nStCh.ord);
                            }
                        }
                        // If nxSet is not null, then there must have been
                        // at least one transition on the current symbol ch.
                        if (nxSet != null)
                        {
                            // NSetFactory.NSet save = nxSet;

                            // Enhance the state set with all the
                            // states in the epsilon-closure. We then
                            // look up the set in the dictionary.  If
                            // the set is in the dictionary retrieve 
                            // the corresponding DState, else create
                            // a new DState and add it to the worklist.
                            MkClosure(nxSet);
                            if (dfaTable.ContainsKey(nxSet))
                                nxState = dfaTable[nxSet];
                            if (nxState == null)
                            {
                                // Console.WriteLine("nxSet, <{0}, {1}>", save.Diag(), nxSet.Diag());
                                nxState = MkNewState(nxSet);
                                stack.Push(nxState);
                            }
                            last.AddTrans(ch, nxState);
                        }
                    }
                }
            }

            /// <summary>
            /// Create a new DState corresponding to the 
            /// given set of NFSA ordinal numbers.
            /// </summary>
            /// <param name="stateSet">The set of NFSA states</param>
            /// <returns>The new state</returns>
            internal DState MkNewState(NSetFactory.NSet stateSet)
            {
                DState dSt = new DState(this);
                dSt.nfaSet = stateSet;
                dSt.Ord = instNext++;
                parent.stateList.Add(dSt);
                dfaTable.Add(stateSet, dSt);

                // Console.WriteLine("New DState, <{0}>", stateSet.Diag());

                foreach (int i in myNfaInst.acceptStates)
                    if (stateSet.Contains(i))
                    {
                        NFSA.NState nfas = myNfaInst.nStates[i];
                        RuleDesc rule = nfas.accept;
                        if (dSt.Num == unset)
                        {
                            // DFSA accept states are compact in the numbering
                            dSt.Num = parent.globNext++;
                            acceptCount++;
                        }
                        if (dSt.accept == null || rule.ord < dSt.accept.ord)
                        {
                            if (parent.myTask.Trace && dSt.accept != null)
                            {
                                Console.WriteLine("Two accept patterns, \"{0}\" Vs. \"{1}\"", rule.Pattern, dSt.accept.Pattern);
                                Console.WriteLine("\tPreferring line {0} to line {1} state {2}",
                                    rule.pSpan.sLin, dSt.accept.pSpan.sLin, dSt.Num);
                            }
                            dSt.accept = rule;
                            dSt.rhCntx = (nfas.rhCntx > 0 ? nfas.rhCntx : 0);
                            dSt.lhCntx = (nfas.lhCntx > 0 ? nfas.lhCntx : 0);
                        }
                    }
                return dSt;
            }


            /// <summary>
            /// The epsilon closure algorithm
            /// The set passed in as argument is updated in place.
            /// </summary>
            /// <param name="states">initial set of NFSA states</param>
            /// <returns>updated set of NFSA states</returns>
            internal void MkClosure(NSetFactory.NSet set)
            {
                Stack<int> stack = new Stack<int>();
                NSetFactory.NEnum inum = set.GetEnumerator();
                while (inum.MoveNext()) stack.Push(inum.Current);
                while (stack.Count > 0)
                {
                    int pos = stack.Pop();
                    foreach (NFSA.NState nxt in myNfaInst.nStates[pos].epsList)
                    {
                        if (!set.Contains(nxt.ord)) { set.Insert(nxt.ord); stack.Push(nxt.ord); }
                    }
                }
            }
        }

        /// <summary>
        /// Class representing a DFSA state. These need to be
        /// sorted according to global ordinal number AFTER
        /// the global numbers have been allocated.
        /// </summary>
        public class DState : IComparable<DState>
        {
            static int nextSN = 1;                // Counter for serial number allocation

            int instOrd;                          // ordinal of this state within this instance
            int globOrd = unset;                  // ordinal of this state within whole DFSA-set
            internal int rhCntx = 0;                        // the right context fixed length
            internal int lhCntx = 0;                        // the left context fixed length
            internal DfsaInstance myDfaInst;                // instance to which this DState belongs
            internal DFSA myDfsa;                           // a reference to the parent DFSA
            internal NSetFactory.NSet nfaSet;               // set of nfsa state that this state represents
            internal List<int> trList = new List<int>();    // list of transitions on this state
            internal RuleDesc accept = null;                // if this is an accept state, the rule recognized
            internal string shortestStr = null;
            internal bool listed = false;

            internal int listOrd = 0;                       // only used by the minimizer algorithm
            internal object block = null;                   // only used by the minimizer algorithm
            internal LinkedListNode<DState> listNode = null;// only used by the minimizer algorithm
            private List<DState>[] predecessors = null;     // inverse nextState, only used by minimizer

            readonly int serialNumber;                      // immutable value used in the dictionary key

            internal DState(DFSA dfsa)
            {
                serialNumber = nextSN++;
                myDfaInst = null;
                myDfsa = dfsa;
            }

            internal DState(DfsaInstance inst) 
            {
                serialNumber = nextSN++;
                myDfaInst = inst;
                myDfsa = inst.parent;
            }

            public int Ord { set { instOrd = value; } }
            public bool HasRightContext { get { return rhCntx > 0 || lhCntx > 0; } }

            /// <summary>
            /// Final global number of this DState. Not valid until
            /// allocation, after separation into accept and non-accept states
            /// </summary>
            public int Num { 
                get { return globOrd; }
                set { globOrd = value; }
            }

            /// <summary>
            /// Getter for next state transition. This wrap the dictionary access.
            /// </summary>
            /// <param name="sym">Symbol ordinal of transition</param>
            /// <returns>Next state on sym, or null</returns>
            public DState GetNext(int sym)
            {
                uint key = (uint)(this.serialNumber << 16) + (ushort)sym;
                DState rslt;
                this.myDfsa.next.TryGetValue(key, out rslt);
                return rslt;
            }

            /// <summary>
            /// Enter transition in next-state dictionary
            /// </summary>
            /// <param name="sym">Symbol for transition</param>
            /// <param name="toState">Target state for transition</param>
            public void SetNext(int sym, DState toState)
            {
                uint key = (uint)(this.serialNumber << 16) + (ushort)sym;
                this.myDfsa.next.Add(key, toState);
            }

            /// <summary>
            /// When next state table is rewritten after minimization, this
            /// method builds the new next state dictionary that will replace
            /// the current dictionary.
            /// </summary>
            /// <param name="sym"></param>
            /// <param name="toState"></param>
            public void SetNewNext(int sym, DState toState)
            {
                uint key = (uint)(this.serialNumber << 16) + (ushort)sym;
                this.myDfsa.newNext.Add(key, toState);
            }

            /// <summary>
            /// Getter for predecessor list for this state. Used by the 
            /// minimizer, effectively to create an inverse next-state table.
            /// </summary>
            /// <param name="ord">symbol for transitions from predecessors</param>
            /// <returns>list of states which transition to this on ord</returns>
            public List<DState> GetPredecessors(int ord) { return predecessors[ord]; }
            public void SetPredecessors(int ord, List<DState> lst) { predecessors[ord] = lst; }
            public bool HasPredecessors() { return predecessors != null; }
            public void InitPredecessors() { predecessors = new List<DState>[myDfsa.MaxSym]; }

            public bool isStart { get { return myDfaInst.start == this; } }
            public bool isAnchor { get { return myDfaInst.anchor == this; } }

            /// <summary>
            /// Compare two DStates for next-state equivalence.
            /// </summary>
            /// <param name="other">state to compare with</param>
            /// <returns>predicate "next-state tables are equal"</returns>
            public bool EquivalentNextStates(DState other)
            {
                if (this.DefaultNext == other.DefaultNext && 
                    this.trList.Count == other.trList.Count)
                {
                    for (int i = 0; i < this.trList.Count; i++)
                    {
                        int sym = this.trList[i];
                        if (sym != other.trList[i] || this.GetNext(sym) != other.GetNext(sym)) 
                            return false;
                    }
                    return true;
                }
                else
                    return false;
            }

            /// <summary>
            /// Method to emulate full next state table from sparse data structure
            /// </summary>
            /// <param name="j"></param>
            /// <returns></returns>
            public int NextOn(int j) { return (GetNext(j) == null ? DefaultNext : GetNext(j).globOrd); }
            public int DefaultNext { get { return (myDfaInst == null ? DFSA.eofNum : DFSA.gotoStart); } }

            /// <summary>
            /// CompareTo method to allow sorting of DState values.
            /// </summary>
            /// <param name="r"></param>
            /// <returns></returns>
            public int CompareTo(DState r)
            {
                if (this.globOrd < r.globOrd) return -1;
                else if (this.globOrd > r.globOrd) return 1;
                else return 0;
            }

            public void AddPredecessor(DState pred, int smbl)
            {
                if (!HasPredecessors())
                    InitPredecessors();
                if (GetPredecessors(smbl) == null) 
                    SetPredecessors(smbl, new List<DState>());
                GetPredecessors(smbl).Add(pred);
            }

            internal void AddTrans(int ch, DState next)
            {
                SetNext(ch, next);
                trList.Add(ch);
            }

            /// <summary>
            /// Returns the name of the start condition with which this state is associated
            /// </summary>
            internal string StartConditionName
            {
                get
                {
                    if (myDfaInst != null)
                        return this.myDfaInst.myNfaInst.myStartCondition.Name;
                    else return "";
                }
            }

            internal string AbreviatedStartConditionName
            { get { string name = StartConditionName; return (name.Equals("INITIAL") ? "0" : name); } }

            /// <summary>
            /// Find the longest run of transitions with the same target
            /// in order to allow table slicing.  This must take into 
            /// account wrap around from character 'MaxSym-1' to character '\0'
            /// </summary>
            /// <param name="min">the start index of the residual table</param>
            /// <param name="rng">the length of the residual table</param>
            /// <param name="pop">the default state of the excluded run</param>
            internal void ExcludeLongestRun(out uint min, out uint rng, out int pop)
            {
                int current = NextOn(0);          // The current nextstate;
                int runLeng = 0;                  // The current run length
                int bestRun = 0;                  // Length of best run found so far.
                int bestIdx = 0;                  // Start index of remainder.
                int bestNxt = current;            // The state to exclude.
                int max = myDfsa.MaxSym;
                for (int i = 0; i < max * 2; i++) // Cater for wrap-around runs;
                {
                    int nxt = NextOn(i % max);
                    if (nxt == current) 
                        runLeng++;
                    else
                    {
                        if (runLeng > bestRun)
                        {
                            bestRun = runLeng;
                            bestIdx = i;
                            bestNxt = current;
                        }
                        current = nxt;
                        runLeng = 1;
                    }
                }
                if (bestRun == max * 2)
                {
                    min = 0; 
                    rng = 0; 
                    pop = bestNxt;
                }
                else
                {
                    min = (uint)(bestIdx % max); 
                    rng = (uint)(max - bestRun); 
                    pop = bestNxt;
                }
            }

            /// <summary>
            /// Predicate "this automaton needs to implement backup moves"
            /// </summary>
            /// <returns></returns>
            internal bool NeedsBackup()
            {
                // An accept state needs backup if it has a transition to a non-accept state
                // There has to be a quicker way to do this?
                for (int i = 0; i < myDfsa.MaxSym; i++)
                    if (GetNext(i) != null && GetNext(i).accept == null) return true;
                return false;
            }

            /// <summary>
            /// Find an example character that goes off down a backup path.
            /// </summary>
            /// <returns>string denoting the character leads to a non-accept
            /// state and might need to be discarded if the match fails</returns>
            internal string BackupTransition()
            {
                int len = myDfsa.MaxSym;
                for (int i = 0; i < len; i++)
                    if (GetNext(i) != null && GetNext(i).accept == null)
                        return myDfsa.MapSymToStr(i);
                return "EOF";
            }

            public void Diagnose()
            {
                string accept = (this.accept == null ? "" : "!");
                string start = (myDfaInst == null ? "eof:" : (isStart ? "-->" : (isAnchor ? "^->" : "")));
                Console.WriteLine("---- <{0}> {1}{2}{3}", StartConditionName, start, instOrd, accept);
            }
        }
        // End of nested DState definition.

        /// <summary>
        /// Convert the non-deterministic FSA to a deterministic FSA
        /// for each DfsaInstance, i.e. for each StartCondition
        /// </summary>
        /// <param name="nfa">The non-deterministic FSA to convert</param>
        public void Convert(NFSA nfa)
        {
            DateTime start = DateTime.Now;
            dfas = new DfsaInstance[nfa.nfas.Length];
            // Perform subset construction, separately for each NfsaInstance
            for (int i = 0; i < dfas.Length; i++)
            {
                NFSA.NfsaInstance nInst = nfa.nfas[i];
                if (nInst != null)
                {
                    DfsaInstance dInst = new DfsaInstance(nInst, this);
                    dfas[i] = dInst;
                    dInst.Convert();
                }
            }
            if (myTask.Verbose)
            {
                myTask.Msg.Write("GPLEX: DFSA built");
                myTask.Msg.Write((myTask.Errors ? ", errors detected" : " without error"));
                myTask.Msg.Write((myTask.Warnings ? "; warnings issued. " : ". "));
                myTask.Msg.WriteLine(TaskState.ElapsedTime(start));
            }
        }

        /// <summary>
        /// Write summary to the listing file.
        /// </summary>
        /// <param name="bckup">A Boolean array with true elements
        /// for every state that is a backup state</param>
        void WriteSummary(bool[] bckup)
        {
            int symCard = myTask.TargetSymCardinality;
            int fullNum = globNext * symCard;
            double totalCompression = 100.0 - (double)(tranNum * 100) / fullNum;

            myTask.ListStream.WriteLine("DFSA Summary");
            myTask.ListDivider();
            myTask.ListStream.WriteLine("Number of dfsa instances = " + (dfas.Length - 1));
            for (int i = 0; i < dfas.Length; i++)
            {
                DfsaInstance inst = dfas[i];
                if (inst != null)
                {
                    myTask.ListStream.WriteLine("Start condition " + inst.myNfaInst.key + ":");
                    myTask.ListStream.Write("  number of dfsa states = " + inst.instNext);
                    myTask.ListStream.WriteLine(", number of accept states = " + inst.acceptCount);
                }
            }
            myTask.ListDivider();
            myTask.ListStream.WriteLine("GPLEX Summary");
            myTask.ListDivider();
            myTask.ListStream.WriteLine("Total number of states = " + globNext +
                                        ", total accept states = " + maxAccept +
                                        ", backup states = " + backupCount);
            if (hasLeftAnchors)
                myTask.ListStream.WriteLine("Automaton will cater for left-anchored patterns");
            
            if (myTask.Minimize)
                myTask.ListStream.WriteLine("Original state number was {0}, minimized machine has {1} states", 
                    origLength, globNext);
            else
                myTask.ListStream.WriteLine("No state minimization.");

            double nextstateCompression = 100.0 - (double)(tranNum * 100) / (globNext * MaxSym);
            if (myTask.ChrClasses)
            {
                int entries = symCard;
                double classCompression = 100.0 - (double)(MaxSym * 100) / symCard;

                if (myTask.CompressMap)
                {
                    entries = 0;
                    foreach (MapRun r in myTask.partition.mapRuns)
                        if (r.tag == MapRun.TagType.mixedValues)
                            entries += r.Length;
                }
                myTask.ListStream.WriteLine("Compression summary: used {0:N0} nextstate entries, plus {1:N0} map entries", 
                    tranNum, entries);
                myTask.ListStream.WriteLine("- Uncompressed automaton would have {0:N0} nextstate entries", fullNum);
                myTask.ListStream.WriteLine("- Input characters are packed into {0:N0} equivalence classes", MaxSym);
                myTask.ListStream.WriteLine("- CharClass compression {0:F2}%, {1:N0} entries Vs {2:N0}", 
                    classCompression, MaxSym, symCard);
                if (myTask.CompressNext)
                {
                    myTask.ListStream.WriteLine("- Nextstate table compression {0:F2}%, {1:N0} entries Vs {2:N0}",
                        nextstateCompression, tranNum, globNext * MaxSym);
                }
                else
                {
                    myTask.ListStream.WriteLine("- Redundant row compression {0:F2}%, {1:N0} entries Vs {2:N0}",
                        nextstateCompression, tranNum, globNext * MaxSym);
                }
                if (myTask.CompressMap)
                {
                    int depth = (int)Math.Ceiling(Math.Log(myTask.partition.mapRuns.Count, 2));
                    myTask.ListStream.WriteLine(
                        "- CharacterMap compression is {0:F2}%, {1:N0} entries Vs {2:N0}",
                        100.0 - (double)(entries * 100) / symCard, entries, symCard);
                    myTask.ListStream.WriteLine("- Decision tree depth is {0}", depth);
                }
                else
                    myTask.ListStream.WriteLine("- ClassMap was not compressed");
            }
            else if (myTask.CompressNext) // compressedNext but no map ...
                myTask.ListStream.WriteLine("Nextstate table compression was {0:F2}%, {1:N0} entries Vs {2:N0}",
                                        totalCompression, tranNum, fullNum);
            else
                myTask.ListStream.WriteLine("- Redundant row compression {0:F2}%, {1:N0} entries Vs {2:N0}",
                        nextstateCompression, tranNum, globNext * MaxSym);

            if (backupCount > 0)
            {
                myTask.ListStream.WriteLine();
                myTask.ListStream.WriteLine("Backup state report --- ");
                FindShortestStrings();
                for (int i = 0; i <= maxAccept; i++)
                    if (bckup[i])
                    {
                        DState dSt = stateList[i];
                        myTask.ListStream.WriteLine(
                            "In <{0}>, after \"{1}\" automaton could accept \"{3}\" in state {2}",
                            dSt.AbreviatedStartConditionName, Utils.Map(dSt.shortestStr), i, dSt.accept.Pattern);
                        myTask.ListStream.WriteLine(
                            "--- after '{0}' automaton is in a non-accept state and might need to backup",
                            dSt.BackupTransition());
                        myTask.ListStream.WriteLine();
                    }
            }
            myTask.ListDivider();
            myTask.ListStream.WriteLine(" */");
            myTask.ListStream.Flush();
        }

        /// <summary>
        /// Minimize the FSA ... using a variant of Hopcroft's algorithm
        /// </summary>
        public void Minimize()
        {
            if (myTask.Minimize)
            {
                DateTime start = DateTime.Now;
                Minimizer mini = new Minimizer(this);
                mini.PopulatePartitions(stateList);
                mini.RefinePartitions();
                this.RewriteStateList(mini);
                if (myTask.Verbose)
                {
                    myTask.Msg.Write("GPLEX: DFSA minimized. ");
                    myTask.Msg.WriteLine(TaskState.ElapsedTime(start));
                }

            }
        }

        /// <summary>
        /// Rewrite the automaton to use the minimized states.
        /// Each partition in the final map is a DFSA state
        /// in the minimal machine.
        /// </summary>
        /// <param name="list">The minimizer object</param>
        public void RewriteStateList(Minimizer mnmzr)
        {
            List<DState> newList = new List<DState>();
            InitNewNext();
            foreach (DfsaInstance inst in dfas)
            {
                if (inst != null)
                {
                    inst.start = mnmzr.PMap(inst.start);
                    if (inst.anchor != null)
                        inst.anchor = mnmzr.PMap(inst.anchor);
                }
            }
            newList.Add(eofState);
            globNext = eofNum + 1;               // All accept state get renumbered.
            for (int idx = 1; idx < stateList.Count; idx++)
            {
                DState dSt = stateList[idx];
                DState pSt = mnmzr.PMap(dSt);
                if (dSt == pSt)
                {
                    newList.Add(pSt);
                    if (pSt.accept != null)
                        pSt.Num = globNext++;
                    for (int sym = 0; sym < this.MaxSym; sym++)
                    {
                        DState nxt = pSt.GetNext(sym);
                        if (nxt != null)
                            // Set value in *new* next-state table
                            pSt.SetNewNext(sym, mnmzr.PMap(nxt));
                    }
                }
            }
            stateList = newList; // swap old for new stateList
            OverwriteOldNext();  // replace old for new next-state table
        }

        /// <summary>
        /// Emit the scanner to the output file
        /// </summary>
        /// <param name="sRdr">the reader for the frame file</param>
        /// <param name="sWrtr">the writer for the output C# file</param>
        public void EmitScanner(StreamReader sRdr, TextWriter sWrtr)
        {
            DateTime start = DateTime.Now;
            if (sRdr != null && sWrtr != null)
            {
                int[] startMap = new int[dfas.Length];
                int[] anchorMap = new int[dfas.Length];
                bool[] backupStates = new bool[globNext]; // states that might need backup
                string line;
                // Write the expanatory header
                sWrtr.WriteLine("//");
                sWrtr.WriteLine("//  This CSharp output file generated by Gardens Point LEX");
                sWrtr.WriteLine("//  Version:  " + myTask.VerString);
                sWrtr.WriteLine("//  Machine:  " + Environment.MachineName);
                sWrtr.WriteLine("//  DateTime: " + DateTime.Now.ToString());
                sWrtr.WriteLine("//  UserName: " + Environment.UserName);
                sWrtr.WriteLine("//  GPLEX input file <" + myTask.FileName + ">");
                sWrtr.WriteLine("//  GPLEX frame file <" + myTask.FrameName + ">");
                sWrtr.WriteLine("//");
                sWrtr.WriteLine("//  Option settings:{0}{1}{2}{3}{4}{5}{6}{7}",
                    (myTask.Unicode ? " unicode," : ""),
                    (myTask.Verbose ? " verbose," : ""),
                    (myTask.HasParser ? " parser," : " noparser,"),
                    (myTask.Stack ? " stack," : ""),
                    (myTask.Minimize ? " minimize," : " nominimize,"),
                    (myTask.ChrClasses ? " classes," : ""),
                    (myTask.ChrClasses ? (myTask.CompressMap ? " compressmap," : " nocompressmap,") : ""),
                    (myTask.CompressNext ? " compressnext" : " nocompressnext"));
                sWrtr.WriteLine("//");
                sWrtr.WriteLine();
                // Number, and then sort the dfsa states according to global numbering
                maxAccept = globNext - 1;
                for (int i = 0; i < stateList.Count; i++)
                {
                    DState dSt = stateList[i];
                    if (dSt.Num == unset) 
                        dSt.Num = globNext++;
                }
                stateList.Sort();           // sorted on globOrd number
                // Only check the accept states. Backup transitions can only start here.
                for (int i = 0; i <= maxAccept; i++)
                {
                    bool need = stateList[i].NeedsBackup();
                    backupStates[i] = need;
                    if (need) backupCount++;
                }
                if (backupCount > 0)
                    sWrtr.WriteLine("#define BACKUP");
                if (hasLeftAnchors)
                    sWrtr.WriteLine("#define LEFTANCHORS");
                if (myTask.Stack)
                    sWrtr.WriteLine("#define STACK");
                if (!myTask.HasParser)
                    sWrtr.WriteLine("#define STANDALONE");
                if (myTask.Babel)
                    sWrtr.WriteLine("#define BABEL");
                for (int i = 0; i < dfas.Length; i++)
                    if (dfas[i] != null)
                    {
                        startMap[dfas[i].StartConditionOrd] = dfas[i].start.Num;
                        anchorMap[dfas[i].StartConditionOrd] =
                            (dfas[i].anchor == null ? dfas[i].start.Num : dfas[i].anchor.Num);
                    }
                //
                // Now the loop that copies the frame file line by line,
                // interleaving the generated material when required.
                //
                while (!sRdr.EndOfStream)
                {
                    line = sRdr.ReadLine();
                    if (line.StartsWith("##-->"))
                    {
                        string selector = line.Substring(5, line.Length - 5).Trim();
                        if (selector.Equals("usingDcl"))
                        {
                            foreach (LexSpan s in myTask.aast.usingStrs)
                            {
                                sWrtr.Write("using ");
                                s.StreamDump(sWrtr);
                            }
                            sWrtr.WriteLine();
                            sWrtr.Write("namespace ");
                            myTask.aast.nameString.StreamDump(sWrtr);
                        }
                        else if (selector.Equals("codeIncl"))
                            foreach (LexSpan s in myTask.aast.codeIncl)
                                s.StreamDump(sWrtr);
                        else if (selector.Equals("tableDef"))
                        {
                            if (myTask.CompressNext)
                                EmitSlicedTables(sWrtr);
                            else
                                EmitRawTables(sWrtr);
                        }
                        else if (selector.Equals("prolog"))
                        {
                            foreach (LexSpan s in myTask.aast.prolog)
                                s.StreamDump(sWrtr);
                            if (myTask.aast.epilog.Count > 0)
                                sWrtr.WriteLine("            try {");
                        }
                        else if (selector.Equals("consts"))
                        {
                            sWrtr.WriteLine("        const int maxAccept = " + maxAccept + ";");
                            sWrtr.WriteLine("        const int initial = " + startMap[0] + ";");
                            sWrtr.WriteLine("        const int eofNum = " + eofNum + ";");
                            sWrtr.WriteLine("        const int goStart = " + gotoStart + ";");
                            for (int i = 0; i < dfas.Length; i++)
                                if (dfas[i] != null)
                                    sWrtr.WriteLine(String.Format("        const int {0} = {1};",
                                                                  dfas[i].StartConditionName,
                                                                  dfas[i].StartConditionOrd));
                        }
                        else if (selector.Equals("actionCases"))
                            EmitActionCases(sWrtr, maxAccept);
                        else if (selector.Equals("epilog") && myTask.aast.epilog.Count > 0)
                        {
                            sWrtr.WriteLine("            } // end try");
                            sWrtr.WriteLine("            finally {");
                            foreach (LexSpan s in myTask.aast.epilog)
                                s.StreamDump(sWrtr);
                            sWrtr.WriteLine("            } // end finally");
                        }
                        else if (selector.Equals("userCode") &&
                            myTask.aast.userCode != null &&
                            myTask.aast.userCode.IsInitialized)
                        {
                            sWrtr.WriteLine("#region UserCodeSection");
                            sWrtr.WriteLine();
                            myTask.aast.userCode.StreamDump(sWrtr);
                            sWrtr.WriteLine();
                            sWrtr.WriteLine("#endregion");
                        }
                        else if (selector.Equals("bufferCtor"))
                        {
                            string txt = (myTask.Unicode ?
                                "buffer = TextBuff.NewTextBuff(file); // selected by /unicode option" :
                                "buffer = new StreamBuff(file);");
                            sWrtr.WriteLine("            " + txt);
                        }
                    }
                    else
                        sWrtr.WriteLine(line);
                }
                if (myTask.Summary) WriteSummary(backupStates);
                if (myTask.Verbose)
                {
                    myTask.Msg.Write("GPLEX: C# file emitted. ");
                    myTask.Msg.WriteLine(TaskState.ElapsedTime(start));
                }
                sWrtr.Flush();
            }
        }


        /// <summary>
        /// Emit the semantic actions for the recognized patterns.
        /// </summary>
        /// <param name="sWrtr">the stream writer</param>
        /// <param name="max">the max accept ordinal</param>
        internal void EmitActionCases(TextWriter sWrtr, int max)
        {
            int eofCount = 0;
            bool[] emitted = new bool[max + 1];
            sWrtr.WriteLine("#region ActionSwitch");
            sWrtr.WriteLine("#pragma warning disable 162");
            sWrtr.WriteLine("    switch (state)");
            sWrtr.WriteLine("    {");
            sWrtr.WriteLine("        case eofNum:");
            //
            //  Must check if there are any explicit EOF actions
            //
            for (int i = 0; i < dfas.Length; i++)
                if (dfas[i] != null && dfas[i].eofCode != null && dfas[i].eofCode.IsInitialized) eofCount++;
            if (eofCount >= 1)
            {
                bool[] eofDone = new bool[dfas.Length];
                sWrtr.WriteLine("            switch (currentStart) {");
                for (int i = 0; i < dfas.Length; i++)
                {
                    DfsaInstance dInst = dfas[i];
                    if (dInst != null && dInst.eofCode != null && dInst.eofCode.IsInitialized && !eofDone[i])
                    {
                        eofDone[i] = true;
                        sWrtr.WriteLine("                case " + dInst.start.Num + ":");
                        //
                        //  We wish to share the same action text spans
                        //  for all equivalent eof-actions.
                        //  Note that the test for equivalent actions is
                        //  simpler than the general case, since eof-actions
                        //  cannot have right (or for that matter left) context.
                        //
                        for (int j = i + 1; j < dfas.Length; j++)
                        {
                            DfsaInstance nInst = dfas[j];
                            if (nInst != null && 
                                nInst.eofCode != null && 
                                nInst.eofCode.IsInitialized && 
                                !eofDone[j] &&
                                SpansEqual(dInst.eofCode, nInst.eofCode))
                            {
                                eofDone[j] = true;
                                sWrtr.WriteLine("                case " + nInst.start.Num + ":");
                            }
                        }
                        dInst.eofCode.StreamDump(sWrtr);
                        sWrtr.WriteLine("                    break;");
                    }
                }
                sWrtr.WriteLine("            }");
            }
            sWrtr.WriteLine("            return (int)Tokens.EOF;");
            //
            //  Many states may share the same actions so reuse
            //  the code whenever possible. 
            //  This sharing arises in two ways:
            //  explicit use of the '|' action in the LEX
            //  file, and splitting of an accept state during
            //  construction of the automaton.
            //
            for (int sOrd = eofNum + 1; sOrd <= max; sOrd++)
            {
                DState dSt = stateList[sOrd];
                if (!emitted[sOrd])
                {
                    int rLen = dSt.rhCntx;
                    int lLen = dSt.lhCntx;
                    sWrtr.WriteLine("        case " + dSt.Num + ":"); 
                    emitted[sOrd] = true;
                    for (int j = sOrd; j <= max; j++)
                    {
                        DState nSt = stateList[j];
                        if (!emitted[j] &&
                            lLen == nSt.lhCntx && rLen == nSt.rhCntx &&
                             SpansEqual(dSt.accept.aSpan, nSt.accept.aSpan))
                        {
                            sWrtr.WriteLine("        case " + nSt.Num + ":"); 
                            emitted[j] = true;
                        }
                    }
                    if (lLen > 0) sWrtr.WriteLine("yyless({0}); ", lLen);
                    else if (rLen > 0) sWrtr.WriteLine("yyless(yyleng - {0}); ", rLen);
                    if (dSt.accept.hasAction)
                        dSt.accept.aSpan.StreamDump(sWrtr);
                    sWrtr.WriteLine("            break;");
                }
            }
            sWrtr.WriteLine("        default:");
            sWrtr.WriteLine("            break;");
            sWrtr.WriteLine("    }");
            sWrtr.WriteLine("#pragma warning restore 162");
            sWrtr.WriteLine("#endregion");
        }

        public static bool SpansEqual(LexSpan l, LexSpan r)
        {
            return l.sPos == r.sPos && l.ePos == r.ePos;
        }

        #region ClassMapHandling
        /// <summary>
        /// Write an uncompressed map from character to 
        /// equivalence class ordinal.
        /// </summary>
        /// <param name="sWrtr">The output stream writer</param>
        private void WriteClassMap(TextWriter sWrtr)
        {
            string mapTyNm;
            int domain = myTask.TargetSymCardinality;
            int range = myTask.partition.Length;
            if (range < sbyte.MaxValue)
                mapTyNm = "sbyte";
            else if (range < short.MaxValue)
                mapTyNm = "short";
            else
                mapTyNm = "int";
            sWrtr.WriteLine("#region CharacterMap"); 
            sWrtr.WriteLine("    static {0}[] map = new {0}[{1}] {{", 
                mapTyNm, domain);
            for (int i = 0; i < domain-1; i++)
            {
                if ((i % 16) == 0)
                    sWrtr.Write("/* {0,-6} */ ", Utils.Map(i));
                sWrtr.Write("{0}, ", myTask.partition[(char)i]);
                if ((i % 16) == 15)
                    sWrtr.WriteLine();
            }
            sWrtr.Write("{0} ", myTask.partition[(char)(domain-1)]);
            sWrtr.WriteLine("};");
            sWrtr.WriteLine("#endregion");
            sWrtr.WriteLine();
        }

        /// <summary>
        /// Write a compressed map from character
        /// to equivalence class ordinal.
        /// </summary>
        /// <param name="sWrtr"></param>
        private void WriteCompressedMap2(TextWriter sWrtr)
        {
            string mapTyNm;
            int mixs = 0;                   // Number of mixed runs
            int runs = 0;                   // Number of constant runs
            int sglt = 0;                   // Number of singletons
            int total = myTask.partition.mapRuns.Count;
            int entries = 0;
            int symCrd = myTask.TargetSymCardinality;
            int range = myTask.partition.Length;
            if (range < sbyte.MaxValue)
                mapTyNm = "sbyte";
            else if (range < short.MaxValue)
                mapTyNm = "short";
            else
                mapTyNm = "int";

            foreach (MapRun r in myTask.partition.mapRuns)
            {
                switch (r.tag)
                {
                    case MapRun.TagType.mixedValues:
                        mixs++;
                        entries += r.Length;
                        break;
                    case MapRun.TagType.shortRun:
                    case MapRun.TagType.longRun:
                        runs++;
                        break;
                    case MapRun.TagType.singleton:
                        sglt++;
                        break;
                    default: 
                        break;
                }
            }

            sWrtr.WriteLine("#region CharacterMap");
            sWrtr.WriteLine("    //");
            sWrtr.WriteLine("    // There are {0} equivalence classes", range);
            sWrtr.WriteLine("    // There are {0} character sequence regions", total);
            sWrtr.WriteLine("    // There are {0} tables, {1} entries", mixs, entries);
            sWrtr.WriteLine("    // There are {0} runs, {1} singletons", runs, sglt);
            sWrtr.WriteLine("    //");
            for (int n = 0; n < total; n++)
            {
                MapRun r = myTask.partition.mapRuns[n];
                if (r.tag == MapRun.TagType.mixedValues)
                {
                    sWrtr.WriteLine("    static {0}[] map{2} = new {0}[{1}] {{",
                        mapTyNm, r.Length, n);
                    for (int i = 0; i < r.Length - 1; i++)
                    {
                        int j = i + (int)r.range.minChr;
                        if ((i % 16) == 0)
                            sWrtr.Write("/* {0,-6} */ ", Utils.Map(j % symCrd));
                        sWrtr.Write("{0}, ", myTask.partition[(char)j]);
                        if ((i % 16) == 15)
                            sWrtr.WriteLine();
                    }
                    sWrtr.Write("{0} ", myTask.partition[(char)r.range.maxChr]);
                    sWrtr.WriteLine("};");
                }
            }
            sWrtr.WriteLine();
            sWrtr.WriteLine("    {0} Map(int chr)", mapTyNm);
            sWrtr.Write("    { ");
            EmitDecisionCode(sWrtr, mapTyNm, 4, 0, symCrd - 1, 0, total - 1);
            sWrtr.WriteLine();
            sWrtr.WriteLine("    }");
            sWrtr.WriteLine("#endregion");
            sWrtr.WriteLine();
        }

        /// <summary>
        /// Emit the decision code tree that selects the
        /// applicable run in the equivalence class map.
        /// The algorithm takes a sub-range of the partition run array,
        /// that applies between known min an max character bounds.
        /// The algorithm bisects the subrange and recurses, until a
        /// subrange of just a single run is encountered.
        /// </summary>
        /// <param name="sWrtr">The output text writer</param>
        /// <param name="mapTyNm">Text name of the table element type</param>
        /// <param name="indent">Indent depth for formatting lines of code</param>
        /// <param name="min">Minimum char value treated by this iteration</param>
        /// <param name="max">Maximum char value treated by this iteration</param>
        /// <param name="first">Lowest numbered run to consider</param>
        /// <param name="last">Highest numbered run to consider</param>
        private void EmitDecisionCode(
            TextWriter sWrtr, string mapTyNm, int indent, int min, int max, int first, int last)
        {
            if (last == first)
            {
                int mapOfMin = myTask.partition[(char)min];
                if (min == max) // this is a singleton;
                    sWrtr.Write("return ({0}){1};", mapTyNm, mapOfMin);
                else if (myTask.partition.mapRuns[first].tag == MapRun.TagType.mixedValues)
                    sWrtr.Write("return map{0}[chr - {1}];", first, min);
                else
                    sWrtr.Write("return ({0}){1};", mapTyNm, mapOfMin);
            }
            else
            {
                int midRun = (first + last + 1) / 2;
                int midPoint = myTask.partition.mapRuns[midRun].range.minChr;
                sWrtr.Write("// '{0}' <= chr <= '{1}'", Utils.Map(min), Utils.Map(max));
                Indent(sWrtr, indent + 2);  
                sWrtr.Write("if (chr < {0}) ", midPoint);
                EmitDecisionCode(sWrtr, mapTyNm, indent + 2, min, midPoint - 1, first, midRun - 1);
                Indent(sWrtr, indent + 2); sWrtr.Write("else ");
                EmitDecisionCode(sWrtr, mapTyNm, indent + 2, midPoint, max, midRun, last);
            }
        }

        private void Indent(TextWriter sWrtr, int count)
        {
            sWrtr.WriteLine();
            for (int i = 0; i < count; i++) sWrtr.Write(" ");
        }
        #endregion

        /// <summary>
        /// Write out the scanner tables: this version emits sliced tables
        /// with the longest same-next-state run excluded from the table.
        /// "Longest Run" takes into account wrap-around from MaxChar to
        /// chr(0).
        /// </summary>
        /// <param name="sWrtr">The output text writer</param>
        internal void EmitSlicedTables(TextWriter sWrtr)
        {
            bool isByte = stateList.Count < 128;
            bool doMap = myTask.ChrClasses;
            bool bigMap = doMap && myTask.CompressMap;
            string eType = (isByte ? "sbyte" : "short");
            string symStr;
            
            if (!doMap)
                symStr = "chr";
            else if (bigMap)
                symStr = "Map(chr)";
            else
                symStr = "map[chr]";

            if (myTask.Verbose) FindShortestStrings();

            sWrtr.WriteLine("#region ScannerTables");
            sWrtr.WriteLine("    struct Table {");
            sWrtr.WriteLine("        public int min; public int rng; public int dflt;");
            sWrtr.WriteLine("        public {0}[] nxt;", eType);
            sWrtr.WriteLine("        public Table(int m, int x, int d, {0}[] n) {{", eType);
            sWrtr.WriteLine("            min = m; rng = x; dflt = d; nxt = n;");
            sWrtr.WriteLine("        }");
            sWrtr.WriteLine("    };"); sWrtr.WriteLine();
            //
            // Emit the start state index for each StartCondition.
            //
            sWrtr.Write("    static int[] startState = {");
            for (int i = 0; i < dfas.Length; i++)
            {
                DfsaInstance dInst = dfas[i];
                sWrtr.Write((dInst == null ? eofNum : dInst.start.Num));
                if (i < dfas.Length - 1)
                {
                    sWrtr.Write(", ");
                    if (i % 16 == 5) { sWrtr.WriteLine(); sWrtr.Write("        "); }
                }
            }
            sWrtr.WriteLine("};"); sWrtr.WriteLine();
            //
            // Emit the left anchored state index for each StartCondition.
            //
            if (hasLeftAnchors)
            {
                sWrtr.Write("   static int[] anchorState = {");
                for (int i = 0; i < dfas.Length; i++)
                {
                    DfsaInstance dInst = dfas[i];
                    int anchorOrd = eofNum;
                    if (dInst != null)
                        anchorOrd = (dInst.anchor == null ? dInst.start.Num : dInst.anchor.Num);
                    sWrtr.Write((dInst == null ? eofNum : anchorOrd));
                    if (i < dfas.Length - 1)
                    {
                        sWrtr.Write(", ");
                        if (i % 16 == 5) { sWrtr.WriteLine(); sWrtr.Write("        "); }
                    }
                }
                sWrtr.WriteLine("};"); sWrtr.WriteLine();
            }

            if (bigMap)
                WriteCompressedMap2(sWrtr);
            else if (myTask.ChrClasses)
                WriteClassMap(sWrtr);
            
            sWrtr.WriteLine("    static Table[] NxS = new Table[{0}];", stateList.Count);
            sWrtr.WriteLine();
            sWrtr.WriteLine("    static Scanner() {");
            for (int i = 0; i < stateList.Count; i++)
            {
                DState dSt = stateList[i];
                int stDef = dSt.DefaultNext;
                sWrtr.Write("    NxS[{0}] = ", i);
                if (dSt.trList.Count == 0)
                {
                    sWrtr.Write("new Table(0, 0, {0}, null);", stDef);
                    if (myTask.Verbose) 
                        sWrtr.Write(" // Shortest string \"{0}\"{1}", Utils.Map(dSt.shortestStr), Environment.NewLine);
                }
                else
                {
                    int dflt = 0;   // The excluded nextstate value.
                    uint min = 0;   // Start index of remainder, in [0-255]
                    uint rng = 0;   // Number of elements in the remainder
                    if (myTask.Verbose)
                        sWrtr.Write("// Shortest string \"{0}\"{1}      ", Utils.Map(dSt.shortestStr), Environment.NewLine);
                    dSt.ExcludeLongestRun(out min, out rng, out dflt);
                    tranNum += (int)rng;
                    sWrtr.Write("new Table({0}, {1}, {2}, new {3}[] {{", min, rng, dflt, eType);
                    for (uint j = 0; j < rng; j++)
                    {
                        sWrtr.Write(dSt.NextOn((int)((j + min) % this.MaxSym)));
                        if (j < rng - 1)
                        {
                            sWrtr.Write(", ");
                            if (j % 16 == 5) { sWrtr.WriteLine(); sWrtr.Write("          "); }
                        }
                    }
                    sWrtr.WriteLine("});");
                }
            }
            sWrtr.WriteLine("    }");
            sWrtr.WriteLine();
            sWrtr.WriteLine("int NextState(int qStat) {");
            sWrtr.WriteLine("    if (chr == ScanBuff.EOF)");
            // The reason for the logic of the following line is this:
            // If the current state is an accept state, and eof is
            // found, return currentState to trigger acceptance.
            // If the current state is not accept, or is a start
            // state, then go to the distinguished eof state.
            sWrtr.WriteLine("        return (qStat <= maxAccept && qStat != currentStart ? currentStart : eofNum);");
            sWrtr.WriteLine("    else {");
            sWrtr.WriteLine("        int rslt;");
            if (myTask.ChrClasses)
            {
                sWrtr.WriteLine("        int idx = {0} - NxS[qStat].min;", symStr);
                sWrtr.WriteLine("        if (idx < 0) idx += {0};", MaxSym);
            }
            else
            {
                sWrtr.WriteLine("        int idx = (byte)({0} - NxS[qStat].min);", symStr);
            }
            sWrtr.WriteLine("        if ((uint)idx >= (uint)NxS[qStat].rng) rslt = NxS[qStat].dflt;");
            sWrtr.WriteLine("        else rslt = NxS[qStat].nxt[idx];");
            sWrtr.WriteLine("        return (rslt == goStart ? currentStart : rslt);");
            sWrtr.WriteLine("    }");
            sWrtr.WriteLine('}');
            sWrtr.WriteLine();
            sWrtr.WriteLine("int NextState() {");
            sWrtr.WriteLine("    if (chr == ScanBuff.EOF)");
            sWrtr.WriteLine("        return (state <= maxAccept && state != currentStart ? currentStart : eofNum);");
            sWrtr.WriteLine("    else {");
            sWrtr.WriteLine("        int rslt;");
            if (myTask.ChrClasses)
            {
                sWrtr.WriteLine("        int idx = {0} - NxS[state].min;", symStr);
                sWrtr.WriteLine("        if (idx < 0) idx += {0};", MaxSym);
            }
            else
            {
                sWrtr.WriteLine("        int idx = (byte)({0} - NxS[state].min);", symStr);
            }
            sWrtr.WriteLine("        if ((uint)idx >= (uint)NxS[state].rng) rslt = NxS[state].dflt;");
            sWrtr.WriteLine("        else rslt = NxS[state].nxt[idx];");
            sWrtr.WriteLine("        return (rslt == goStart ? currentStart : rslt);");
            sWrtr.WriteLine("    }");
            sWrtr.WriteLine('}');
            sWrtr.WriteLine("#endregion");
            sWrtr.Flush();
        }

        /// <summary>
        ///  Emit uncompressed nextstate tables.
        /// </summary>
        /// <param name="sWrtr">The output text writer</param>
        internal void EmitRawTables(TextWriter sWrtr)
        {
            bool isByte = stateList.Count < 128;
            bool doMap = myTask.ChrClasses;
            bool bigMap = doMap && myTask.CompressMap;
            string eType = (isByte ? "sbyte" : "short");
            string symStr;

            if (!doMap)
                symStr = "chr";
            else if (bigMap)
                symStr = "Map(chr)";
            else
                symStr = "map[chr]";

            if (myTask.Verbose) 
                FindShortestStrings();

            sWrtr.WriteLine("#region ScannerTables");
            sWrtr.Write("  static int[] startState = {");
            for (int i = 0; i < dfas.Length; i++)
            {
                DfsaInstance dInst = dfas[i];
                sWrtr.Write((dInst == null ? eofNum : dInst.start.Num));
                if (i < dfas.Length - 1)
                {
                    sWrtr.Write(", ");
                    if (i % 16 == 5) { sWrtr.WriteLine(); sWrtr.Write("        "); }
                }
            }
            sWrtr.WriteLine("};"); sWrtr.WriteLine();
            if (hasLeftAnchors)
            {
                sWrtr.Write("   static int[] anchorState = {");
                for (int i = 0; i < dfas.Length; i++)
                {
                    DfsaInstance dInst = dfas[i];
                    int anchorOrd = eofNum;
                    if (dInst != null)
                        anchorOrd = (dInst.anchor == null ? dInst.start.Num : dInst.anchor.Num);
                    sWrtr.Write((dInst == null ? eofNum : anchorOrd));
                    if (i < dfas.Length - 1)
                    {
                        sWrtr.Write(", ");
                        if (i % 16 == 5) { sWrtr.WriteLine(); sWrtr.Write("        "); }
                    }
                }
                sWrtr.WriteLine("};"); sWrtr.WriteLine();
            }
            if (bigMap)
                WriteCompressedMap2(sWrtr);
            else if (myTask.ChrClasses)
                WriteClassMap(sWrtr);
            
            int len = this.maxSym;
            sWrtr.WriteLine("  static {0}[][] nextState = new {0}[{1}][];", eType, stateList.Count);
            sWrtr.WriteLine("  static Scanner() {");
            for (int i = 0; i < stateList.Count; i++)
            {
                DState dSt = stateList[i];
                // ====== Replace redundant rows with an alias =======
                bool usedShortCircuit = false;
                sWrtr.Write("    nextState[{0}] = ", i);
                for (int j = 0; j < i; j++)
                    if (dSt.EquivalentNextStates(stateList[j]))
                    {
                        copyNum++;  // Keep count on alias rows for statistics
                        usedShortCircuit = true;
                        sWrtr.Write("nextState[{0}];", j);
                            if (myTask.Verbose) sWrtr.Write(
                                " // Shortest string \"{0}\"{1}", Utils.Map(dSt.shortestStr), Environment.NewLine);
                        break;
                    }
                if (!usedShortCircuit)
                {
                    sWrtr.Write("new {0}[] {{", eType);
                    if (myTask.Verbose) sWrtr.Write(
                        " // Shortest string \"{0}\"{1}        ", Utils.Map(dSt.shortestStr), Environment.NewLine);
                    for (int j = 0; j < len; j++)
                    {
                        if (dSt.GetNext(j) == null) sWrtr.Write(dSt.DefaultNext);
                        else sWrtr.Write(dSt.GetNext(j).Num);
                        if (j < (len - 1))
                        {
                            sWrtr.Write(", ");
                            if (j % 16 == 15) { sWrtr.WriteLine(); sWrtr.Write("        "); }
                        }
                    }
                    sWrtr.WriteLine("};");
                }

            }
            sWrtr.WriteLine('}');

            tranNum = (globNext - copyNum) * this.MaxSym;
            sWrtr.WriteLine();
            sWrtr.WriteLine("int NextState() {");
            sWrtr.WriteLine("    if (chr == ScanBuff.EOF)");
            sWrtr.WriteLine("        return (state <= maxAccept && state != currentStart ? currentStart : eofNum);");
            sWrtr.WriteLine("    else {{ int rslt = nextState[state][{0}]; return (rslt == goStart? currentStart : rslt); }}", symStr);
            sWrtr.WriteLine('}');
            sWrtr.WriteLine();
            sWrtr.WriteLine("int NextState(int query) {");
            sWrtr.WriteLine("    if (chr == ScanBuff.EOF)");
            sWrtr.WriteLine("        return (query <= maxAccept && query != currentStart ? currentStart : eofNum);");
            sWrtr.WriteLine("    else {{ int rslt = nextState[query][{0}]; return (rslt == goStart? currentStart : rslt); }}", symStr);
            sWrtr.WriteLine('}');
            sWrtr.WriteLine("#endregion");
            sWrtr.Flush();
        }
    }
}

// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using GPLEX.Automaton;

namespace GPLEX.Parser
{
    /// <summary>
    /// Partition is the class that represents a 
    /// partitioning of the character set into 
    /// equivalence classes for a particular set
    /// of literals from a given LEX specification.
    /// The literals are of two kinds: singleton
    /// characters and character sets [...].
    /// A partition object is initialized with 
    /// a single range from CharMin to CharMax,
    /// this is refined by the Refine method.
    /// The invariants of the partition are:
    /// (1) the character sets denoted by 
    /// the partition elements are disjoint,
    /// and together cover the character range.
    /// (2) for every processed literal L, and
    /// every partition element E, either every
    /// character in E is in L, or none is.
    /// </summary>
    public class Partition
    {
        public const int CUTOFF = 64; // Shortest run to consider

        List<PartitionElement> elements = new List<PartitionElement>();
        internal BitArray singletons;
        internal List<MapRun> mapRuns;

        int[] cMap;

        int[] iMap;

        /// <summary>
        /// Create a new partition
        /// </summary>
        /// <param name="symCard">The symbol alphabet cardinality</param>
        public Partition(int symCard)
        {
            CharRange.Init(symCard);
            PartitionElement.Reset();
            singletons = new BitArray(symCard);
            elements.Add(PartitionElement.AllChars());
        }

        /// <summary>
        /// The mapping function from character
        /// ordinal to equivalence class ordinal.
        /// </summary>
        public int this[char ch] { get { return cMap[(int)ch]; } }

        /// <summary>
        /// A projection of the inverse map from class ordinal
        /// back to character ordinal. iMap returns an example 
        /// character, used for diagnostic information only.
        /// </summary>
        public int InvMap(int ch) { return iMap[ch]; }

        /// <summary>
        /// The number of equivalence classes.
        /// </summary>
        public int Length { get { return elements.Count; } }

        internal void FindClasses(AAST aast)
        {
            Accumulator visitor = new Accumulator(this, aast);
            foreach (RuleDesc rule in aast.ruleList)
            {
                RegExTree reTree = rule.Tree;
                reTree.Visit(visitor);
            }
        }

        /// <summary>
        /// Fix the partition, and generate the forward and
        /// inverse mappings "char : equivalence class"
        /// </summary>
        internal void FixMap()
        {
            cMap = new int[CharRange.SymCard];
            iMap = new int[elements.Count];
            foreach (PartitionElement pElem in elements)
            {
                if (pElem.list.Ranges.Count > 0)
                    iMap[pElem.ord] = pElem.list.Ranges[0].minChr;
                foreach (CharRange range in pElem.list.Ranges)
                    for (int i = range.minChr; i <= range.maxChr; i++)
                        cMap[i] = pElem.ord;
            }
            mapRuns = FindRuns();
        }

        /// <summary>
        /// Refine the current partition with respect 
        /// to the given range literal "lit".
        /// </summary>
        /// <param name="lit">The Range Literal</param>
        internal void Refine(RangeLiteral lit)
        {
            int idx;
            int max = elements.Count; // because this varies inside the loop
            //
            // For each of the *current* elements of the partition do:
            //
            for (idx = 0; idx < max; idx++)
            {
                PartitionElement elem = elements[idx];
                RangeList intersection = lit.list.AND(elem.list);
                // 
                // There are four cases here:
                // (1) No intersection of lit and elem ... do nothing
                // (2) Literal properly contains the partition element ...
                //     Add this element to the equivClasses list of this lit.
                //     Add this lit to the list of literals dependent on "elem".
                //     The intersection of other parts of the literal with other
                //     elements will be processed by other iterations of the loop.
                // (3) Literal is properly contained in the partition element ...
                //     Split the element into two: a new element containing the
                //     intersection, and an updated "elem" with the intersection
                //     subtracted. All literals dependent on the old element are
                //     now dependent on the new element and (the new version of)
                //     this element. The literal cannot overlap with any other 
                //     element, so the iteration can be terminated.
                // (4) Literal partially overlaps the partition element ...
                //     Split the element as for case 2.  Overlaps of the rest
                //     of the literal with other elements will be dealt with by
                //     other iterations of the loop. 
                //
                if (!intersection.IsEmpty) // not empty intersection
                {
                    // Test if elem is properly contained in lit
                    // If so, intersection == elem ...
                    if (intersection.EQU(elem.list))
                    {
                        elem.literals.Add(lit);
                        lit.equivClasses.Add(elem.ord);
                    }
                    else
                    {
                        PartitionElement newElem =
                            new PartitionElement(intersection.Ranges, false);
                        elements.Add(newElem);
                        lit.equivClasses.Add(newElem.ord);
                        newElem.literals.Add(lit);
                        //
                        //  We are about to split elem.
                        //  All literals that include elem
                        //  must now also include newElem
                        //
                        foreach (RangeLiteral rngLit in elem.literals)
                        {
                            rngLit.equivClasses.Add(newElem.ord);
                            newElem.literals.Add(rngLit);
                        }
                        elem.list = elem.list.SUB(intersection);
                        //
                        // Test if lit is a subset of elem
                        // If so, intersection == lit and we can
                        // assert that no other loop iteration has
                        // a non-empty intersection with this lit.
                        //
                        if (intersection.EQU(lit.list))
                            return;
                    }
                }
            }
        }

        internal List<MapRun> FindRuns()
        {
            List<MapRun> result = new List<MapRun>();
            int max = singletons.Count;
            int start = 0;                       // Start of the run
            int value = cMap[0];                 // Map value of the run
            for (int chOrd = 0; chOrd <= max; chOrd++)
            {
                int next = (chOrd == max ? -1 : cMap[chOrd]);
                if (next != value)               // The run has ended at chOrd-1
                {
                    if (result.Count == 0) 
                        result.Add(new MapRun(start, chOrd - 1));
                    else
                    {
                        MapRun last = result[result.Count - 1];
                        int length = chOrd - start;
                        if (length >= Partition.CUTOFF || last.tag == MapRun.TagType.longRun)
                            result.Add(new MapRun(start, chOrd - 1));
                        else
                            last.Merge(start, chOrd - 1);
                    }
                    start = chOrd;
                    value = next;  
                }
            }
            return result;
        }
    }

    /// <summary>
    /// This is the visitor pattern that extracts
    /// ranges from the leaves of the Regular Expressions.
    /// The RegExDFS base class provides the traversal
    /// code, while this extension provides the "Op" method.
    /// </summary>
    public class Accumulator : RegExDFS
    {
        AAST aast;
        Partition partition;

        internal Accumulator(Partition part, AAST aast)
        {
            this.partition = part;
            this.aast = aast;
        }

        private void DoSingleton(char ch)
        {
            if (!partition.singletons[(int)ch])
            {
                partition.singletons[(int)ch] = true;
                partition.Refine(new RangeLiteral(ch));
            }
        }

        private void DoLiteral(RangeLiteral lit)
        {
            if (lit.part != this.partition)
            {
                lit.part = this.partition;
                lit.list.Canonicalize();
                partition.Refine(lit);
            }
        }

        public override void Op(RegExTree tree)
        {
            Leaf leaf = tree as Leaf;
            if (leaf != null)
            {
                switch (leaf.op)
                {
                    case RegOp.primitive:
                        DoSingleton(leaf.chVal);
                        break;
                    case RegOp.litStr:
                        for (int i = 0; i < leaf.str.Length; i++)
                            DoSingleton(leaf.str[i]);
                        break;
                    case RegOp.charClass:
                        DoLiteral(leaf.rangeLit);
                        break;
                    case RegOp.eof: // no action required
                        break;
                    default:
                        throw new Exception("Unknown RegOp");
                }
            }
        }
    }

    /// <summary>
    /// Represents a set of characters as a
    /// list of character range objects.
    /// </summary>
    public class RangeList
    {
        // Notes:
        // This is a sparse representation of the character set. The
        // operations that are supported in primitive code are AND,
        // the inversion of the underlying List<CharRange>, and
        // value equality EQU. This is functionally complete, over
        // the Boolean operations. The set difference SUB is packaged
        // as AND (inverse of rh operand).

        /// <summary>
        /// Asserts that the list has been canonicalized, that is
        /// (1) the ranges are in sorted order of minChr
        /// (2) there is no overlap of ranges
        /// (3) contiguous ranges have been merged
        /// (3) invert is false.
        /// The set operations AND, SUB, EQU rely on this property!
        /// </summary>
        private bool isCanonical = true;
        private bool invert = false;

        private List<CharRange> ranges;
        internal List<CharRange> Ranges { get { return ranges; } }

        /// <summary>
        /// Construct a new RangeList with the given
        /// list of ranges.
        /// </summary>
        /// <param name="ranges">the list of ranges</param>
        /// <param name="invert">if true, means the inverse of the list</param>
        internal RangeList(List<CharRange> ranges, bool invert) {
            this.invert = invert;
            this.ranges = ranges;
        }

        /// <summary>
        /// Construct an empty list, initialized with
        /// the invert flag specified.
        /// </summary>
        /// <param name="invert"></param>
        internal RangeList(bool invert) {
            this.invert = invert;
            ranges = new List<CharRange>(); 
        }

        /// <summary>
        /// Construct an RangeList corresponding to the
        /// given bit-vector representation.  Only use this
        /// if you really need to, as it is slow.
        /// </summary>
        /// <param name="bits"></param>
        private RangeList(BitArray bits)
        {
            invert = false;
            ranges = new List<CharRange>();
            int j = 0;
            int max = bits.Length;
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
                ranges.Add(new CharRange((char)start, (char)(j - 1)));
            }
            // No Canonicalization is required in this case.
            isCanonical = true;
        }

        internal bool IsEmpty { get { return ranges.Count == 0; } }
        internal bool IsInverted { get { return invert; } }

        internal void Add(CharRange rng)
        {
            ranges.Add(rng);  // AddToRange
            isCanonical = !invert && ranges.Count == 1;
        }

        /// <summary>
        /// Return a new RangeList that is the intersection of
        /// "this" and rhOp.  Neither operand is mutated.
        /// </summary>
        /// <param name="rhOp"></param>
        /// <returns></returns>
        internal RangeList AND(RangeList rhOp)
        {
            if (!isCanonical || !rhOp.isCanonical) 
                throw new Exception();
            if (this.ranges.Count == 0 || rhOp.ranges.Count == 0)
                return new RangeList(false); // return empty RangeList

            int thisIx;
            int rhOpIx = 0;
            int thisNm = this.ranges.Count;
            int rhOpNm = rhOp.ranges.Count;
            List<CharRange> newList = new List<CharRange>();
            RangeList result = new RangeList(newList, false);
            CharRange rhOpElem = rhOp.ranges[rhOpIx++];
            for (thisIx = 0; thisIx < thisNm; thisIx++)
            {
                CharRange thisElem = this.ranges[thisIx];
                // Attempt to find an overlapping element.
                // If necessary fetch new elements from rhOp
                // until maxChr of the new element is greater
                // than minChr of the current thisElem.
                while (rhOpElem.maxChr < thisElem.minChr)
                    if (rhOpIx < rhOpNm)
                        rhOpElem = rhOp.ranges[rhOpIx++];
                    else
                        return result;
                // It is possible that the rhOpElem is entirely beyond thisElem
                // It is also possible that rhOpElem and several following 
                // elements are all overlapping with thisElem.
                while (rhOpElem.minChr <= thisElem.maxChr)
                {
                    // process overlap
                    newList.Add(new CharRange(
                        (char)(thisElem.minChr < rhOpElem.minChr ? rhOpElem.minChr : thisElem.minChr),
                        (char)(thisElem.maxChr < rhOpElem.maxChr ? thisElem.maxChr : rhOpElem.maxChr)));
                    // If rhOpElem extends beyond thisElem.maxChr it is possible that
                    // it will overlap with the next thisElem, so do not advance rhOpIx.
                    if (rhOpElem.maxChr > thisElem.maxChr)
                        break;
                    else if (rhOpIx == rhOpNm)
                        return result;
                    else 
                        rhOpElem = rhOp.ranges[rhOpIx++];
                }
            }
            return result;
        }

        /// <summary>
        /// Return a list of char ranges that represents
        /// the inverse of the set represented by "this".
        /// The RangeList must be sorted but not necessarily
        /// completely canonicalized.
        /// </summary>
        /// <returns></returns>
        internal List<CharRange> InvertedList()
        {
            int index = 0;
            List<CharRange> result = new List<CharRange>();
            foreach (CharRange range in this.ranges)
            {
                if (range.minChr > index)
                    result.Add(new CharRange((char)index, (char)(range.minChr - 1)));
                index = range.maxChr + 1;
            }
            if (index < CharRange.SymCard)
                result.Add(new CharRange((char)index, (char)(CharRange.SymCard - 1)));
            return result;
        }

        /// <summary>
        /// Return the set difference of "this" and rhOp
        /// </summary>
        /// <param name="rhOp"></param>
        /// <returns></returns>
        internal RangeList SUB(RangeList rhOp)
        {
            if (!isCanonical || !rhOp.isCanonical) 
                throw new Exception();
            if (this.ranges.Count == 0)
                return new RangeList(false);
            else if (rhOp.ranges.Count == 0)
                return this;
            return this.AND(new RangeList(rhOp.InvertedList(), false)); 
        }

        /// <summary>
        /// Check value equality for "this" and rhOp.
        /// </summary>
        /// <param name="rhOp"></param>
        /// <returns></returns>
        internal bool EQU(RangeList rhOp)
        {
            if (!isCanonical || !rhOp.isCanonical) 
                throw new Exception();
            if (this == rhOp)
                return true;
            else if (this.ranges.Count != rhOp.ranges.Count)
                return false;
            else
            {
                for (int i = 0; i < this.ranges.Count; i++)
                    if (rhOp.ranges[i].CompareTo(ranges[i]) != 0) 
                        return false;
                return true;
            }
        }

        /// <summary>
        /// Canonicalize the set. This may mutate
        /// both this.ranges and the invert flag.
        /// </summary>
        internal void Canonicalize()
        {
            if (!invert && this.ranges.Count <= 1) 
                return; // Empty and singleton RangeLists are trivially canonical
            // Process non-empty lists.
            int listIx = 0;
            this.ranges.Sort();
            List<CharRange> newList = new List<CharRange>();
            CharRange currentRange = ranges[listIx++];
            while (listIx < ranges.Count)
            {
                CharRange nextRange = ranges[listIx++];
                if (nextRange.minChr > currentRange.maxChr)
                {
                    newList.Add(currentRange);
                    currentRange = nextRange;
                }
                else if (nextRange.minChr < currentRange.maxChr && nextRange.maxChr > currentRange.maxChr)
                    currentRange = new CharRange((char)currentRange.minChr, (char)nextRange.maxChr);
                // Else skip ...
            }
            newList.Add(currentRange);
            if (this.invert)
            {
                this.invert = false;
                this.ranges = this.InvertedList();
            }
            isCanonical = true;
        }
    }

    /// <summary>
    /// Represents a contiguous range of characters
    /// between a given minimum and maximum values.
    /// </summary>
    public class CharRange : IComparable<CharRange>
    {
        private static int symCard = 0;
        public static int SymCard { get { return symCard; } }
        public static void Init(int num) { symCard = num; }
        public static CharRange AllChars { get { return new CharRange((char)0, (char)(symCard - 1)); } }

        internal int minChr;
        internal int maxChr;

        public CharRange(char min, char max)
        {
            minChr = (int)min;
            maxChr = (int)max;
        }

        public CharRange(char chr) { minChr = maxChr = (int)chr; }

        public override string ToString()
        {
            if (minChr == maxChr)
                return String.Format("singleton char {0}", Utils.Map(minChr));
            else
                return String.Format("char range {0} .. {1}, {2} chars", 
                    Utils.Map(minChr), Utils.Map(maxChr), maxChr - minChr + 1);
        }

        public int CompareTo(CharRange rhOp)
        {
            if (minChr < rhOp.minChr)
                return -1;
            else if (minChr > rhOp.minChr)
                return +1;
            else if (maxChr > rhOp.maxChr)
                // When two ranges start at the same minChr
                // we want the longer range to come first.
                return -1;
            else if (maxChr < rhOp.maxChr)
                return +1;
            else
                return 0;
        }
    }

    /// <summary>
    /// This class represents a single partition in 
    /// a partition set. Each such element denotes
    /// a set of characters that belong to the same
    /// equivalence class with respect to the literals
    /// already processed.
    /// </summary>
    internal class PartitionElement
    {
        static int nextOrd = 0;

        internal static void Reset()
        { nextOrd = 0; }

        internal static PartitionElement AllChars()
        {
            List<CharRange> singleton = new List<CharRange>();
            singleton.Add(CharRange.AllChars);
            return new PartitionElement(singleton, false);
        }


        internal int ord;
        internal RangeList list = null;

        /// <summary>
        /// List of literals that contain this partition element.
        /// </summary>
        internal List<RangeLiteral> literals = new List<RangeLiteral>();

        internal PartitionElement(List<CharRange> ranges, bool invert)
        {
            ord = nextOrd++;
            list = new RangeList(ranges, invert);
        }
    }

    /// <summary>
    /// Represents a character set literal as a
    /// list of character ranges.  A direct mapping
    /// of a LEX character set "[...]".
    /// The field equivClasses holds the list of
    /// ordinals of the partition elements that
    /// cover the characters of the literal.
    /// </summary>
    internal class RangeLiteral
    {
        internal string name;

        /// <summary>
        /// The last partition that this literal has refined
        /// </summary>
        internal Partition part = null;

        internal RangeList list;
        internal List<int> equivClasses = new List<int>();

        internal RangeLiteral(bool invert) { list = new RangeList(invert); }
        internal RangeLiteral(char ch)
        {
            list = new RangeList(false);
            list.Add(new CharRange(ch, ch)); // AddToRange
        }

        internal bool Empty { get { return list.IsEmpty; } }

        public override string ToString() { return name; }
    }

    /// <summary>
    /// For the compression of sparse maps, adjacent sequences
    /// of character values are denoted as singletons (a single char),
    /// shortRuns (a run of identical values but shorter than CUTOFF)
    /// longRuns (a run of identical values longer than CUTOFF) and
    /// mixed values (a run containing two or more different values.
    /// </summary>
    internal class MapRun
    {
        internal enum TagType { empty, singleton, shortRun, longRun, mixedValues }

        internal TagType tag = TagType.empty;
        internal CharRange range;

        internal MapRun(int min, int max)
        {
            range = new CharRange((char)min, (char)max);
            if (min == max)
                tag = TagType.singleton;
            else if (max - min > Partition.CUTOFF)
                tag = TagType.longRun;
            else
                tag = TagType.shortRun;
        }

        internal int Length { 
            get { return ((int)range.maxChr - (int)range.minChr + 1); } 
        }

        internal void Merge(int min, int max)
        {
            // Assert: this.range.maxChr == min - 1 ... should check?
            this.range.maxChr = (char)max;
            this.tag = TagType.mixedValues;
        }

    }
}

// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Text;
using GPLEX.Parser;

namespace GPLEX.Automaton
{
	public enum OptionState { clear, needUsage, inconsistent, alphabetLocked, errors};

    /// <summary>
    /// A singleton of this type holds the main program state during
    /// processing a LEX file when run from the command line. It sets
    /// up the parser, scanner, errorHandler and AAST objects and calls
    /// Parse() on the parser.  When the parser is invoked by Visual
    /// Studio, by contrast, there is no task state and Parse is called
    /// from the managed babel wrapper.
    /// </summary>
	public class TaskState
	{
        const string dotLex = ".lex";
        const string dotLst = ".lst";
        const string dotCs = ".cs";
        readonly string version;
        const int notSet = -1;
        const int asciiCardinality = 256;
        const int unicodeCardinality = 0x10000;

        int hostSymCardinality = asciiCardinality;
        int targetSymCardinality = notSet;

        bool trace = false;
        bool stack = false;
        bool babel = false;
		bool verbose = false;
		bool checkOnly = false;
        bool parseOnly = false;
		bool summary = false;
		bool listing = false;
        bool emitVer = false;
        bool compressExplicit = false;
        bool compressMap = true;              // Compress the class map
        bool compressNext = true;             // Compress the next-state tables
        bool minimize = true;
        bool hasParser = true;
        bool charClasses = false;
        bool useUnicode = false;
		string fileName = null;               // Filename of the input file.
        string pathName = null;               // Input file path string from user.
        string dirName = null;                // Directory name of input file.
        string outName = null;                // Output file path string (possibly empty)
        string baseName = null;               // Base name of input file, without extension.
        string exeDirNm = null;               // Directory name from which program executed.
        string userFrame = null;              // Usually null.  Set by the /frame:path options
        string dfltFrame = "gplexx.frame";    // Default frame name.
        string frameName = null;              // Path of the frame file actually used.

		Stream inputFile = null;
        StreamWriter listWriter = null;
        TextWriter msgWrtr = Console.Out;

		NFSA nfsa = null;
		DFSA dfsa = null;
        internal Partition partition = null;

		internal AAST aast = null;
        public ErrorHandler handler = null;
        public GPLEX.Parser.Parser parser = null;
        public GPLEX.Lexer.Scanner scanner = null;

		public TaskState() 
        {
            Assembly assm = Assembly.GetExecutingAssembly();
            object info = Attribute.GetCustomAttribute(assm, typeof(AssemblyInformationalVersionAttribute));
            this.version = ((AssemblyInformationalVersionAttribute)info).InformationalVersion;
        }

		// Support for various properties of the task
        public bool Trace { get { return trace; } }
        public bool Stack { get { return stack; } }
        public bool Babel { get { return babel; } }
        public bool Verbose { get { return verbose; } }
        public bool HasParser { get { return hasParser; } }
        public bool ChrClasses { get { return charClasses; } }
        public bool Version    { get { return emitVer; } }
        public bool Summary    { get { return summary; } }
		public bool Listing    { get { return listing; } }
        public bool CheckOnly  { get { return checkOnly; } }
        public bool ParseOnly  { get { return parseOnly; } }
        public bool Errors     { get { return handler.Errors; } }
        public bool CompressMap  {
            // If useUnicode, we obey the compressMap Boolean.
            // If compressExplicit, we obey the compressMap Boolean.
            // Otherwise we return false.
            // 
            // The result of this is that the default for unicode
            // is to compress both map and next-state tables, while
            // for 8-bit scanners we compress next-state but not map.
            get {
                if (useUnicode || compressExplicit)
                    return compressMap;
                else
                    return false;
            } 
        }
        public bool CompressNext { get { return compressNext; } }
        public bool Minimize   { get { return minimize; } }
        public bool Warnings   { get { return handler.Warnings; } }
        public bool Unicode    { get { return useUnicode; } }

        public int    ErrNum     { get { return handler.ErrNum; } }
        public int    WrnNum     { get { return handler.WrnNum; } }
        public string VerString  { get { return version; } }
        public string FileName   { get { return fileName; } }
        public string FrameName        { get { return frameName; } }
        public TextWriter Msg    { get { return msgWrtr; } }

        public int HostSymCardinality { get { return hostSymCardinality; } }

        public int TargetSymCardinality { 
            get 
            {
                if (targetSymCardinality == notSet)
                    targetSymCardinality = asciiCardinality;
                return targetSymCardinality;
            }
        }

        public TextWriter ListStream
        {
            get
            {
                if (listWriter == null) 
                    listWriter = ListingFile(baseName + dotLst);
                return listWriter;
            }
        }

		// parse the command line options
		public OptionState ParseOption(string arg)
		{
            arg = arg.ToLower();
            if (arg.StartsWith("out:"))
            {
                outName = arg.Substring(4);
                if (outName.Equals("-"))
                    msgWrtr = Console.Error;
            }
            else if (arg.StartsWith("frame:"))
                userFrame = arg.Substring(6);
            else if (arg.Equals("help") || arg.Equals("?"))
            {
                    return OptionState.needUsage;
            }
            else
            {
                bool negate = arg.StartsWith("no");

                if (negate) arg = arg.Substring(2);
                if (arg.Equals("check")) checkOnly = !negate;
                else if (arg.Equals("listing")) listing = !negate;
                else if (arg.Equals("summary")) summary = !negate;
                else if (arg.Equals("trace")) trace = !negate;
                else if (arg.Equals("stack")) stack = !negate;
                else if (arg.Equals("minimize")) minimize = !negate;
                else if (arg.Equals("version")) emitVer = !negate;
                else if (arg.Equals("parseonly")) parseOnly = !negate;
                else if (arg.Equals("parser")) hasParser = !negate;
                else if (arg.Equals("babel")) babel = !negate;
                else if (arg.Equals("compressmap"))
                {
                    compressMap = !negate;
                    compressExplicit = true;
                }
                else if (arg.Equals("compressnext"))
                {
                    compressNext = !negate;
                    compressExplicit = true;
                }
                else if (arg.Equals("compress"))
                {
                    compressMap = !negate;
                    compressNext = !negate;
                    compressExplicit = true;
                }
                else if (arg.Equals("unicode"))
                {
                    // Have to do some checks here. If an attempt is made to
                    // set (no)unicode after the alphabet size has been set
                    // it is a command line or inline option error.
                    int cardinality = (negate ? asciiCardinality : unicodeCardinality);
                    useUnicode = !negate;
                    if (targetSymCardinality == notSet ||
                        targetSymCardinality == cardinality)
                        targetSymCardinality = cardinality;
                    else
                        return OptionState.alphabetLocked;
                    if (useUnicode)
                        charClasses = true;
                }
                else if (arg.Equals("verbose"))
                {
                    verbose = !negate;
                    if (verbose) emitVer = true;
                }
                else if (arg.Equals("classes"))
                {
                    if (negate && useUnicode)
                        return OptionState.inconsistent;
                    charClasses = !negate;
                }
                else
                    return OptionState.errors;
            }
            return OptionState.clear;
		}

		public void ErrorReport()
		{
            try { handler.DumpAll(scanner.buffer, msgWrtr); }
            catch { }
		}

        public void MakeListing()
        {
            // list could be null, if this is an un-requested listing
            // for example after errors have been detected.
            if (listWriter == null) 
                listWriter = ListingFile(baseName + dotLst);
            try { handler.MakeListing(scanner.buffer, listWriter, fileName, version); }
            catch { }
        }

        public static string ElapsedTime(DateTime start)
        {
            TimeSpan span = DateTime.Now - start;
            return String.Format("{0,4:D} msec", (int)span.TotalMilliseconds);
        }

        /// <summary>
        /// Set up file paths: called after options are processed
        /// </summary>
        /// <param name="path"></param>
        internal void GetNames(string path)
        {
            string xNam = Path.GetExtension(path).ToLower();
            string flnm = Path.GetFileName(path);

            string locn = System.Reflection.Assembly.GetExecutingAssembly().Location;
            this.exeDirNm = Path.GetDirectoryName(locn);

            this.dirName = Path.GetDirectoryName(path);
            this.pathName = path;

            if (xNam.Equals(dotLex))
                this.fileName = flnm;
            else if (xNam.Equals(""))
            {
                this.fileName = flnm + dotLex;
                this.pathName = path + dotLex;
            }
            else
                this.fileName = flnm;
            this.baseName = Path.GetFileNameWithoutExtension(this.fileName);

            if (this.outName == null) // do the default outfilename
                this.outName = this.baseName + dotCs;

        }

        /// <summary>
        /// This method opens the source file.  The file is not disposed in this file.
        /// The mainline code (program.cs) can call MakeListing and/or ErrorReport, for 
        /// which the buffered stream needs to be open so as to interleave error messages 
        /// with the source.
        /// </summary>
        internal void OpenSource()
        {
            try
            {
                inputFile = new FileStream(this.pathName, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (verbose) msgWrtr.WriteLine("GPLEX: opened input file <{0}>", pathName);
            }
            catch (IOException)
            {
                inputFile = null;
                handler = new ErrorHandler(); // To stop handler.ErrNum faulting!
                string message = String.Format("Source file <{0}> not found{1}", fileName, Environment.NewLine);
                handler.AddError(message, null); // aast.AtStart);
                throw new Exception(message);
            }
        }

        FileStream FrameFile()
        {
            FileStream frameFile;
            string path1 = null;
            string path2 = Path.Combine(this.exeDirNm, this.dfltFrame);
            if (this.userFrame != null)
            {
                path1 = this.userFrame;
                try
                {
                    // Try the user-specified path if there is one given.
                    frameFile = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (verbose) msgWrtr.WriteLine("GPLEX: opened frame file <{0}>", path1);
                    this.frameName = path1;
                    return frameFile;
                }
                catch (IOException)
                {
                }
            }
            else
            {
                path1 = this.dfltFrame;
                try
                {
                    // If there is no user-defined path, look for default in the working directory.
                    frameFile = new FileStream(path1, FileMode.Open, FileAccess.Read, FileShare.Read);
                    if (verbose) msgWrtr.WriteLine("GPLEX: opened frame file <{0}>", path1);
                    this.frameName = path1;
                    return frameFile;
                }
                catch (IOException)
                { } // No warning, just look in the usual place. 
            }
            try
            {
                frameFile = new FileStream(path2, FileMode.Open, FileAccess.Read, FileShare.Read);
                if (verbose) msgWrtr.WriteLine("GPLEX: opened frame file <{0}>", path2);
                if (this.userFrame != null)
                    // User specified a frame file, but is ending up with the default file ...
                    handler.AddWarning(
                        String.Format("GPLEX specified frame file <{0}> not found, {1}\t using default <{2}> instead",
                        path1, Environment.NewLine, path2),
                        aast.AtStart);
                this.frameName = path2;
                return frameFile;
            }
            catch (IOException)
            {
                // This time it is serious.  This is a fatal error.
                handler.AddError("GPLEX frame file <" + path1 + "> not found", aast.AtStart);
                handler.AddError("GPLEX frame file <" + path2 + "> not found", aast.AtStart);
                return null;
            }
        }

        StreamReader FrameReader()
        {
            return new StreamReader(FrameFile());
        }

        FileStream OutputFile()
        {
            FileStream rslt = null;
            try
            {
                rslt = new FileStream(this.outName, FileMode.Create);
                if (verbose) msgWrtr.WriteLine("GPLEX: opened output file <{0}>", this.outName);
            }
            catch (IOException)
            {
                handler.AddError("GPLEX: output file <" + this.outName + "> not opened", aast.AtStart);
            }
            return rslt;
        }

        TextWriter OutputWriter()
        {
            TextWriter rslt = null;
            if (this.outName.Equals("-"))
            {
                rslt = Console.Out;
                if (verbose) msgWrtr.WriteLine("GPLEX: output sent to StdOut");
            }
            else
                rslt = new StreamWriter(OutputFile());
            return rslt;
        }

        StreamWriter ListingFile(string outName)
        {
            try
            {
                FileStream listFile = new FileStream(outName, FileMode.Create);
                if (verbose) msgWrtr.WriteLine("GPLEX: opened listing file <{0}>", outName);
                return new StreamWriter(listFile);
            }
            catch (IOException)
            {
                handler.AddError("GPLEX: listing file <" + outName + "> not opened", aast.AtStart);
                return null;
            }
        }

        public void ListDivider()
        {
            ListStream.WriteLine(
            "============================================================================="); 
        }

        void Status(DateTime start)
        {
            msgWrtr.Write("GPLEX: input parsed, AST built");
            msgWrtr.Write((Errors ? ", errors detected" : " without error"));
            msgWrtr.Write((Warnings ? "; warnings issued. " : ". "));
            msgWrtr.WriteLine(ElapsedTime(start));
        }

        void ClassStatus(DateTime start, int len)
        {
            msgWrtr.Write("GPLEX: {0} character classes found.", len);
            msgWrtr.WriteLine(ElapsedTime(start));
        }

        void CheckOptions()
        {
            if (Babel && !Unicode)
                handler.ListError(aast.AtStart, 112);
        }

        public void Process(string fileArg)
		{
            GetNames(fileArg);
            // check for file exists
            OpenSource();
            // parse source file
            if (inputFile != null)
            {
                DateTime start = DateTime.Now;
                try
                {
                    handler = new ErrorHandler();
                    scanner = new GPLEX.Lexer.Scanner(inputFile);
                    parser = new GPLEX.Parser.Parser();
                    scanner.yyhdlr = handler;
                    parser.Initialize(this, scanner, handler, new OptionParser2(ParseOption));
                    aast = parser.Aast;
                    parser.Parse();
                    // aast.DiagnosticDump();
                    if (verbose) 
                        Status(start);
                    CheckOptions();
                    if (!Errors && !ParseOnly)
                    {	// build NFSA
                        if (ChrClasses)
                        {
                            DateTime t0 = DateTime.Now;
                            partition = new Partition(TargetSymCardinality);
                            partition.FindClasses(aast);
                            partition.FixMap();
                            if (verbose)
                                ClassStatus(t0, partition.Length);
                        }
                        nfsa = new NFSA(this);
                        nfsa.Build(aast);
                        if (!Errors)
                        {	// convert to DFSA
                            dfsa = new DFSA(this);
                            dfsa.Convert(nfsa);
                            if (!Errors)
                            {	// minimize automaton
                                dfsa.Minimize();
                                if (!Errors && !checkOnly)
                                {   // emit the scanner to output file
                                    StreamReader frameRdr = FrameReader();
                                    TextWriter outputWrtr = OutputWriter();
                                    dfsa.EmitScanner(frameRdr, outputWrtr);
                                    if (frameRdr != null) 
                                        frameRdr.Close();
                                    if (outputWrtr != null) 
                                        outputWrtr.Close();
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string str = ex.Message;
                    handler.AddError(str, aast.AtStart);
                    throw ex;
                }
            }
		}

        public void Cleanup()
        {
            if (inputFile != null) 
                inputFile.Close();
            if (listWriter != null)
                listWriter.Close();
        }
	}
}

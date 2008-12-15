// Gardens Point Parser Generator
// Copyright (c) Wayne Kelly, QUT 2005-2007
// (see accompanying GPPGcopyright.rtf)


using System;
using System.Reflection;
using System.Collections.Generic;

[assembly:AssemblyVersionAttribute("1.2.0.115")]
[assembly:AssemblyInformationalVersionAttribute("1.2.0.115 (2007-11-7)")]
namespace gpcc
{
	class GPCG
	{
        public static bool BABEL = false;
		public static bool LINES = true;
		public static bool REPORT = false;
        public static bool DEFINES = false;
        public static bool FORGPLEX = false;
        public static bool VERBOSE = false;
        public static bool CONFLICTS = false;
        public static string versionInfo = null;

		private static void Main(string[] args)
		{
			try
			{
				string filename = ProcessOptions(args);

				if (filename == null)
					return;

                Assembly assm = Assembly.GetExecutingAssembly();
                object info = Attribute.GetCustomAttribute(assm, typeof(AssemblyInformationalVersionAttribute));
                versionInfo = ((AssemblyInformationalVersionAttribute)info).InformationalVersion;

				Parser parser = new Parser();
				Grammar grammar = parser.Parse(filename);

				LALRGenerator generator = new LALRGenerator(grammar);
				List<State> states = generator.BuildStates();
				generator.ComputeLookAhead();
				generator.BuildParseTable();
                if (!grammar.CheckGrammar())
                    throw new Exception("Non-terminating grammar");

				if (REPORT)
					generator.Report();
				else
				{
					CodeGenerator code = new CodeGenerator();
					code.Generate(states, grammar);
				}
			}
			catch (Scanner.ParseException e)
			{
				Console.Error.WriteLine("Parse error (line {0}, column {1}): {2}", e.line, e.column, e.Message);
			}
			catch (System.Exception e)
			{
				Console.Error.WriteLine("Unexpected Error {0}", e.Message);
			}
		}


		private static string ProcessOptions(string[] args)
		{
			string filename = null;

			foreach (string arg in args)
			{
				if (arg[0] == '-' || arg[0] == '/')
					switch (arg.Substring(1))
					{
						case "?":
						case "h":
						case "help":
							DisplayHelp();
							return null;
						case "v":
						case "version":
							DisplayVersion();
							return null;
						case "l":
						case "no-lines":
							LINES = false;
							break;
						case "r":
						case "report":
							REPORT = true;
							break;
                        case "d":
                        case "defines":
                            DEFINES = true;
                            break;
                        case "gplex":
                            FORGPLEX = true;
                            break;
                        case "verbose":
                            VERBOSE = true;
                            break;
                        case "babel":
                            BABEL = true;
                            FORGPLEX = true;
                            break;
                        case "conflicts":
                            CONFLICTS = true;
                            VERBOSE = true;
                            break;
                    }
				else
					filename = arg;
			}

			if (filename == null)
				DisplayHelp();

			return filename;
		}


		private static void DisplayHelp()
		{
			Console.WriteLine("Usage gppg [options] filename");
			Console.WriteLine();
			Console.WriteLine("/help:       Display this help message");
			Console.WriteLine("/version:    Display version information");
			Console.WriteLine("/report:     Emit LALR(1) parsing states");
			Console.WriteLine("/no-lines:   Suppress the generation of #line directives");
            Console.WriteLine("/defines:    Emit \"tokens\" file with token name list");
            Console.WriteLine("/gplex:      Generate scanner base class for GPLEX");
            Console.WriteLine("/babel:      Generate class compatible with Managed Babel");
            Console.WriteLine("/verbose:    Display additional information on parser conflicts");
            Console.WriteLine("/conflicts:  Emit \"conflicts\" file with full conflict details");
			Console.WriteLine();
		}


		private static void DisplayVersion()
		{
            Assembly assm = Assembly.GetExecutingAssembly();
            object info = Attribute.GetCustomAttribute(assm, typeof(AssemblyInformationalVersionAttribute));
            versionInfo = ((AssemblyInformationalVersionAttribute)info).InformationalVersion;

            Console.WriteLine("Gardens Point Parser Generator (gppg) " + versionInfo);
            Console.WriteLine("Copyright (c) 2005-2007 Wayne Kelly, QUT");
            Console.WriteLine("w.kelly@qut.ed.au");
            Console.WriteLine("Queensland University of Technology");
            Console.WriteLine();
		}
	}
}

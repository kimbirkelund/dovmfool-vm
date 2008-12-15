// Gardens Point Scanner Generator
// Copyright (c) K John Gough, QUT 2006-2007
// (see accompanying GPLEXcopyright.rtf)

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using GPLEX.Automaton;
using GPLEX.Parser;

[assembly: AssemblyVersionAttribute("0.6.2.196")]
[assembly: AssemblyInformationalVersionAttribute("0.6.2.196 (2007-11-13)")]
namespace GPLEX
{
	class Program
	{
		const string prefix = "GPLEX: ";

		static void Main(string[] args)
		{
            bool fileArg = false;
			TaskState task = new TaskState();
            OptionState opResult = OptionState.clear;
			if (args.Length == 0)
				Usage("No arguments");
			for (int i = 0; i < args.Length; i++)
			{
                if (args[i][0] == '/' || args[i][0] == '-')
                {
                    string arg = args[i].Substring(1);
                    opResult = task.ParseOption(arg);
                    if (opResult != OptionState.clear &&
                        opResult != OptionState.needUsage)
                        BadOption(arg, opResult);
                }
                else if (i != args.Length - 1)
                    Usage("Too many arguments");
                else
                    fileArg = true;
			}
            if (task.Version)
                task.Msg.WriteLine("GPLEX version: " + task.VerString);
            if (opResult == OptionState.errors)
                Usage(null); // print usage and abort
            else if (!fileArg)
                Usage("No filename");
            else if (opResult == OptionState.needUsage)
                Usage();     // print usage but do not abort
            try
            {
                task.Process(args[args.Length - 1]);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
            if (task.ErrNum + task.WrnNum > 0 || task.Listing)
                task.MakeListing();
            if (task.ErrNum + task.WrnNum > 0)
                task.ErrorReport();
            else if (task.Verbose)
                task.Msg.WriteLine("GPLEX <" + task.FileName + "> Completed successfully");
            task.Cleanup();
            if (task.ErrNum > 0)
                Environment.Exit(1);
            else
                Environment.Exit(0);
		}

		static void BadOption(string arg, OptionState rslt)
		{
            string marker = "";
            switch (rslt)
            {
                case OptionState.errors:
                    marker = "unknown argument";
                    break;
                case OptionState.inconsistent:
                    marker = "inconsistent argument";
                    break;
                case OptionState.alphabetLocked:
                    marker = "can't change alphabet";
                    break;
            }
            Console.Error.WriteLine("{0} {1}: {2}", prefix, marker, arg);
		}

		static void Usage() // print the usage message but do not abort.
		{
			Console.WriteLine(prefix + "Usage");
			Console.WriteLine("  gplex [options] filename");
			Console.WriteLine("  default filename extension is \".lex\"");
  			Console.WriteLine("  options:  /babel          -- create extra interface for Managed Babel scanner");
            Console.WriteLine("            /check          -- create automaton but do not create output file");
            Console.WriteLine("            /classes        -- use character equivalence classes");
            Console.WriteLine("            /frame:path     -- use \"path\" as frame file"); 
			Console.WriteLine("            /help           -- display this usage message");
            Console.WriteLine("            /listing        -- emit listing even if no errors");
            Console.WriteLine("            /nocompress     -- do not compress scanner tables");
            Console.WriteLine("            /nocompressmap  -- do not compress classes map");
            Console.WriteLine("            /nocompressnext -- do not compress nextstate tables");
            Console.WriteLine("            /nominimize     -- do not minimize the states of the dfsa");
            Console.WriteLine("            /noparser       -- create stand-alone scanner");
            Console.WriteLine("            /out:path       -- send output to filename \"path\"");
            Console.WriteLine("            /out:-          -- send output to Console.Out");
            Console.WriteLine("            /parseonly      -- syntax check only, do not create automaton");
            Console.WriteLine("            /stack          -- enable built-in stacking of start states");
            Console.WriteLine("            /babel          -- create colorizing scanner for Visual Studio");
            Console.WriteLine("            /summary        -- emit statistics to list file");
            Console.WriteLine("            /unicode        -- generate a unicode enabled scanner");
            Console.WriteLine("            /verbose        -- chatter on about progress");
            Console.WriteLine("            /version        -- give version information for GPLEX");
        }

		static void Usage(string msg)  // print the usage message and die.
		{
			if (msg != null)
				Console.WriteLine(prefix + msg);
			Usage();
			Console.WriteLine("  Terminating ...");
			Environment.Exit(1);
		}
	}
}

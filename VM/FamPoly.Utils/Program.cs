using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;

namespace FamPoly.Utils {
    class Program {
        enum Switch { GPLex, GPPG }

        static int Main( string[] args ) {
            var argSwitch = new SelectArgument( "Mode", typeof( Switch ) ) { IsRequired = true, Position = 0 };
            var argInputFile = new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 1 };
            var argOutputFile = new PathArgument( "OutputFile", false, true ) { IsRequired = true, Position = 2 };
            try {
                string switchArg = args[0];
                var argsMan = new ArgumentsManager( new IArgument[] { argSwitch, argInputFile, argOutputFile } );
                argsMan.Parse( args );
            } catch (ArgumentMatchException exp) {
                exp.Print( Console.Out );
                Console.WriteLine();
                return 1;
            }

            switch ((Switch) argSwitch.Value) {
                case Switch.GPLex: return GPLex.Execute( argInputFile.Value, argOutputFile.Value );
                case Switch.GPPG: return GPPG.Execute( argInputFile.Value, argOutputFile.Value );
                default:
                    PrintUsage();
                    return 1;
            }
        }

        private static void PrintUsage() {
            Console.WriteLine( "Usage fputils [gplex|gppg] arguments" );
        }
    }
}

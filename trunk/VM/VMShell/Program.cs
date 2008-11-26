using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;

namespace VMShell {
	class Program {
		static void Main( string[] args ) {
			var argsMan = new ArgumentsManager();
			var inputFileArg = argsMan.AddArgument( new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 0, Description = "The input file to execute." } );
			argsMan.Parse( args );

			System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.ConsoleTraceListener() );

			VM.VirtualMachine.Execute( inputFileArg.Value );
		}
	}
}

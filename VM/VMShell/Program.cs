using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;
using System.Threading;

namespace VMShell {
	class Program {
		static bool isDebug;
		static void Main( string[] args ) {
			var argsMan = new ArgumentsManager();
			var inputFileArg = argsMan.AddArgument( new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 0, Description = "The input file to execute." } );
			argsMan.Parse( args );

			System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.ConsoleTraceListener() );

			try {
				//var to = new Timer( new TimerCallback( Swapper ), null, TimeSpan.FromSeconds( 5 ), TimeSpan.FromSeconds( 5 ) );

				var ret = VM.VirtualMachine.Execute( inputFileArg.Value );
				if (ret != null)
					Console.WriteLine( ret );
			} catch (Exception e) {
				if (e.InnerException != null)
					Console.WriteLine( e.InnerException );
				else
					Console.WriteLine( e );
			}
		}

		static void Swapper( object obj ) {
			if (isDebug)
				VM.VirtualMachine.DebuggerDetached();
			else
				VM.VirtualMachine.DebuggerAttached();
			isDebug = !isDebug;
		}
	}
}

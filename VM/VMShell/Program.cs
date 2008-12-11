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
			var swapperArg = argsMan.AddArgument( new FlagArgument( "Swapper" ) { IsRequired = false, ArgumentGroup = 1, Description = "Enables the swapper test." } );
			var pauserArg = argsMan.AddArgument( new FlagArgument( "Pauser" ) { IsRequired = false, ArgumentGroup = 1, Description = "Enables the pauser test." } );
			argsMan.Parse( args );

			System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.ConsoleTraceListener() );

			//try {
				Thread thread = null;
				if (swapperArg.Value)
					thread = new Thread( Swapper );
				else if (pauserArg.Value)
					thread = new Thread( Pauser );

				VM.VirtualMachine.BeginExecuting( inputFileArg.Value );
				if (thread != null) {
					thread.IsBackground = true;
					thread.Start();
				}
				var ret = VM.VirtualMachine.EndExecuting();
				if (ret != null)
					Console.WriteLine( ret );
			//} catch (Exception e) {
			//    if (e.InnerException != null)
			//        Console.WriteLine( e.InnerException );
			//    else
			//        Console.WriteLine( e );
			//}
		}

		static void Swapper() {
			while (true) {
				Thread.Sleep( 2000 );
				if (isDebug) {
					Console.Write( "Continue" );
					VM.Debugging.Service.Server.DebuggerService.GetInterpretors().ForEach( i => VM.Debugging.Service.Server.DebuggerService.Continue( i ) );
					VM.VirtualMachine.DebuggerDetached();
				} else {
					VM.VirtualMachine.DebuggerAttached();
					Console.Write( "Break" );
					VM.Debugging.Service.Server.DebuggerService.GetInterpretors().ForEach( i => VM.Debugging.Service.Server.DebuggerService.Break( i ) );
					Thread.Sleep( 2000 );
					Console.Write( "Step" );
					for (int i = 0; i < 20; i++) {
						VM.Debugging.Service.Server.DebuggerService.GetInterpretors().ForEach( j => VM.Debugging.Service.Server.DebuggerService.StepOne( j ) );
					}
				}

				isDebug = !isDebug;
			}
		}

		static void Pauser() {
			while (true) {
				Console.Write( "Sleeping" );
				Thread.Sleep( 2000 );
				Console.Write( "BeginPausing" );
				VM.VirtualMachine.GetInterpretors().ForEach( i => i.BeginPause() );
				Console.Write( "EndPausing" );
				VM.VirtualMachine.GetInterpretors().ForEach( i => i.EndPause() );
				Console.Write( "Sleeping" );
				Thread.Sleep( 2000 );
				Console.Write( "Resuming" );
				VM.VirtualMachine.GetInterpretors().ForEach( i => i.Resume() );
			}
		}
	}
}

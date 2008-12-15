using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;
using System.Threading;

namespace VMShell {
	class Program {
		static bool isDebug;
		static int Main( string[] args ) {
			var argsMan = new ArgumentsManager();
			var inputFileArg = argsMan.AddArgument( new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 0, Description = "The input file to execute." } );
			var swapperArg = argsMan.AddArgument( new FlagArgument( "Swapper" ) { IsRequired = false, ArgumentGroup = 1, Description = "Enables the swapper test." } );
			var pauserArg = argsMan.AddArgument( new FlagArgument( "Pauser" ) { IsRequired = false, ArgumentGroup = 2, Description = "Enables the pauser test." } );

			try {
				argsMan.Parse( args );
			} catch (Exception ex) {
				Console.WriteLine( ex.Message );
				Console.WriteLine();
				argsMan.PrintUsage( "VMShell.exe", Console.WindowWidth, Console.Out );
				return -1;
			}
			System.Diagnostics.Trace.Listeners.Add( new System.Diagnostics.ConsoleTraceListener() );

			try {
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
			} catch (Exception e) {
				if (e.InnerException != null)
					Console.WriteLine( e.InnerException );
				else
					Console.WriteLine( e );
				return -2;
			}
			return 1;
		}

		static void Swapper() {
			while (true) {
				Thread.Sleep( 5000 );
				if (isDebug) {
					Console.WriteLine( "\n\n---------------------Continue---------------------\n\n" );
					VM.Debugging.Service.Server.DebuggerService.GetInterpreters().ForEach( i => VM.Debugging.Service.Server.DebuggerService.Continue( i ) );
					VM.VirtualMachine.DebuggerDetached();
				} else {
					VM.VirtualMachine.DebuggerAttached();
					Console.WriteLine( "\n\n---------------------Break---------------------\n\n" );
					VM.Debugging.Service.Server.DebuggerService.GetInterpreters().ForEach( i => VM.Debugging.Service.Server.DebuggerService.Break( i ) );
					Thread.Sleep( 5000 );
					Console.WriteLine( "\n\n---------------------Step---------------------\n\n" );
					for (int i = 0; i < 20; i++)
						VM.Debugging.Service.Server.DebuggerService.GetInterpreters().ForEach( j => VM.Debugging.Service.Server.DebuggerService.StepOne( j ) );
				}

				isDebug = !isDebug;
			}
		}

		static void Pauser() {
			while (true) {
				Console.Write( "Sleeping" );
				Thread.Sleep( 2000 );
				Console.Write( "BeginPausing" );
				VM.VirtualMachine.GetInterpreters().ForEach( i => i.BeginPause() );
				Console.Write( "EndPausing" );
				VM.VirtualMachine.GetInterpreters().ForEach( i => i.EndPause() );
				Console.Write( "Sleeping" );
				Thread.Sleep( 2000 );
				Console.Write( "Resuming" );
				VM.VirtualMachine.GetInterpreters().ForEach( i => i.Resume() );
			}
		}
	}
}

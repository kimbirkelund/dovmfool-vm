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
			var initialHeapSizeArg = argsMan.AddArgument( new IntegerArgument( "InitialHeapSize", 1 ) { IsRequired = false, Description = "Specifies the initial size of the heap." } );
			var maxHeapSizeArg = argsMan.AddArgument( new IntegerArgument( "MaxHeapSize", 1 ) { IsRequired = false, Description = "Specifies the maximum size the heap can grow to." } );
			var heapGrowFactorArg = argsMan.AddArgument( new IntegerArgument( "HeapGrowFactor", 2 ) { IsRequired = false, Description = "Specifies the factor by which the heap is grown." } );
			var disableGCArg = argsMan.AddArgument( new FlagArgument( "DisableGarbageCollection" ) { IsRequired = false, Description = "Specifying this argument disables garbage collection." } );
			var logArg = argsMan.AddArgument( new IntegerArgument( "LogLevel", 0, 2 ) { IsRequired = false, Description = "Enables logging at the specified level. Default is no logging." } );

			try {
				argsMan.Parse( args );
			} catch (Exception ex) {
				Console.WriteLine( ex.Message );
				Console.WriteLine();
				argsMan.PrintUsage( "VMShell.exe", Console.WindowWidth, Console.Out );
				return -1;
			}

			var initialHeapSize = 10000;
			if (initialHeapSizeArg.IsPresent)
				initialHeapSize = initialHeapSizeArg.Value;
			var maxHeapSize = 100000000;
			if (maxHeapSizeArg.IsPresent)
				maxHeapSize = maxHeapSizeArg.Value;
			var heapGrowFactor = 2;
			if (heapGrowFactorArg.IsPresent)
				heapGrowFactor = heapGrowFactorArg.Value;
			var useGC = !disableGCArg;

#if RELEASE
			try {
#endif
			Thread thread = null;
			if (swapperArg.Value)
				thread = new Thread( Swapper );
			else if (pauserArg.Value)
				thread = new Thread( Pauser );

			if (logArg.IsPresent && logArg.Value > 0) {
				var h = new Sekhmet.Logging.ConsoleLogHandler();
				VM.VirtualMachine.Logger.Handlers.Add( h );
				if (logArg.Value == 1)
					h.IgnoreCategories.Add( "MEMAlloc" );
			}
			VM.VirtualMachine.BeginExecuting( inputFileArg.Value, useGC, initialHeapSize, maxHeapSize, heapGrowFactor );
			if (thread != null) {
				thread.IsBackground = true;
				thread.Start();
			}
			var ret = VM.VirtualMachine.EndExecuting();
			if (ret != null)
				Console.WriteLine( ret );
#if RELEASE
			} catch (Exception e) {
					Console.WriteLine( e );
				return -2;
			}
#endif
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

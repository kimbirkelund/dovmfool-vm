using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;
using Sekhmet.Logging;

namespace VMILAssembler {
	public class Program {
		public static int Main( string[] args ) {
			var argsMan = new ArgumentsManager();
			var inputFileArg = argsMan.AddArgument( new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 0, Description = "The input file." } );
			var outputFileArg = argsMan.AddArgument( new PathArgument( "OutputFile", false, true ) { IsRequired = false, Position = 1, Description = "The output file." } );

			try {
				argsMan.Parse( args );
			} catch (ArgumentMatchException ex) {
				var color = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				ex.Print( Console.Out );
				Console.ForegroundColor = color;
				Console.WriteLine();
				argsMan.PrintSyntax( "vmilasm.exe", Console.WindowWidth, Console.Out );
				return 1;
			}

			var inputFile = inputFileArg.Value;
			var outputFile = outputFileArg.Value;
			if (!outputFileArg.IsPresent)
				outputFile = inputFileArg.Value + ".new";

			var logger = new Logger();
			logger.Handlers.Add( new ConsoleLogHandler() );

			using (var reader = new VMILLib.SourceReader( inputFile )) {
				reader.Logger = logger;
				using (var writer = new VMILLib.SourceWriter( outputFile )) {

					//try {
					writer.Write( reader.Read() );
					//} catch (ArgumentException ex) {
					//    Console.WriteLine( ex.Message );
					//    return 1;
					//}
				}
			}

			return 0;
		}
	}
}

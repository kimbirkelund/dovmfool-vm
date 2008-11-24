using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;
using Sekhmet.Logging;
using System.IO;

namespace VMILAssembler {
	public class Program {
		public static int Main( string[] args ) {
			var argsMan = new ArgumentsManager();
			var inputFileArg = argsMan.AddArgument( new PathArgument( "InputFile", true, true ) { IsRequired = true, Position = 0, Description = "The input file." } );
			var outputFileArg = argsMan.AddArgument( new PathArgument( "OutputFile", false, true ) { IsRequired = false, Position = 1, Description = "The output file." } );
			var outputAsSourceArg = argsMan.AddArgument( new FlagArgument( "OutputAsSource" ) { IsRequired = false, Position = 2, Description = "Makes the assembler output to a new vmil source file instead of a binary executable." } );

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
				outputFile = Path.ChangeExtension( inputFileArg.Value, outputAsSourceArg.Value ? ".new.vmil" : ".vmb" );

			var logger = new Logger();
			logger.Handlers.Add( new ConsoleLogHandler() );

			try {
				VMILLib.Assembly program;
				using (var reader = new VMILLib.SourceReader( inputFile )) {
					reader.Logger = logger;
					program = reader.Read();
				}

				if (outputAsSourceArg.Value)
					using (var writer = new VMILLib.SourceWriter( outputFile ))
						writer.Write( program );
				else
					using (var writer = new VMILLib.BinaryWriter( outputFile ))
						writer.Write( program );
			} catch (Exception e) {
				Console.WriteLine( e.Message );
				return -1;
			}

			return 0;
		}
	}
}

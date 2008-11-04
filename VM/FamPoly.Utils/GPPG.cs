using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using FamPoly.Utils.Properties;
using System.IO;
using Sekhmet.CommandLineArguments;

namespace FamPoly.Utils {
    static class GPPG {
        public static int Execute( string inputFile, string outputFile ) {
            if (!File.Exists( inputFile ))
                return 1;
            if (File.Exists( outputFile ) && File.GetLastWriteTime( outputFile ) >= File.GetLastWriteTime( inputFile ))
                return 0;

            var psi = new ProcessStartInfo( Settings.Default.GPPGPath, "/gplex " + inputFile ) {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true
            };

            var p = Process.Start( psi );

            string output = p.StandardOutput.ReadToEnd();

            if (p.ExitCode != 0) {
                Console.Write( output );
                return 1;
            }

            output = MakeInternal.Execute( output );

            using (var writer = new StreamWriter( outputFile ))
                writer.Write( output );

            return 0;
        }
    }
}

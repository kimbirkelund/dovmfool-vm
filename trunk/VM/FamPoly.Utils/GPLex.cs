using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.CommandLineArguments;
using System.Diagnostics;
using FamPoly.Utils.Properties;
using System.IO;

namespace FamPoly.Utils {
    static class GPLex {
        public static int Execute( string inputFile, string outputFile ) {
            if (!File.Exists( inputFile ))
                return 1;
            if (File.Exists( outputFile ) && File.GetLastWriteTime( outputFile ) >= File.GetLastWriteTime( inputFile ))
                return 0;

            var psi = new ProcessStartInfo( Settings.Default.GPLexPath, "/out:" + outputFile + " " + inputFile ) { UseShellExecute = false };
            var p = Process.Start( psi );
            p.WaitForExit();
            if (p.ExitCode != 0)
                return 1;

            string result = null;
            using (var reader = new StreamReader( outputFile ))
                result = reader.ReadToEnd();

            result = MakeInternal.Execute( result );
            using (var writer = new StreamWriter( outputFile ))
                writer.Write( result );

            return 0;
        }
    }
}

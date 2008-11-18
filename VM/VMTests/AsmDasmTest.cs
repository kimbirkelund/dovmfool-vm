using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using VMILLib;
using VMTests.Properties;
using Sekhmet.Logging;

namespace VMTests {
	/// <summary>
	/// Summary description for AsmDasmTest
	/// </summary>
	[TestClass]
	public class AsmDasmTest {
		string input;

		public AsmDasmTest() {
			input = Encoding.ASCII.GetString( Resources.AsmDasmTestFile );
		}

		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestAsmDasm() {
			var sourceInputStream = new MemoryStream( Encoding.ASCII.GetBytes( input ) );
			var binaryStream = new MemoryStream();
			var sourceOutputStream = new MemoryStream();

			var logger = new Logger();
			logger.Handlers.Add( new ConsoleLogHandler() );

			sourceInputStream.Position = 0;
			var sourceReader = new VMILLib.SourceReader( sourceInputStream ) { Logger = logger };
			var binaryWriter = new VMILLib.BinaryWriter( binaryStream );
			var binaryReader = new VMILLib.BinaryReader( binaryStream );
			var sourceWriter = new VMILLib.SourceWriter( sourceOutputStream );

			binaryWriter.Write( sourceReader.Read() );
			binaryStream.Position = 0;
			sourceWriter.Write( binaryReader.Read() );
			sourceWriter.Dispose();

			var expected = input;
			new[] { " ", "\t", "\r", "\n" }.ForEach( s => expected = expected.Replace( s, "" ) );
			var actual = Encoding.ASCII.GetString( sourceOutputStream.ToArray() );
			new[] { " ", "\t", "\r", "\n" }.ForEach( s => actual = actual.Replace( s, "" ) );

			if (expected.Length != actual.Length) {
				TestContext.WriteLine( "expected:[" + expected.Replace( "{", "{{" ).Replace( "}", "}}" ) + "]" );
				TestContext.WriteLine( "actual:[" + actual.Replace( "{", "{{" ).Replace( "}", "}}" ) + "]" );
				Assert.Fail( "Different length." );
			} else {
				for (int i = 0; i < expected.Length; i++) {
					if (expected[i] == actual[i])
						continue;

					TestContext.WriteLine( "expected:[" + expected.Substring( Math.Max( 0, i - 20 ), i ) + "]" );
					TestContext.WriteLine( "actual:[" + actual.Substring( Math.Max( 0, i - 20 ), i ) + "]" );
					Assert.Fail( "Different length." );
				}
			}
		}
	}
}

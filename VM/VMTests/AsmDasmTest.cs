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

			var logger = new Logger();
			logger.Handlers.Add( new ConsoleLogHandler() );

			using (var sourceReader = new VMILLib.SourceReader( sourceInputStream ) { Logger = logger })
				sourceReader.Read();
		}
	}
}

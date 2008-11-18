using VMILLib;
using System.Linq;
using u = Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Sekhmet;

namespace VMTests {
	[TestClass()]
	public class ExtensionsTest {
		public TestContext TestContext { get; set; }

		[TestMethod()]
		public void ToUIntStreamTest() {
			byte[] arr = new byte[] { 0x80, 0x80, 0x80, 0x80, 0x7B, 0x36, 0x16, 0x09 };
			var expected = new uint[] { 0x80808080, 0x7B361609 }.ToArray();
			var actual = arr.ToUIntStream().ToArray();
			TestContext.WriteLine( string.Join( ", ", expected.Select( i => i.ToString() ).ToArray() ) );
			TestContext.WriteLine( string.Join( ", ", actual.Select( i => i.ToString() ).ToArray() ) );

			u.Assert.AreEqual( expected.Length, actual.Length );
			for (var i = 0; i < expected.Length; i++)
				u.Assert.AreEqual( expected[i], actual[i] );
		}

		[TestMethod()]
		public void ToByteStreamTest() {
			uint[] arr = new uint[] { 0x80808080, 0x7B361609 };
			var expected = new byte[] { 0x80, 0x80, 0x80, 0x80, 0x7B, 0x36, 0x16, 0x09 }.ToArray();
			var actual = arr.ToByteStream().ToArray();

			u.Assert.AreEqual( expected.Length, actual.Length );
			for (var i = 0; i < expected.Length; i++)
				u.Assert.AreEqual( expected[i], actual[i] );
		}

		[TestMethod()]
		public void ToByteStreamTest1() {
			var arr = new int[] { 380927398, -2342342, 13232, -10 };
			var expected = new byte[] { 0x16, 0xB4, 0x7D, 0xA6, 0xFF, 0xDC, 0x42, 0x3A, 0x00, 0x00, 0x33, 0xB0, 0xFF, 0xFF, 0xFF, 0xF6 };
			var actual = arr.ToByteStream().ToArray();

			u.Assert.AreEqual( expected.Length, actual.Length );
			for (var i = 0; i < expected.Length; i++)
				u.Assert.AreEqual( expected[i], actual[i] );
		}

		[TestMethod()]
		public void ToIntStreamTest() {
			var arr = new byte[] { 0x16, 0xB4, 0x7D, 0xA6, 0xFF, 0xDC, 0x42, 0x3A, 0x00, 0x00, 0x33, 0xB0, 0xFF, 0xFF, 0xFF, 0xF6 };
			var expected = new int[] { 380927398, -2342342, 13232, -10 };
			var actual = arr.ToIntStream().ToArray();

			u.Assert.AreEqual( expected.Length, actual.Length );
			for (var i = 0; i < expected.Length; i++) {
				TestContext.WriteLine( expected[i] + " - " + actual[i] );
				u.Assert.AreEqual( expected[i], actual[i] );
			}
		}
	}
}

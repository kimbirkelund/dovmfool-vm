using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VMTests {
	[TestClass]
	public class AlgTest {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void TestMethod1() {
			var rnd = new Random();

			for (int i = 0; i < 1000000; i++) {
				var x = rnd.Next( int.MaxValue / 3 );
				var y = rnd.Next( int.MaxValue / 3 );
				if (y > x) {
					var t = y;
					y = x;
					x = y;
				}
				var expected = x / y + (x % y != 0 ? 1 : 0);
				var actual = (x + (y - 1)) / y;

				if (expected != actual)
					System.Diagnostics.Debugger.Break();
				Assert.AreEqual( expected, actual );
			}
		}
	}
}

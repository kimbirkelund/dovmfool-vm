using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VMTests {
	[TestClass]
	public class VMObjectsArrayTests {
		public TestContext TestContext { get; set; }

		[TestMethod]
		public void AllNotReferences() {
			_this[0] = _this[1] = 0;
			for (var i = 0; i < 64; i++)
				Assert.IsFalse( IsReference( i ) );
		}

		[TestMethod]
		public void AllReferences() {
			_this[0] = _this[1] = uint.MaxValue;
			for (var i = 0; i < 64; i++)
				Assert.IsTrue( IsReference( i ) );
		}

		[TestMethod]
		public void SetAllReferences() {
			_this[0] = _this[1] = 0;
			for (var i = 0; i < 64; i++)
				SetReference( i, true );
			for (var i = 0; i < 64; i++)
				Assert.IsTrue( IsReference( i ) );
		}

		[TestMethod]
		public void SetAllSetNoneReferences() {
			_this[0] = _this[1] = 0;
			for (var i = 0; i < 64; i++)
				SetReference( i, true );
			for (var i = 0; i < 64; i++)
				SetReference( i, false );
			for (var i = 0; i < 64; i++)
				Assert.IsFalse( IsReference( i ) );
		}

		[TestMethod]
		public void SetRandom() {
			_this[0] = _this[1] = 0;

			var rnd = new Random();
			var list = new List<int>();

			for (int i = 0; i < 20; i++) {
				var r = rnd.Next( 64 );
				SetReference( r, true );
				list.Add( r );
			}

			for (int i = 0; i < 64; i++)
				Assert.AreEqual( list.Contains( i ), IsReference( i ) );
		}

		uint[] _this = new uint[2];
		bool IsReference( int arrayIndex ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;
			return (_this[word] & (1 << bit)) != 0;
		}

		void SetReference( int arrayIndex, bool isReference ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;

			if (isReference)
				_this[word] = _this[word] | (1u << bit);
			else
				_this[word] = _this[word] ^ (1u << bit);
		}

	}
}

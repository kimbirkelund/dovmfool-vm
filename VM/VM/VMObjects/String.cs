using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.VMObjects {
	public struct String : IVMObject {
		public const int STRING_LENGTH_OFFSET = 0;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( String cls ) {
			return cls.start;
		}

		public static implicit operator String( int cls ) {
			return new String { start = cls };
		}

		public static implicit operator AppObject( String s ) {
			return (int) s;
		}

		public static explicit operator String( AppObject obj ) {
			return (int) obj;
		}
	}

	public static class ExtString {
		static readonly Word[] byteMasks = new Word[] { 0xFF000000, 0x00FF0000, 0x0000FF00, 0x000000FF };
		static readonly int[] byteRShifts = new int[] { 6, 4, 2, 0 };

		public static int Length( this String str ) {
			return str.Get( String.STRING_LENGTH_OFFSET );
		}

		public static char ChatAt( this String str, int pos ) {
			int word = pos / 2;
			int firstByte = (pos % 2) * 2;
			var bytes = new byte[] { 
				(byte) ((str.Get( word ) & byteMasks[firstByte]) >> byteRShifts[firstByte]), 
				(byte) ((str.Get( word ) & byteMasks[firstByte + 1]) >> byteRShifts[firstByte + 1]) };

			return Encoding.Unicode.GetChars( bytes )[0];
		}

		public static bool EqualTo( this String string1, String string2 ) {
			if (string1 == string2)
				return true;
			var s1Len = string1.Length();
			if (s1Len != string2.Length())
				return false;

			var s1WC = s1Len / 2 + s1Len % 2;

			for (var i = 1; i < s1WC; i++)
				if (string1.Get( i ) != string2.Get( i ))
					return false;

			return true;
		}

		public static int Compare( this String string1, String string2 ) {
			if (string1 == string2)
				return 0;

			var s1Len = string1.Length();
			var s1WC = s1Len / 2 + s1Len % 2;
			var s2Len = string2.Length();
			var s2WC = s2Len / 2 + s2Len % 2;

			for (var i = 1; i < Math.Min( s1WC, s2WC ) + 1; i++) {
				if (string1.Get( i ) == string2.Get( i ))
					continue;

				var s1FC = string1.ChatAt( (i - 1) * 2 );
				var s2FC = string2.ChatAt( (i - 1) * 2 );

				if (s1FC == s2FC) {
					if (s1WC == i)
						return -1;
					else if (s2WC == i)
						return 1;
					else
						return string2.ChatAt( (i - 1) * 2 + 1 ) - string1.ChatAt( (i - 1) * 2 + 1 );
				} else
					return s2FC - s1FC;
			}

			return 0;
		}
	}
}

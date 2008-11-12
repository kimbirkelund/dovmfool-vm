using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public struct String {
		public const uint STRING_LENGTH_OFFSET = 0;

		uint start;

		public static implicit operator uint( String cls ) {
			return cls.start;
		}

		public static implicit operator String( uint cls ) {
			return new String { start = cls };
		}
	}

	public static class ExtString {
		static readonly uint[] byteMasks = new uint[] { 0xFF000000, 0x00FF0000, 0x0000FF00, 0x000000FF };
		static readonly int[] byteRShifts = new int[] { 6, 4, 2, 0 };

		public static uint Length( this String str ) {
			return str.Get( String.STRING_LENGTH_OFFSET );
		}

		public static char ChatAt( this String str, uint pos ) {
			uint word = pos / 2;
			uint firstByte = (pos % 2) * 2;
			var bytes = new byte[] { 
				(byte) ((str.Get( word ) & byteMasks[firstByte]) >> byteRShifts[firstByte]), 
				(byte) ((str.Get( word ) & byteMasks[firstByte + 1]) >> byteRShifts[firstByte + 1]) };

			return Encoding.Unicode.GetChars( bytes )[0];
		}

		public static int Compare( this String string1, String string2 ) {
			var s1Len = string1.Length();
			var s1WC = s1Len / 2 + s1Len % 2;
			var s2Len = string2.Length();
			var s2WC = s2Len / 2 + s2Len % 2;

			for (uint i = 1; i < Math.Min( s1WC, s2WC ) + 1; i++) {
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

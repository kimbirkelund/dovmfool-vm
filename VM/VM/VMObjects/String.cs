using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;
using Sekhmet;

namespace VM.VMObjects {
	public struct String : IVMObject, IComparable<String> {
		#region Constants
		static readonly Word[] byteMasks = new Word[] { 0xFF000000, 0x00FF0000, 0x0000FF00, 0x000000FF };
		static readonly int[] byteRShifts = new int[] { 6, 4, 2, 0 };

		public const int LENGTH_OFFSET = 1;
		public const int FIRST_CHAR_OFFSET = 2;
		#endregion

		#region Properties
		public TypeId TypeId { get { return VMILLib.TypeId.String; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public int Length { get { return this[String.LENGTH_OFFSET]; } }
		#endregion

		#region Casts
		public static implicit operator int( String cls ) {
			return cls.start;
		}

		public static explicit operator String( int cls ) {
			return new String { start = cls };
		}

		public static implicit operator AppObject( String s ) {
			return new AppObject { Start = s.start };
		}

		public static explicit operator String( AppObject obj ) {
			return new String { start = obj.Start };
		}
		#endregion

		#region Instance methods
		public Char CharAt( int pos ) {
			int word = pos / 2;
			int firstByte = (pos % 2) * 2;

			return new Char {
				Byte1 = (byte) ((this[word] & byteMasks[firstByte]) >> byteRShifts[firstByte]),
				Byte2 = (byte) ((this[word] & byteMasks[firstByte + 1]) >> byteRShifts[firstByte + 1])
			};
		}

		#region Equals
		public bool Equals( String value ) {
			if (this == value)
				return true;
			var s1Len = this.Length;
			if (s1Len != value.Length)
				return false;

			var s1WC = s1Len / 2 + s1Len % 2;

			for (var i = String.LENGTH_OFFSET + 1; i < s1WC; i++)
				if (this[i] != value[i])
					return false;

			return true;
		}

		public override bool Equals( object value ) {
			if (!(value is String))
				return false;
			return this == (String) value;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==( String value1, String value2 ) {
			if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
				return true;
			if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
				return false;
			return value1.Equals( value2 );
		}

		public static bool operator !=( String value1, String value2 ) {
			return !(value1 == value2);
		}
		#endregion

		public Array Split( String splitAt ) {
			var list = List.New();

			if (splitAt.Length > Length)
				return Array.New( 0 );

			var lastMatch = 0;
			for (int i = 0; i < Length; ) {
			top:
				for (int j = 0; j < splitAt.Length; j++)
					if (CharAt( i ) != splitAt.CharAt( j )) {
						i += j + 1;
						goto top;
					}

				list.Add( Substring( lastMatch, i - lastMatch ) );
				lastMatch = i + splitAt.Length;
				i += splitAt.Length;
			}
			list.Add( Substring( lastMatch ) );
			return list.ToArray();
		}

		public String Substring( int start, int count ) {
			if (start < 0)
				throw new ArgumentOutOfBoundsException( "start" );
			if (count < 0 || start + count >= Length)
				throw new ArgumentOutOfBoundsException( "count" );

			var wordCount = (count + 3) / 4;
			var newStr = VirtualMachine.MemoryManager.Allocate<String>( LENGTH_OFFSET + wordCount );
			newStr[LENGTH_OFFSET] = count;

			for (int cur = start, w = FIRST_CHAR_OFFSET; cur < count + start; cur += 2, w++) {
				if (cur % 2 == 0)
					newStr[w] = this[cur / 2 + FIRST_CHAR_OFFSET];
				else
					newStr[w] = (this[cur / 2 + FIRST_CHAR_OFFSET] << 16) | (cur / 2 + 1 + FIRST_CHAR_OFFSET < Size ? (this[cur / 2 + 1 + FIRST_CHAR_OFFSET] >> 16) : (Word) 0);
			}

			return newStr;
		}

		public String Substring( int start ) {
			return Substring( start, Length - start );
		}

		public int CompareTo( String value ) {
			if (this == value)
				return 0;

			var s1Len = this.Length;
			var s1WC = (s1Len + 3) / 4;
			var s2Len = value.Length;
			var s2WC = (s2Len + 3) / 4;

			for (var i = String.LENGTH_OFFSET + 1; i < Math.Min( s1WC, s2WC ) + 1; i++) {
				if (this[i] == value[i])
					continue;

				var s1FC = this.CharAt( (i - 1) * 2 );
				var s2FC = value.CharAt( (i - 1) * 2 );

				if (s1FC == s2FC) {
					if (s1WC == i)
						return -1;
					else if (s2WC == i)
						return 1;
					else
						return value.CharAt( (i - 1) * 2 + 1 ) - this.CharAt( (i - 1) * 2 + 1 );
				} else
					return s2FC - s1FC;
			}

			return 0;
		}

		public override string ToString() {
			return Encoding.Unicode.GetString( GetUInts().ToByteStream().Take( Length * 2 ).ToArray() );
		}

		IEnumerable<uint> GetUInts() {
			for (int i = String.FIRST_CHAR_OFFSET; i < Size; i++)
				yield return this[i];
		}
		#endregion
	}
}

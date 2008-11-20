using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;
using Sekhmet;

namespace VM.VMObjects {
	public struct String : IVMObject<String>, IComparable<String> {
		#region Constants
		static readonly Word byteMask = 0x000000FF;
		static readonly int[] byteRShifts = new int[] { 24, 16, 8, 0 };

		public const int LENGTH_OFFSET = 1;
		public const int HASH_CODE_OFFSET = 2;
		public const int FIRST_CHAR_OFFSET = 3;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return VMILLib.TypeId.String; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
		}

		public int Length { get { return this[String.LENGTH_OFFSET]; } }

		static Handle<String> empty;
		public static Handle<String> Empty {
			get {
				if (empty == null)
					empty = VirtualMachine.ConstantPool.RegisterString( "" );
				return empty;
			}
		}

		static Handle<String> dot;
		public static Handle<String> Dot {
			get {
				if (dot == null)
					dot = VirtualMachine.ConstantPool.RegisterString( "." );
				return dot;
			}
		}
		#endregion

		#region Cons
		public String( int start ) {
			this.start = start;
		}

		public String New( int startPosition ) {
			return new String( startPosition );
		}
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
			int word = pos / 2 + FIRST_CHAR_OFFSET;
			int firstByte = (pos % 2) * 2;

			var b1 = (byte) ((this[word] >> byteRShifts[firstByte]) & byteMask);
			var b2 = (byte) ((this[word] >> byteRShifts[firstByte + 1]) & byteMask);

			return new Char( b1, b2 );
		}

		#region Equals
		public bool Equals( String value ) {
			if (this.start == value.start)
				return true;
			var s1Len = this.Length;
			if (s1Len != value.Length)
				return false;

			var s1WC = s1Len / 2 + s1Len % 2;

			for (var i = String.FIRST_CHAR_OFFSET; i < String.FIRST_CHAR_OFFSET + s1WC; i++)
				if (this[i] != value[i])
					return i != String.FIRST_CHAR_OFFSET + s1WC - 1 ? false : s1Len % 2 == 1 && (this[i] & 0xFFFF0000) == (value[i] & 0xFFFF0000);

			return true;
		}

		public override bool Equals( object value ) {
			if (!(value is String))
				return false;
			return this == (String) value;
		}

		public override int GetHashCode() {
			if (this[HASH_CODE_OFFSET] == 0) {
				long val = 0;
				for (int i = 0; i < Length; i++)
					val += CharAt( i );
				this[HASH_CODE_OFFSET] = val.GetHashCode();
			}

			return this[HASH_CODE_OFFSET];
		}

		public static bool operator ==( String value1, String value2 ) {
			return value1.Equals( value2 );
		}

		public static bool operator !=( String value1, String value2 ) {
			return !(value1 == value2);
		}
		#endregion

		public Array Split( String splitAt ) {
			var list = List.CreateInstance();

			if (splitAt.Length > Length)
				return Array.CreateInstance( 0 );

			var lastMatch = 0;
			for (int i = 0; i < Length; ) {
			top:
				for (int j = 0; j < splitAt.Length && i < Length; j++)
					if (CharAt( i ) != splitAt.CharAt( j )) {
						i += j + 1;
						goto top;
					}

				list.Add( Substring( lastMatch, i - lastMatch ) );
				lastMatch = i + splitAt.Length;
				i += splitAt.Length;
			}

			return list.ToArray();
		}

		public int IndexOf( String str, int startIndex ) {
			int i = 0;
		top:
			if (i >= Length)
				return -1;
			for (int j = 0; j < str.Length; j++)
				if (CharAt( i ) != str.CharAt( j )) {
					i += j + 1;
					goto top;
				}

			return i;
		}

		public int IndexOf( String str ) {
			return IndexOf( str, 0 );
		}

		public String Substring( int start, int count ) {
			if (start < 0)
				throw new ArgumentOutOfBoundsException( "start" );
			if (count < 0 || start + count > Length)
				throw new ArgumentOutOfBoundsException( "count" );

			var wordCount = (count + 1) / 2;
			var newStr = CreateInstance( count );
			newStr[LENGTH_OFFSET] = count;

			for (int cur = start, w = FIRST_CHAR_OFFSET; cur < count + start; cur += 2, w++) {
				if (cur % 2 == 0)
					newStr[w] = this[cur / 2 + FIRST_CHAR_OFFSET];
				else {
					Word w1 = (this[cur / 2 + FIRST_CHAR_OFFSET] << 16);
					Word w2 = (cur / 2 + 1 + FIRST_CHAR_OFFSET < Size ? (this[cur / 2 + 1 + FIRST_CHAR_OFFSET] >> 16) : (Word) 0);
					Word w3 = w1 | w2;
					newStr[w] = w1 | w2;
				}
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
			if (IsNull)
				return "{NULL}";
			return Encoding.Unicode.GetString( GetUInts().ToByteStream().Take( Length * 2 ).ToArray() );
		}

		IEnumerable<uint> GetUInts() {
			for (int i = String.FIRST_CHAR_OFFSET; i < Size; i++)
				yield return this[i];
		}
		#endregion

		#region Static methods
		public static String CreateInstance( int length ) {
			return VirtualMachine.MemoryManager.Allocate<String>( FIRST_CHAR_OFFSET - 1 + (length + 1) / 2 );
		}
		#endregion
	}
}

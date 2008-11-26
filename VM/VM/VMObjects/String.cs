using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;
using Sekhmet;

namespace VM.VMObjects {
	public struct String : IVMObject<String> {
		#region Constants
		public const int LENGTH_OFFSET = 1;
		public const int HASH_CODE_OFFSET = 2;
		public const int FIRST_CHAR_OFFSET = 3;
		#endregion

		#region Properties
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.String; } }

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
			return new AppObject( s.start );
		}

		public static explicit operator String( AppObject obj ) {
			return new String( obj.Start );
		}
		#endregion

		#region Instance methods
		public override bool Equals( object value ) {
			if (value is String)
				return ExtString.Equals( this.ToHandle(), ((String) value).ToHandle() );
			if (value is Handle<String>)
				return ExtString.Equals( this.ToHandle(), (Handle<String>) value );
			return false;
		}

		public override int GetHashCode() {
			return ExtString.GetHashCode( this );
		}

		public override string ToString() {
			return ExtString.ToString( this );
		}
		#endregion

		#region Static methods
		public static Handle<String> CreateInstance( int length ) {
			var str = VirtualMachine.MemoryManager.Allocate<String>( FIRST_CHAR_OFFSET - 1 + (length + 1) / 2 );
			str[String.LENGTH_OFFSET] = length;
			return str;
		}
		#endregion
	}

	public static class ExtString {
		static readonly Word byteMask = 0x000000FF;
		static readonly int[] byteRShifts = new int[] { 24, 16, 8, 0 };

		public static int Length( this Handle<String> obj ) {
			return obj[String.LENGTH_OFFSET];
		}

		public static int GetHashCode( this Handle<String> obj ) {
			if (obj[String.HASH_CODE_OFFSET] == 0) {
				long val = 0;
				for (int i = 0; i < obj.Length(); i++)
					val += obj.CharAt( i );
				obj[String.HASH_CODE_OFFSET] = val.GetHashCode();
			}

			return obj[String.HASH_CODE_OFFSET];
		}

		public static bool Equals( this Handle<String> obj, Handle<String> other ) {
			var objNull = object.ReferenceEquals( obj, null );
			var otherNull = object.ReferenceEquals( other, null );
			if (objNull || otherNull)
				return objNull && otherNull;
			if (obj.Start == other.Start)
				return true;
			var s1Len = obj.Length();
			if (s1Len != other.Length())
				return false;

			var s1WC = s1Len / 2 + s1Len % 2;

			for (var i = String.FIRST_CHAR_OFFSET; i < String.FIRST_CHAR_OFFSET + s1WC; i++)
				if (obj[i] != other[i])
					return i != String.FIRST_CHAR_OFFSET + s1WC - 1 ? false : s1Len % 2 == 1 && (obj[i] & 0xFFFF0000) == (other[i] & 0xFFFF0000);

			return true;
		}

		public static Character CharAt( this Handle<String> obj, int pos ) {
			int word = pos / 2 + String.FIRST_CHAR_OFFSET;
			int firstByte = (pos % 2) * 2;

			var b1 = (byte) ((obj[word] >> byteRShifts[firstByte]) & byteMask);
			var b2 = (byte) ((obj[word] >> byteRShifts[firstByte + 1]) & byteMask);

			return new Character( b1, b2 );
		}

		public static Handle<Array> Split( this Handle<String> obj, Handle<String> splitAt ) {
			var list = new List<Handle<String>>();

			if (splitAt.Length() > obj.Length())
				return Array.CreateInstance( 0 );

			var lastMatch = 0;
			for (int i = 0; i < obj.Length(); ) {
			top:
				for (int j = 0; j < splitAt.Length() && i < obj.Length(); j++)
					if (obj.CharAt( i ) != splitAt.CharAt( j )) {
						i += j + 1;
						goto top;
					}

				list.Add( obj.Substring( lastMatch, i - lastMatch ) );
				lastMatch = i + splitAt.Length();
				i += splitAt.Length();
			}

			var arr = Array.CreateInstance( list.Count );
			list.ForEach( ( e, i ) => arr.Set( i, e, true ) );

			return arr;
		}

		public static int IndexOf( this Handle<String> obj, Handle<String> str, int startIndex ) {
			int i = 0;
		top:
			if (i >= obj.Length())
				return -1;
			for (int j = 0; j < str.Length(); j++)
				if (obj.CharAt( i ) != str.CharAt( j )) {
					i += j + 1;
					goto top;
				}

			return i;
		}

		public static int IndexOf( this Handle<String> obj, Handle<String> str ) {
			return obj.IndexOf( str, 0 );
		}

		public static int LastIndexOf( this Handle<String> obj, Handle<String> str ) {
			int last = -1;
			last = obj.IndexOf( str, 0 );
		top:
			if (last != -1) {
				int next = obj.IndexOf( str, last + 1 );
				if (next != -1) {
					last = next;
					goto top;
				}
			}
			return last;
		}

		public static Handle<String> Substring( this Handle<String> obj, int start, int count ) {
			if (start < 0)
				throw new ArgumentOutOfBoundsException( "start" );
			if (count < 0 || start + count > obj.Length())
				throw new ArgumentOutOfBoundsException( "count" );

			var wordCount = (count + 1) / 2;
			var newStr = String.CreateInstance( count );

			for (int cur = start, w = String.FIRST_CHAR_OFFSET; cur < count + start; cur += 2, w++) {
				if (cur % 2 == 0)
					newStr[w] = obj[cur / 2 + String.FIRST_CHAR_OFFSET];
				else {
					Word w1 = (obj[cur / 2 + String.FIRST_CHAR_OFFSET] << 16);
					Word w2 = (cur / 2 + 1 + String.FIRST_CHAR_OFFSET < obj.Size() ? (obj[cur / 2 + 1 + String.FIRST_CHAR_OFFSET] >> 16) : (Word) 0);
					Word w3 = w1 | w2;
					newStr[w] = w1 | w2;
				}
			}

			return newStr;
		}

		public static Handle<String> Substring( this Handle<String> obj, int start ) {
			return obj.Substring( start, obj.Length() - start );
		}

		public static int CompareTo( this Handle<String> obj, Handle<String> value ) {
			if (obj == value)
				return 0;

			var s1Len = obj.Length();
			var s1WC = (s1Len + 1) / 2;
			var s2Len = value.Length();
			var s2WC = (s2Len + 1) / 2;

			for (var i = String.LENGTH_OFFSET + 1; i < Math.Min( s1WC, s2WC ) + 1; i++) {
				if (obj[i] == value[i])
					continue;

				var s1FC = obj.CharAt( (i - 1) * 2 );
				var s2FC = value.CharAt( (i - 1) * 2 );

				if (s1FC == s2FC) {
					if (s1WC == i)
						return -1;
					else if (s2WC == i)
						return 1;
					else
						return value.CharAt( (i - 1) * 2 + 1 ) - obj.CharAt( (i - 1) * 2 + 1 );
				} else
					return s2FC - s1FC;
			}

			return 0;
		}

		public static Handle<String> Concat( this Handle<String> str1, Handle<VM.VMObjects.String> str2 ) {
			var str = String.CreateInstance( str1.Length() + str2.Length() );

			var s1Len = str1.Length();
			var s1WC = (s1Len + 1) / 2;
			var s2Len = str2.Length();
			var s2WC = (s2Len + 1) / 2;

			for (int i = 0; i < s1WC; i++)
				str[String.FIRST_CHAR_OFFSET + i] = str1[String.FIRST_CHAR_OFFSET + i];

			if (s1Len % 2 == 0) {
				for (int i = 0; i < s2WC; i++)
					str[String.FIRST_CHAR_OFFSET + s1WC + i] = str2[String.FIRST_CHAR_OFFSET + i];
			} else {
				int j = String.FIRST_CHAR_OFFSET + s1WC - 1;
				for (int i = 0; i < s2WC; i++) {
					str[j++] |= str2[String.FIRST_CHAR_OFFSET + i] >> 16;
					str[j] |= str2[String.FIRST_CHAR_OFFSET + i] << 16;
				}
			}

			return str;
		}

		public static string ToString( this Handle<String> obj ) {
			if (obj == null)
				return "{NULL}";
			return Encoding.Unicode.GetString( obj.GetUInts().ToByteStream().Take( obj.Length() * 2 ).ToArray() );
		}

		static IEnumerable<uint> GetUInts( this Handle<String> obj ) {
			for (int i = String.FIRST_CHAR_OFFSET; i < obj.Size(); i++)
				yield return obj[i];
		}

		public static Handle<String> ToVMString( this string str ) {
			return VirtualMachine.ConstantPool.RegisterString( str );
		}
	}
}

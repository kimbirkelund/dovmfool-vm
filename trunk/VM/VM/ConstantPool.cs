using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using Sekhmet;

namespace VM {
	class ConstantPool {
		List<Handle<VMObjects.String>> strings = new List<Handle<VM.VMObjects.String>>();

		public Handle<VMObjects.String> GetString( int index ) {
			return strings[index];
		}

		public int RegisterString( Handle<VMObjects.String> str ) {
			var index = strings.FindIndex( s => s.Value.Equals( str.Value ) );
			if (index == -1) {
				strings.Add( str );
				return strings.Count - 1;
			}
			return index;
		}

		public Handle<VMObjects.String> RegisterString( string str ) {
			var bytes = Encoding.Unicode.GetBytes( str );
			if (bytes.Length % 4 != 0) {
				var temp = bytes;
				bytes = new byte[temp.Length + 4 - (temp.Length % 4)];
				System.Array.Copy( temp, bytes, temp.Length );
			}
			var ints = bytes.ToUIntStream();

			var vmStr = VMObjects.String.CreateInstance( str.Length );
			vmStr[VMObjects.String.LENGTH_OFFSET] = str.Length;
			ints.ForEach( ( b, i ) => vmStr[i + VMObjects.String.FIRST_CHAR_OFFSET] = b );

			return GetString( RegisterString( vmStr ) );
		}
	}
}

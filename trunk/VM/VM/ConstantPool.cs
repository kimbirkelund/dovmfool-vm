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

		public Handle<VMObjects.String> Intern( Handle<VMObjects.String> str ) {
			if (str.IsInterned())
				return str;

			foreach (var istr in strings)
				if (istr.Equals( str ))
					return istr;

			strings.Add( str );
			str[VMObjects.String.LENGTH_INTERNED_OFFSET] |= 0x80000000;
			return str;
		}
	}
}

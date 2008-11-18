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
				return strings.Count;
			}
			return index;
		}

		public Handle<VMObjects.String> RegisterString( string str ) {
			var ints = Encoding.Unicode.GetBytes( str ).ToUIntStream();

			var vmStr = VirtualMachine.MemoryManager.Allocate<VMObjects.String>( 1 + ints.Count() );
			vmStr[VMObjects.String.LENGTH_OFFSET] = ints.Count();
			ints.ForEach( ( b, i ) => vmStr[i + VMObjects.String.FIRST_CHAR_OFFSET] = b );

			return vmStr.ToHandle();
		}
	}
}

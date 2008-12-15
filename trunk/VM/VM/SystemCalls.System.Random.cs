using System.Linq;
using System.Text;
using VM.VMObjects;
using System;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Rand" )]
			class Rand {
				static Random r = new Random(7657);

				[SystemCallMethod( "next:1" )]
				public static UValue ToString( UValue receiver, UValue[] arguments ) {
					return r.Next( arguments[0].Value );
				}
			}
		}
	}
}
using System.Linq;
using System.Text;
using VM.VMObjects;
using System;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Rand" )]
			class Rand {
				[SystemCallMethod( "next:1" )]
				public static UValue ToString( UValue receiver, UValue[] arguments ) {
					Random r = new Random();

					return r.Next( arguments[0].Value ).ToHandle().ToUValue();
				}
			}
		}
	}
}
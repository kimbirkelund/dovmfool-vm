using System.Linq;
using System.Text;
using VM.VMObjects;
using System;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "DT" )]
			class DT {
				[SystemCallMethod( "get-millisecond:0" )]
				public static UValue ToString( UValue receiver, UValue[] arguments ) {
					DateTime Last = new DateTime( DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddHours( -1 ).Hour, 0, 0, 0 );

					TimeSpan TS = (TimeSpan) (DateTime.Now - Last);
					return ((int) TS.TotalMilliseconds);
				}
			}
		}
	}
}
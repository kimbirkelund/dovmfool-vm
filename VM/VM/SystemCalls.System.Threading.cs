using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		partial class System {
			#region Threading
			[SystemCallClass( "Threading" )]
			public static partial class Threading {
				[SystemCallMethod( "sleep:1" )]
				public static UValue Sleep( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					Thread.Sleep( arguments[0].Value );
					return UValue.Void();
				}
			}
			#endregion
		}
	}
}
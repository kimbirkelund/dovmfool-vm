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
				public static Handle<AppObject> Sleep( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					if (!(arguments[0] is IntHandle))
						throw new ArgumentException( "Argument must be an integer." );

					Thread.Sleep( arguments[0].Start );
					return null;
				}
			}
			#endregion
		}
	}
}
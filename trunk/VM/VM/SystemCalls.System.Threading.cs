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
					global::System.Threading.Thread.Sleep( arguments[0].Value );
					return UValue.Void();
				}

				#region Thread
				[SystemCallClass( "Thread" )]
				public static partial class Thread {
					[SystemCallMethod( "start:1" )]
					public static UValue Start( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var entrypointObj = arguments[0].ToHandle();
						var entrypointCls = entrypointObj.Class().ToHandle();
						var entrypointHandler = entrypointCls.ResolveMessageHandler( receiver.ToHandle().Class().ToHandle(), KnownStrings.run_0 ).ToHandle();

						var interp = VirtualMachine.Fork( entrypointHandler, entrypointObj );
						var rech = receiver.ToHandle();
						rech.SetField( rech.GetFieldOffset( KnownClasses.SystemThreadingThread ) + 0, interp.Id );
						interp.Start();

						return UValue.Void();
					}

					[SystemCallMethod( "join:0" )]
					public static UValue Join( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var thread = receiver.ToHandle();
						var interp = VirtualMachine.GetInterpretor( thread.GetField( thread.GetFieldOffset( KnownClasses.SystemThreadingThread ) + 0 ).Value );
						interp.Join();

						return UValue.Void();
					}
				}
				#endregion
			}
			#endregion
		}
	}
}
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
				public static UValue Sleep( UValue receiver, UValue[] arguments ) {
					InterpreterThread.Current.Sleep( arguments[0].Value );
					return UValue.Void();
				}

				#region Thread
				[SystemCallClass( "Thread" )]
				public static partial class Thread {
					[SystemCallMethod( "start:1" )]
					public static UValue Start( UValue receiver, UValue[] arguments ) {
						using (var entrypointObj = arguments[0].ToHandle())
						using (var entrypointCls = entrypointObj.Class().ToHandle())
						using (var hReceiver = receiver.ToHandle())
						using (var hReceiverClass = hReceiver.Class().ToHandle())
						using (var entrypointHandler = entrypointCls.ResolveMessageHandler( hReceiverClass, KnownStrings.run_0 ).ToHandle()) {
							var interp = VirtualMachine.Fork( entrypointHandler, entrypointObj );
							hReceiver.SetField( hReceiver.GetFieldOffset( KnownClasses.System_Threading_Thread ) + 0, interp.Id );
							interp.Start();

							return UValue.Void();
						}
					}

					[SystemCallMethod( "join:0" )]
					public static UValue Join( UValue receiver, UValue[] arguments ) {
						using (var thread = receiver.ToHandle()) {
							var interp = VirtualMachine.GetInterpreter(
								thread.GetField( thread.GetFieldOffset( KnownClasses.System_Threading_Thread ) + 0 ).Value );
							interp.Join();

							return UValue.Void();
						}
					}
				}
				#endregion
			}
			#endregion
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		[SystemCallClass( "System" )]
		partial class System {
			[SystemCallClass( "Console" )]
			class Console {
				[SystemCallMethod( "write-line:1" )]
				public static UValue WriteLine( UValue receiver, UValue[] arguments ) {
					using (var hArg0 = arguments[0].ToHandle())
						global::System.Console.WriteLine( VMObjects.ExtString.ToString( InterpreterThread.Current.Send( KnownStrings.to_string_0, hArg0 ).ToWeakHandle<VMObjects.String>() ) );
					return UValue.Void();
				}

				[SystemCallMethod( "write-line:0" )]
				public static UValue WriteEmptyLine( UValue receiver, UValue[] arguments ) {
					global::System.Console.WriteLine();
					return UValue.Void();
				}

				[SystemCallMethod( "write:1" )]
				public static UValue Write( UValue receiver, UValue[] arguments ) {
					using (var hArg0 = arguments[0].ToHandle())
						global::System.Console.Write( VMObjects.ExtString.ToString( InterpreterThread.Current.Send( KnownStrings.to_string_0, hArg0 ).ToWeakHandle<VMObjects.String>() ) );
					return UValue.Void();
				}

				[SystemCallMethod( "read-line:0" )]
				public static UValue ReadLine( UValue receiver, UValue[] arguments ) {
					return UValue.Ref( KnownClasses.System_String, global::System.Console.ReadLine().ToVMString().Start );
				}
			}
		}
	}
}
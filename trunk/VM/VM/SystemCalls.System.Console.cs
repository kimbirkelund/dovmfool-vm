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
				static Handle<VMObjects.String> toStringStr = "to-string:0".ToVMString().Intern();

				[SystemCallMethod( "write-line:1" )]
				public static UValue WriteLine( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					global::System.Console.WriteLine( interpretor.Send( toStringStr, arguments[0].ToHandle() ).ToHandle<VMObjects.String>().Value.ToString() );
					return UValue.Void();
				}

				[SystemCallMethod( "write-line:0" )]
				public static UValue WriteEmptyLine( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					global::System.Console.WriteLine();
					return UValue.Void();
				}

				[SystemCallMethod( "write:1" )]
				public static UValue Write( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					global::System.Console.Write( interpretor.Send( toStringStr, arguments[0].ToHandle() ).ToHandle<VMObjects.String>().Value.ToString() );
					return UValue.Void();
				}

				[SystemCallMethod( "read-line:0" )]
				public static UValue ReadLine( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return UValue.Ref( KnownClasses.System_String, global::System.Console.ReadLine().ToVMString() );
				}
			}
		}
	}
}
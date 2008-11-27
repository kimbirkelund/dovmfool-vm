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
				public static Handle<VMObjects.AppObject> WriteLine( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					global::System.Console.WriteLine( arguments[0].Send( toStringStr ).To<VMObjects.String>().Value.ToString() );
					return null;
				}

				[SystemCallMethod( "write-line:0" )]
				public static Handle<VMObjects.AppObject> WriteEmptyLine( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					global::System.Console.WriteLine();
					return null;
				}

				[SystemCallMethod( "write:1" )]
				public static Handle<VMObjects.AppObject> Write( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					global::System.Console.Write( arguments[0].Send( toStringStr ).To<VMObjects.String>().Value.ToString() );
					return null;
				}
			}
		}
	}
}
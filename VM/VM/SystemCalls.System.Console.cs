using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class SystemCalls {
		[SystemCallClass( "System" )]
		partial class System {
			[SystemCallClass( "Console" )]
			class Console {
				[SystemCallMethod( "write-line:1" )]
				public static Handle<VMObjects.AppObject> WriteLine( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var objBase = arguments[0];
					//if (objBase.TypeId() == VMILLib.TypeId.String)
					//    global::System.Console.WriteLine( objBase.To<VMObjects.String>().Value.ToString() );
					//else if (objBase is IntHandle)
					//    global::System.Console.WriteLine( ((IntHandle) objBase).Value );
					//else {
					var str = interpretor.Send( VirtualMachine.ConstantPool.RegisterString( "to-string:0" ), arguments[0] ).To<VMObjects.String>().Value.ToString();
					global::System.Console.WriteLine( str );
					//}

					return null;
				}
			}
		}
	}
}
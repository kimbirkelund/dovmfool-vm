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
					var objBase = (VMObjects.ObjectBase) arguments[0].Value;
					if (objBase.TypeId == VMILLib.TypeId.String)
						global::System.Console.WriteLine( ((VMObjects.String) objBase).ToString() );
					else if (objBase.TypeId == VMILLib.TypeId.Integer)
						global::System.Console.WriteLine( objBase.Start );
					else {
						var str = interpretor.Send( VirtualMachine.ConstantPool.RegisterString( "to-string:0" ), arguments[0] ).ToString();
						global::System.Console.WriteLine( str );
					}

					return null;
				}
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		[SystemCallClass( "Object" )]
		class Object {
			[SystemCallMethod( "get-type:0" )]
			public static UValue GetType( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
				return UValue.Ref( KnownClasses.SystemReflectionClass, receiver.Type );
			}

			[SystemCallMethod( "to-string:0" )]
			public static UValue ToString( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
				return UValue.Ref( KnownClasses.SystemReflectionClass, receiver.Type.ToHandle().Name() );
			}


			[SystemCallMethod( "equals:1" )]
			public static UValue Equals( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
				return receiver.Equals( arguments[0] ) ? 1 : 0;
			}
		}
	}
}
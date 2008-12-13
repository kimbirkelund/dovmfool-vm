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
			public static UValue GetType( UValue receiver, UValue[] arguments ) {
				return UValue.Ref( KnownClasses.System_Reflection_Class, receiver.Type );
			}

			[SystemCallMethod( "to-string:0" )]
			public static UValue ToString( UValue receiver, UValue[] arguments ) {
				using (var receiverType = receiver.Type.ToHandle())
					return UValue.Ref( KnownClasses.System_Reflection_Class, receiverType.Name() );
			}


			[SystemCallMethod( "equals:1" )]
			public static UValue Equals( UValue receiver, UValue[] arguments ) {
				return receiver.Equals( arguments[0] ) ? 1 : 0;
			}
		}
	}
}
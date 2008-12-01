using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	static partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Array" )]
			public static partial class Array {
				[SystemCallMethod( "new-array:1" )]
				public static UValue New( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					if (arguments[0].Type != KnownClasses.SystemInteger.Start)
						throw new ArgumentException( "Argument should be an integer.", "initialSize" );

					var initialSize = arguments[0].Value;
					var arr = VMObjects.Array.CreateInstance( initialSize );

					return UValue.Ref( KnownClasses.SystemArray, arr );
				}

				[SystemCallMethod( "set:2" )]
				public static UValue Set( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					arr.Set( arguments[0].Value, arguments[1] );
					return UValue.Void();
				}

				[SystemCallMethod( "get:1" )]
				public static UValue Get( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					return arr.Get( arguments[0].Value );
				}

				[SystemCallMethod( "length:0" )]
				public static UValue Length( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					return arr.Length();
				}
			}
		}
	}
}
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
					if (arguments[0].Type != KnownClasses.System_Integer.Start)
						throw new ArgumentException( "Argument should be an integer.".ToVMString(), "initialSize".ToVMString() );

					var initialSize = arguments[0].Value;
					var arr = VMObjects.Array.CreateInstance( initialSize );

					return UValue.Ref( KnownClasses.System_Array, arr );
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

				[SystemCallMethod( "index-of:1" )]
				public static UValue IndexOf( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					var index = arguments[0].ToHandle();

					return arr.IndexOf( index );
				}


				[SystemCallMethod( "copy:4" )]
				public static UValue Copy( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					var idsrc = arguments[0].Value;
					var arr2 = arguments[1].ToHandle<VMObjects.Array>();
					var iddes = arguments[2].Value;
					var count = arguments[3].Value;
					VMObjects.Array.Copy( arr, idsrc, arr2, iddes, count );
					return UValue.Void();

				}
				[SystemCallMethod( "copydescending:4" )]
				public static UValue CopyDescending( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = receiver.ToHandle<VMObjects.Array>();
					var idsrc = arguments[0].Value;
					var arr2 = arguments[1].ToHandle<VMObjects.Array>();
					var iddes = arguments[2].Value;
					var count = arguments[3].Value;
					VMObjects.Array.CopyDescending( arr, idsrc, arr2, iddes, count );
					return UValue.Void();
				}
			}
		}
	}
}
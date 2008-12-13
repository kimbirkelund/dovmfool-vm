using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "String" )]
			class String {
				[SystemCallMethod( "get-hashcode:0" )]
				public static UValue GetHashcode( UValue receiver, UValue[] arguments ) {
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return hReceiver.GetHashCode();
				}

				[SystemCallMethod( "equals:1" )]
				public static UValue Equals( UValue receiver, UValue[] arguments ) {
					using (var arg = arguments[0].ToHandle<VMObjects.String>()) {
						if (arg.Class().Start != KnownClasses.System_String.Start)
							return UValue.Null();
						using (var hReceiver = receiver.ToHandle<VMObjects.String>())
							return hReceiver.Equals( arg ) ? 1 : 0;
					}
				}

				[SystemCallMethod( "compare-to:1" )]
				public static UValue CompareTo( UValue receiver, UValue[] arguments ) {
					if (arguments[0].Type != KnownClasses.System_String.Start)
						return 0;

					using (var other = ((VMObjects.String) arguments[0].Value).ToHandle())
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return hReceiver.CompareTo( other );
				}

				[SystemCallMethod( "substring:2" )]
				public static UValue Substring( UValue receiver, UValue[] arguments ) {
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return UValue.Ref( KnownClasses.System_String, hReceiver.Substring( arguments[0].Value, arguments[1].Value ) );
				}

				[SystemCallMethod( "split:1" )]
				public static UValue Split( UValue receiver, UValue[] arguments ) {
					using (var splitter = arguments[0].ToHandle<VMObjects.String>())
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return UValue.Ref( KnownClasses.System_Array, hReceiver.Split( splitter ) );
				}

				[SystemCallMethod( "index-of:1" )]
				public static UValue IndexOf( UValue receiver, UValue[] arguments ) {
					using (var other = arguments[0].ToHandle<VMObjects.String>())
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return hReceiver.IndexOf( other );
				}

				[SystemCallMethod( "last-index-of:1" )]
				public static UValue LastIndexOf( UValue receiver, UValue[] arguments ) {
					using (var arr = arguments[0].ToHandle<VMObjects.String>())
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return hReceiver.LastIndexOf( arr );
				}

				[SystemCallMethod( "concat:1" )]
				public static UValue Concat( UValue receiver, UValue[] arguments ) {
					using (var str1 = receiver.ToHandle<VMObjects.String>())
					using (var hArg0 = arguments[0].ToHandle())
					using (var str2 = InterpreterThread.Current.Send( KnownStrings.to_string_0, hArg0 ).ToHandle<VMObjects.String>())
						return UValue.Ref( KnownClasses.System_String, str1.Concat( str2 ) );
				}

				[SystemCallMethod( "length:0" )]
				public static UValue Length( UValue receiver, UValue[] arguments ) {
					using (var hReceiver = receiver.ToHandle<VMObjects.String>())
						return hReceiver.Length();
				}
			}
		}
	}
}
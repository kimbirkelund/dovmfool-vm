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
				static Handle<VMObjects.String> toStringStr = "to-string:0".ToVMString().Intern();

				[SystemCallMethod( "get-hashcode:0" )]
				public static UValue GetHashcode( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return ((VMObjects.String) receiver.Value).ToHandle().GetHashCode();
				}

				[SystemCallMethod( "equals:1" )]
				public static UValue Equals( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arg = ((VMObjects.String) arguments[0].Value).ToHandle();
					if (arg.Class().Start != KnownClasses.System_String.Start)
						return UValue.Null();
					return ((VMObjects.String) receiver.Value).ToHandle().Equals( arg ) ? 1 : 0;
				}

				[SystemCallMethod( "compare-to:1" )]
				public static UValue CompareTo( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					if (arguments[0].Type != KnownClasses.System_String.Start)
						return 0;

					var other = ((VMObjects.String) arguments[0].Value).ToHandle();
					return ((VMObjects.String) receiver.Value).ToHandle().CompareTo( other );
				}

				[SystemCallMethod( "substring:2" )]
				public static UValue Substring( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return UValue.Ref( KnownClasses.System_String, ((VMObjects.String) receiver.Value).ToHandle().Substring( arguments[0].Value, arguments[1].Value ) );
				}

				[SystemCallMethod( "split:1" )]
				public static UValue Split( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var splitter = ((VMObjects.String) arguments[0].Value).ToHandle();
					return UValue.Ref( KnownClasses.System_Array, ((VMObjects.String) receiver.Value).ToHandle().Split( splitter ) );
				}

				[SystemCallMethod( "index-of:1" )]
				public static UValue IndexOf( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var other = ((VMObjects.String) arguments[0].Value).ToHandle();

					return ((VMObjects.String) receiver.Value).ToHandle().IndexOf( other );
				}

				[SystemCallMethod( "last-index-of:1" )]
				public static UValue LastIndexOf( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var arr = ((VMObjects.String) arguments[0].Value).ToHandle();

					return ((VMObjects.String) receiver.Value).ToHandle().LastIndexOf( arr );
				}

				[SystemCallMethod( "concat:1" )]
				public static UValue Concat( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					var str1 = ((VMObjects.String) receiver.Value).ToHandle();
					var str2 = interpretor.Send( toStringStr, arguments[0].ToHandle() );

					return UValue.Ref( KnownClasses.System_String, str1.Concat( str2.ToHandle<VMObjects.String>() ) );
				}

				[SystemCallMethod( "length:0" )]
				public static UValue Length( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return ((VMObjects.String) receiver.Value).ToHandle().Length();
				}
			}
		}
	}
}
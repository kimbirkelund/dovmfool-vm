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
				public static Handle<VMObjects.AppObject> GetHashcode( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					return new IntHandle( receiver.To<VMObjects.String>().GetHashCode() );
				}

				[SystemCallMethod( "equals:1" )]
				public static Handle<VMObjects.AppObject> Equals( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					if (arguments[0].Class() != KnownClasses.SystemString)
						return new IntHandle( 0 );
					return new IntHandle( receiver.To<VMObjects.String>().Equals( arguments[0].To<VMObjects.String>() ) ? 1 : 0 );
				}

				[SystemCallMethod( "compare-to:1" )]
				public static Handle<VMObjects.AppObject> CompareTo( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = arguments[0].To<VMObjects.String>();
					return new IntHandle( receiver.To<VMObjects.String>().CompareTo( arr.Value ) );

				}

				[SystemCallMethod( "substring:2" )]
				public static Handle<VMObjects.AppObject> Substring( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					return receiver.To<VMObjects.String>().Substring( ((IntHandle) arguments[0]).Value, ((IntHandle) arguments[1]).Value ).To<AppObject>();
				}

				[SystemCallMethod( "split:1" )]
				public static Handle<VMObjects.AppObject> Split( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = arguments[0].To<VMObjects.String>();
					return receiver.To<VMObjects.String>().Split( arr ).To<VMObjects.AppObject>();
				}

				[SystemCallMethod( "index-of:1" )]
				public static Handle<VMObjects.AppObject> IndexOf( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = arguments[0].To<VMObjects.String>();

					return new IntHandle( receiver.To<VMObjects.String>().IndexOf( arr ) );
				}

				[SystemCallMethod( "last-index-of:1" )]
				public static Handle<VMObjects.AppObject> LastIndexOf( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = arguments[0].To<VMObjects.String>();

					return new IntHandle( receiver.To<VMObjects.String>().LastIndexOf( arr ) );
				}

				[SystemCallMethod( "concat:1" )]
				public static Handle<VMObjects.AppObject> Concat( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var str1 = receiver.To<VMObjects.String>();
					var str2 = interpretor.Send( toStringStr, arguments[0] ).To<VMObjects.String>();

					return str1.Concat( str2 ).To<VMObjects.AppObject>();
				}

				[SystemCallMethod( "length:0" )]
				public static Handle<AppObject> Length( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					return new IntHandle( receiver.To<VMObjects.String>().Length() );
				}
			}
		}
	}
}
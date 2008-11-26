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
				public static Handle<VMObjects.AppObject> GetHashcode( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					//var objBase = arguments[0];
					//if (objBase.TypeId() == VMILLib.TypeId.String)
					//    global::System.Console.WriteLine( objBase.To<VMObjects.String>().Value.ToString() );
					//else if (objBase is IntHandle)
					//    global::System.Console.WriteLine( ((IntHandle) objBase).Value );
					//else {
					//var str = interpretor.Send(VirtualMachine.ConstantPool.RegisterString("to-string:0"), arguments[0]).To<VMObjects.String>().Value.ToString();
					//global::System.Console.WriteLine(str);
					//}

					return new IntHandle( receiver.To<VMObjects.String>().GetHashCode() );
				}

				[SystemCallMethod( "equals:1" )]
				public static Handle<VMObjects.AppObject> Equals( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					return new IntHandle( receiver.To<VMObjects.String>().Equals( arguments[0] ) ? 1 : 0 );
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
					var str2 = interpretor.Send( "to-string:0".ToVMString(), arguments[0] ).To<VMObjects.String>();

					return str1.Concat( str2 ).To<VMObjects.AppObject>();
				}
			}
		}
	}
}
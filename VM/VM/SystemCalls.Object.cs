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
			public static Handle<VMObjects.AppObject> GetType( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
				return null;
			}

			[SystemCallMethod( "to-string:0" )]
			public static Handle<VMObjects.AppObject> ToString( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
				return receiver.Class().Name().To<VMObjects.AppObject>();
			}


			[SystemCallMethod( "equals:1" )]
			public static Handle<VMObjects.AppObject> Equals( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
				return new IntHandle( receiver.Equals( arguments[0] ) ? 1 : 0 );
			}
		}
	}
}
﻿using System;
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
				public static Handle<VMObjects.AppObject> New( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					if (arguments[0].TypeId() != VMILLib.TypeId.Integer)
						throw new ArgumentException( "Argument should be an integer.", "initialSize" );

					var initialSize = (arguments[0] as IntHandle).Value;
					var arr = VMObjects.Array.CreateInstance( initialSize );

					return arr.To<VMObjects.AppObject>();
				}

				[SystemCallMethod( "set:2" )]
				public static Handle<AppObject> Set( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = receiver.To<VMObjects.Array>();
					arr.Set( ((IntHandle) arguments[0]).Value, arguments[1].Start, !(arguments[1] is IntHandle) );
					return null;
				}

				[SystemCallMethod( "get:1" )]
				public static Handle<AppObject> Get( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = receiver.To<VMObjects.Array>();
					var index = ((IntHandle) arguments[0]).Value;
					if (!arr.IsReference( index ))
						return new IntHandle( arr.Get( index ) );
					return (AppObject) arr.Get( index );
				}

				[SystemCallMethod( "length:0" )]
				public static Handle<AppObject> Length( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
					var arr = receiver.To<VMObjects.Array>();
					return new IntHandle( arr.Length() );
				}
			}
		}
	}
}
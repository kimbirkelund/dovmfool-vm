using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	static partial class SystemCalls {
		public static partial class Array {
			public static Handle<VMObjects.AppObject> New( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
				if (arguments[0].TypeId() != VMILLib.TypeId.Integer)
					throw new ArgumentException( "Argument should be an integer.", "initialSize" );

				var initialSize = (arguments[0] as IntHandle).Value;
				var arr = VMObjects.Array.CreateInstance( initialSize );

				return arr.To<VMObjects.AppObject>();
			}
		}
	}
}

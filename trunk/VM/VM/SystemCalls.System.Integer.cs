using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Integer" )]
			class Integer {
				[SystemCallMethod( "to-string:0" )]
				public static UValue ToString( UValue receiver, UValue[] arguments ) {
					return UValue.Ref( KnownClasses.System_String, receiver.Value.ToString().ToVMString().Start );
				}

				[SystemCallMethod( "subtract:1" )]
				public static UValue subtract( UValue receiver, UValue[] arguments ) {
					return receiver.Value - arguments[0].Value;
				}

				[SystemCallMethod( "add:1" )]
				public static UValue add( UValue receiver, UValue[] arguments ) {
					return receiver.Value + arguments[0].Value;
				}

				[SystemCallMethod( "multiply:1" )]
				public static UValue multiply( UValue receiver, UValue[] arguments ) {
					return receiver.Value * arguments[0].Value;
				}

				[SystemCallMethod( "divide:1" )]
				public static UValue divide( UValue receiver, UValue[] arguments ) {
					return receiver.Value / arguments[0].Value;
				}

				[SystemCallMethod( "modulo:1" )]
				public static UValue Modulo( UValue receiver, UValue[] arguments ) {
					return receiver.Value % arguments[0].Value;
				}
			}
		}
	}
}
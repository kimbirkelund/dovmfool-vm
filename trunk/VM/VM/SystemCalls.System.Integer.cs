using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Integer" )]
			class Integer {
				[SystemCallMethod( "to-string:0" )]
				public static UValue ToString( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return receiver.Value.ToString().ToVMString().ToUValue();
				}

				[SystemCallMethod( "subtract:1" )]
				public static UValue subtract( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return receiver.Value - arguments[0].Value;
				}

				[SystemCallMethod( "add:1" )]
				public static UValue add( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return receiver.Value + arguments[0].Value;
				}

				[SystemCallMethod( "multiply:1" )]
				public static UValue multiply( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return receiver.Value * arguments[0].Value;
				}

				[SystemCallMethod( "divide:1" )]
				public static UValue divide( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
					return receiver.Value / arguments[0].Value;
				}
			}
		}
	}
}
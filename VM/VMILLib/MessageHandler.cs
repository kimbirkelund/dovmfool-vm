using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class MessageHandler {
		public readonly VisibilityModifier Visibility;
		public readonly CString Name;
		public readonly IList<string> Arguments;
		public readonly IList<string> Locals;
		public readonly InstructionList Instructions;
		public readonly bool IsEntrypoint;

		Class @class;
		public Class Class {
			get { return @class; }
			internal set {
				if (@class != null)
					throw new InvalidOperationException( "Class already set." );
				this.@class = value;
			}
		}

		public MessageHandler( VisibilityModifier visibility, CString name, IEnumerable<string> arguments, IEnumerable<string> locals, InstructionList instructions, bool isEntrypoint ) {
			this.Visibility = visibility;
			this.Name = name;
			this.Instructions = instructions;
			this.Arguments = arguments.ToList().AsReadOnly();
			this.Locals = locals.ToList().AsReadOnly();
			this.Instructions.ForEach( i => i.MessageHandler = this );
			this.IsEntrypoint = isEntrypoint;
		}

		public override string ToString() {
			return Name == null ? "Default handler" : Visibility.ToString().ToLower() + " " + Name + "(" + Arguments.Join( ", " ) + ")";
		}
	}
}

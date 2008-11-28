using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public abstract class MessageHandlerBase {
		public readonly VisibilityModifier Visibility;
		public readonly string Name;
		public readonly IList<string> Arguments;
		public abstract bool IsExternal { get; }
		public virtual bool IsEntrypoint { get { return false; } }

		Class @class;
		public Class Class {
			get { return @class; }
			internal set {
				if (@class != null)
					throw new InvalidOperationException( "Class already set." );
				this.@class = value;
			}
		}

		public MessageHandlerBase( VisibilityModifier visibility, string name, IEnumerable<string> arguments ) {
			this.Visibility = visibility;
			this.Name = name;
			this.Arguments = arguments.ToList().AsReadOnly();
		}
	}
}

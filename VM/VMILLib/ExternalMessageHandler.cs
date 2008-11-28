using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class ExternalMessageHandler : MessageHandlerBase {
		public readonly string ExternalName;
		public override bool IsExternal { get { return true; } }

		public ExternalMessageHandler( VisibilityModifier visibility, string name, string externalName, IEnumerable<string> arguments )
			: base( visibility, name, arguments ) {
			this.ExternalName = externalName;
		}

		public override string ToString() {
			return ".handler " + Visibility.ToString().ToLower() + " " + Name + " .external " + ExternalName + "(" + Arguments.Join( ", " ) + ")";
		}
	}
}

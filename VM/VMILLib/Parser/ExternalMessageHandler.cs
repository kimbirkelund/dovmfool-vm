using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class ExternalMessageHandler : MessageHandlerBase {
		public readonly string ExternalName;

		public ExternalMessageHandler( LexLocation location, VisibilityModifier visibility, string name, string externalName, List<string> arguments )
			: base( location, visibility, name, arguments ) {
			this.ExternalName = externalName;
		}
	}
}

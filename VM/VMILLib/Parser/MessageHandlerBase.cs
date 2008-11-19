using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	abstract class MessageHandlerBase : ASTNode {
		public readonly VisibilityModifier Visibility;
		public readonly string Name;
		public readonly List<string> Arguments;

		public MessageHandlerBase( LexLocation location, VisibilityModifier visibility, string name, List<string> arguments )
			: base( location ) {
			this.Visibility = visibility;
			this.Name = name;
			this.Arguments = arguments;
		}
	}
}

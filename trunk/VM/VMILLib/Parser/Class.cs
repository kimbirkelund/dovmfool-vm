using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class Class : ASTNode {
		public readonly VisibilityModifier Visibility;
		public readonly string Name;
		public readonly List<string> InheritsFrom;
		public readonly List<string> Fields;
		public readonly VMILMessageHandler DefaultHandler;
		public readonly List<MessageHandlerBase> Handlers;
		public readonly List<Class> Classes;

		public Class( LexLocation location, VisibilityModifier visibility, string name, List<string> inheritsFrom, List<string> fields,
			VMILMessageHandler defaultHandler, List<MessageHandlerBase> handlers, List<Class> classes )
			: base( location ) {
			this.Visibility = visibility;
			this.Name = name;
			this.InheritsFrom = inheritsFrom;
			this.Fields = fields;
			this.DefaultHandler = defaultHandler;
			this.Handlers = handlers;
			this.Classes = classes;
		}
	}
}

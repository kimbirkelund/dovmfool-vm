using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class ClassBody : ASTNode {
		public readonly List<string> Fields;
		public readonly VMILMessageHandler DefaultHandler;
		public readonly List<MessageHandlerBase> Handlers;
		public readonly List<Class> Classes;

		public ClassBody( List<string> fields, VMILMessageHandler defaultHandler, List<MessageHandlerBase> handlers, List<Class> classes )
			: base( new LexLocation() ) {
			this.Fields = fields;
			this.DefaultHandler = defaultHandler;
			this.Handlers = handlers;
			this.Classes = classes;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib.Parser {
	class ClassBody : ASTNode {
		public readonly List<string> Fields;
		public readonly MessageHandler DefaultHandler;
		public readonly List<MessageHandler> Handlers;
		public readonly List<Class> Classes;

		public ClassBody( List<string> fields, MessageHandler defaultHandler, List<MessageHandler> handlers, List<Class> classes )
			: base( new LexLocation() ) {
			this.Fields = fields;
			this.DefaultHandler = defaultHandler;
			this.Handlers = handlers;
			this.Classes = classes;
		}
	}
}

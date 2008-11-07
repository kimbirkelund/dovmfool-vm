using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Class {
		public readonly VisibilityModifier Visibility;
		public readonly CString Name;
		public readonly NameList InheritsFrom;
		public readonly NameList Fields;
		public readonly MessageHandler DefaultHandler;
		public readonly MessageHandlerList Handlers;
		public readonly ClassList Classes;

		public Class( VisibilityModifier visibility, CString name,NameList inheritsFrom, NameList fields, MessageHandler defaultHandler, MessageHandlerList handlers, ClassList classes ) {
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

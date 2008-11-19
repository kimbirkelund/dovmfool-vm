using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Class {
		public readonly VisibilityModifier Visibility;
		public readonly CString Name;
		public readonly NameList InheritsFrom;
		public readonly IList<string> Fields;
		public readonly MessageHandlerBase DefaultHandler;
		public readonly MessageHandlerList Handlers;
		public readonly ClassList Classes;
		public Class ParentClass { get; private set; }

		public Class( VisibilityModifier visibility, CString name, NameList inheritsFrom, IEnumerable<string> fields, MessageHandlerBase defaultHandler, MessageHandlerList handlers, ClassList classes ) {
			this.Visibility = visibility;
			this.Name = name;
			this.InheritsFrom = inheritsFrom;
			this.Fields = fields.ToList().AsReadOnly();
			this.DefaultHandler = defaultHandler;
			this.Handlers = handlers;
			this.Classes = classes;
			this.Classes.ForEach( c => c.ParentClass = this );
			if (this.DefaultHandler != null)
				this.DefaultHandler.Class = this;
			this.Handlers.ForEach( h => h.Class = this );
		}

		public override string ToString() {
			return Visibility.ToString().ToLower() + " class " + Name + (InheritsFrom.Count == 0 ? "" : " extends " + InheritsFrom.Join( ", " ));
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public sealed class Class {
		public readonly VisibilityModifier Visibility;
		public readonly string Name;
		public readonly NameList SuperClasses;
		public readonly IList<string> Fields;
		public readonly MessageHandlerBase DefaultHandler;
		public readonly MessageHandlerList Handlers;
		public readonly ClassList InnerClasses;
		public Class ParentClass { get; private set; }

		public Class( VisibilityModifier visibility, string name, NameList superClasses, IEnumerable<string> fields, MessageHandlerBase defaultHandler, MessageHandlerList handlers, ClassList classes ) {
			this.Visibility = visibility;
			this.Name = name;
			this.SuperClasses = superClasses.Count == 0 && name != "Object" && ParentClass == null ? new NameList( new[] { "Object" } ) : superClasses;
			this.Fields = fields.ToList().AsReadOnly();
			this.DefaultHandler = defaultHandler;
			this.Handlers = handlers;
			this.InnerClasses = classes;
			this.InnerClasses.ForEach( c => c.ParentClass = this );
			if (this.DefaultHandler != null)
				this.DefaultHandler.Class = this;
			this.Handlers.ForEach( h => h.Class = this );
		}

		public override string ToString() {
			return Visibility.ToString().ToLower() + " class " + Name + (SuperClasses.Count == 0 ? "" : " extends " + SuperClasses.Join( ", " ));
		}
	}
}

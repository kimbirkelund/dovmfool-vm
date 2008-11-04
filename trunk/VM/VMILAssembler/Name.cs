using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILAssembler {
	class Name : ASTNode {
		static SortedList<string,string> names =new SortedList<string,string>();

		public readonly string Name;

		Name( LexLocation location, string name )
			: base( location ) {
			this.Name = name;
		}

		public Name Create( string name ) {
			
		}
	}
}

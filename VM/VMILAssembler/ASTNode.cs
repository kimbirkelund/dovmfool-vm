using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILAssembler {
	class ASTNode {
		public readonly LexLocation Location;

		public ASTNode( LexLocation location ) {
			this.Location = location;
		}
	}
}

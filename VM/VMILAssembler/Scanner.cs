using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILAssembler {
	partial class Scanner {
		public sealed class ConstantNode : ASTNode {
			public override IEnumerable<ASTNode> Children { get { yield break; } }
			public object Value { get; set; }
		}
	}

	internal static partial class Extensions {
		public static Scanner.ConstantNode ToNode( this object value ) {
			return new Scanner.ConstantNode() { Value = value };
		}

		public static T As<T>( this ASTNode node ) {
			if (node == null)
				return default( T );
			if (node is Scanner.ConstantNode)
				return (T) ((Scanner.ConstantNode) node).Value;

			throw new ArgumentException( "Argument must be a ConstantNode" );
		}
	}
}

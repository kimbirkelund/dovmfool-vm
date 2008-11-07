using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.Logging;

namespace VMILLib.Parser {
	partial class Scanner {
		public Logger Logger { get; set; }

		public sealed class ConstantNode : ASTNode {
			public object Value { get; set; }

			public ConstantNode() : base( new LexLocation() ) { }
		}
	}

	internal static partial class Extensions {
		public static Scanner.ConstantNode ToNode( this object value ) {
			return new Scanner.ConstantNode() { Value = value };
		}

		public static T As<T>( this ASTNode node ) {
			if (node == null)
				return default( T );
			if (node is Scanner.ConstantNode) {
				Console.WriteLine( (T) ((Scanner.ConstantNode) node).Value );
				return (T) ((Scanner.ConstantNode) node).Value;
			}

			throw new ArgumentException( "Argument must be a ConstantNode, was '" + node.GetType() + "'." );
		}
	}
}

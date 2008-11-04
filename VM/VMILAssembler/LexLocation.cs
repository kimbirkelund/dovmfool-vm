using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gppg;

namespace VMILAssembler {
	/// <summary>
	/// For internal use only.
	/// </summary>
	public class LexLocation : IMerge<LexLocation> {
		/// <summary>
		/// For internal use only.
		/// </summary>
		public readonly string SourceFile;
		/// <summary>
		/// For internal use only.
		/// </summary>
		public readonly int StartLine; // start line
		/// <summary>
		/// For internal use only.
		/// </summary>
		public readonly int StartColumn; // start column
		/// <summary>
		/// For internal use only.
		/// </summary>
		public readonly int EndLine; // end line
		/// <summary>
		/// For internal use only.
		/// </summary>
		public readonly int EndColumn; // end column

		/// <summary>
		/// For internal use only.
		/// </summary>
		public LexLocation() { }

		/// <summary>
		/// For internal use only.
		/// </summary>
		public LexLocation( string sourceFile, int sl, int sc, int el, int ec ) {
			this.SourceFile = sourceFile;
			StartLine = sl;
			StartColumn = sc;
			EndLine = el;
			EndColumn = ec;
		}

		/// <summary>
		/// For internal use only.
		/// </summary>
		public LexLocation Merge( LexLocation last ) {
			if (this.SourceFile != last.SourceFile)
				throw new ArgumentException( "Argument specifies location in different file", "last" );

			return new LexLocation( SourceFile, this.StartLine, this.StartColumn, last.EndLine, last.EndColumn );
		}
	}
}

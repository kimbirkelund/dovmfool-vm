using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Security.Permissions;
using System.Diagnostics;

namespace Sekhmet.IO {
	/// <summary>
	/// Specialization of <see cref="T:TextWriter"/> for the purpose of writing structured clear-text (i.e. for writing IL).
	/// </summary>
	public class StructuredWriter : TextWriter {
		TextWriter output;
		bool newLine = true;
		int newLinePos = 0;

		/// <summary>
		/// Gets or sets the indentation level.
		/// </summary>
		/// <value>The indentation level.</value>
		public int IndentationLevel { get; set; }
		/// <summary>
		/// Gets or sets the identation string.
		/// </summary>
		/// <value>The identation string.</value>
		public string IdentationString { get; private set; }
		/// <summary>
		/// Gets or sets the line prefix.
		/// </summary>
		/// <value>The line prefix.</value>
		public string LinePrefix { get; set; }
		/// <summary>
		/// Gets or sets the line prefix level.
		/// </summary>
		/// <value>The line prefix level.</value>
		public int LinePrefixLevel { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="StructuredWriter"/> class.
		/// </summary>
		/// <param name="output">The writer handling the actual output.</param>
		/// <param name="indentationString">The indentation string.</param>
		public StructuredWriter( TextWriter output, string indentationString ) {
			this.output = output;
			this.IdentationString = indentationString;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="StructuredWriter"/> class.
		/// </summary>
		/// <param name="output">The writer handling the actual output.</param>
		public StructuredWriter( TextWriter output )
			: this( output, " " ) {
		}

		/// <summary>
		/// When overridden in a derived class, returns the <see cref="T:System.Text.Encoding"/> in which the output is written.
		/// </summary>
		/// <value></value>
		/// <returns>The Encoding in which the output is written.</returns>
		public override Encoding Encoding {
			get { return Encoding.UTF8; }
		}

		/// <summary>
		/// Writes a character to the text stream.
		/// </summary>
		/// <param name="value">The character to write to the text stream.</param>
		/// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
		/// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
		public override void Write( char value ) {
			if (newLine) {
				for (var l = 0; l < IndentationLevel; l++) {
					if (l == LinePrefixLevel)
						output.Write( LinePrefix );
					output.Write( IdentationString );
				}
				if (IndentationLevel == LinePrefixLevel)
					output.Write( LinePrefix );
				newLine = false;
			}

			output.Write( value );

			if (value == NewLine[newLinePos]) {
				newLinePos++;
				if (newLinePos == NewLine.Length) {
					newLine = true;
					newLinePos = 0;
				}
			} else
				newLinePos = 0;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="StructuredWriter"/> and optionally releases the managed resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing ) {
			base.Dispose( disposing );
			if (disposing && output != null) {
				output.Dispose();
				output = null;
			}
		}
	}
}

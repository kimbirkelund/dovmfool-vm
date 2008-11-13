using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VM {
	class ClassLoader {
		Stream input;

		public ClassLoader( Stream input ) {
			this.input = input;
		}

		public ClassLoader( string fileName ) : this( new FileStream( fileName, FileMode.Open, FileAccess.Read ) ) { }

		
	}
}

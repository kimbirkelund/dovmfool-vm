using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class String : ObjectBase {
		public override uint Size { get { return 0; } }

		public uint Length { get { return 0; } }

		internal String( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

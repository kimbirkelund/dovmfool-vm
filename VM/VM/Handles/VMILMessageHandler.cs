using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class VMILMessageHandler : MessageHandlerBase {
		public new const uint TypeId = 6;

		internal VMILMessageHandler( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

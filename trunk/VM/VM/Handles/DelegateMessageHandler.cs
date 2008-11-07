using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class DelegateMessageHandler : MessageHandlerBase {
		public new const uint TypeId = 5;

		internal DelegateMessageHandler( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

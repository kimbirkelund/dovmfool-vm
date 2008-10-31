using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class ClassManager : InternalObjectBase {
		internal ClassManager( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

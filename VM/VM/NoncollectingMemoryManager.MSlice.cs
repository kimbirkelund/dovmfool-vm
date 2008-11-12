using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class NoncollectingMemoryManager {
		class MSlice : Handle {
			public MSlice( uint start, uint size ) {
				this.memory = new uint[size];
				this.start = start;
				this.size = size;
			}
		}
	}
}

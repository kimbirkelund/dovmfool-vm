using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public partial class NoncollectingMemoryManager : MemoryManagerBase {
		List<MSlice> slices = new List<MSlice>();

		public override uint SizeInWords { get { return int.MaxValue; } }
		public override uint FreeSizeInWords { get { return uint.MaxValue - (uint) slices.Count; } }
		public override uint AllocatedSizeInWords { get { return (uint) slices.Count; } }

		internal override uint this[uint index] {
			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		public NoncollectingMemoryManager() {
		}

		public override IEnumerator<Handle> GetEnumerator() {
			return slices.OfType<Handle>().GetEnumerator();
		}

		internal override uint Allocate( uint size ) {
			var start = (uint) slices.Count;
			var slice = new MSlice( start, size );
			slices.Add( slice );
			return 0;
		}

		internal override uint Retrieve( uint position ) {
			if (position < 0 || slices.Count <= position)
				throw new ArgumentOutOfRangeException( "No object at the specified position." );

			return 0;
		}
	}
}

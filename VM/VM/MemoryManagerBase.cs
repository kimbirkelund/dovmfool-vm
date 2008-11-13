using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	internal abstract class MemoryManagerBase {
		Dictionary<int, List<HandleBase>> handles = new Dictionary<int, List<HandleBase>>();

		public abstract int SizeInWords { get; }
		public abstract int FreeSizeInWords { get; }
		public abstract int AllocatedSizeInWords { get; }

		public abstract int Allocate( int size );
		public abstract Word this[int index] { get; set; }

		public Handle<T> Register<T>( T obj ) where T : struct, IVMObject {
			var handle = new Handle<T>( obj );

			if (handles.ContainsKey( handle.Start ))
				handles[handle.Start].Add( handle );
			else
				handles.Add( handle.Start, new List<HandleBase> { handle } );

			return handle;
		}

		public void Unregister( HandleBase handle ) {
			if (handle.IsValid)
				handle.Unregister();
			else {
				if (!handles.ContainsKey( handle.Start ))
					return;

				handles[handle.Start].Remove( handle );
			}
		}

		protected void MoveHandles( int fromPosition, int toPosition ) {
			if (!handles.ContainsKey( fromPosition ))
				return;
			if (handles.ContainsKey( toPosition ))
				throw new ArgumentException( "Target position already contains a handles list. Those must be moved first.", "toPosition" );

			var l = handles[fromPosition];
			handles.Remove( fromPosition );
			handles.Add( toPosition, l );
			l.ForEach( h => h.Start = toPosition );
		}
	}
}

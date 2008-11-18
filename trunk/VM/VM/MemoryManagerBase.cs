using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract partial class MemoryManagerBase {
		Dictionary<int, List<HandleBase.HandleUpdater>> handles = new Dictionary<int, List<HandleBase.HandleUpdater>>();

		public abstract int SizeInWords { get; }
		public abstract int FreeSizeInWords { get; }
		public abstract int AllocatedSizeInWords { get; }

		internal abstract T Allocate<T>( int size ) where T : struct, IVMObject;
		internal virtual T Allocate<T>( int size, bool writeZeroes ) where T : struct, IVMObject {
			var obj = Allocate<T>( size );
			for (var i = 1; i < size + 1; i++)
				obj[i] = 0;

			return obj;
		}

		internal abstract Word this[int index] { get; set; }

		internal Handle<T> CreateHandle<T>( T obj ) where T : struct, IVMObject {
			var handle = new Handle<T>( obj );

			if (handles.ContainsKey( handle.Start ))
				handles[handle.Start].Add( handle.Updater );
			else
				handles.Add( handle.Start, new List<HandleBase.HandleUpdater> { handle.Updater } );

			return handle;
		}

		void Unregister( HandleBase handle ) {
			if (handle.IsValid)
				handle.Unregister();
			else {
				if (!handles.ContainsKey( handle.Start ))
					return;

				handles[handle.Start].Remove( handle.Updater );
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
			l.ForEach( h => h( toPosition ) );
		}
	}
}

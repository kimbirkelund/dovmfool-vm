using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public abstract partial class MemoryManagerBase {
		static Dictionary<int, List<HandleBase.HandleUpdater>> handles = new Dictionary<int, List<HandleBase.HandleUpdater>>();
		static int handlesCreated, handlesDisposed;

		public abstract int SizeInWords { get; }
		public abstract int FreeSizeInWords { get; }
		public abstract int AllocatedSizeInWords { get; }

		internal abstract T Allocate<T>( int size ) where T : struct, IVMObject<T>;
		internal virtual T Allocate<T>( int size, bool writeZeroes ) where T : struct, IVMObject<T> {
			var obj = Allocate<T>( size );
			if (writeZeroes)
				for (var i = 1; i < size + 1; i++)
					obj[i] = 0;

			return obj;
		}

		internal abstract Word this[int index] { get; set; }

		static internal Handle<T> CreateHandle<T>( T obj ) where T : struct, IVMObject<T> {
			var handle = new Handle<T>( obj );

			if (obj.Start > 0) {
				//lock (handles) {
					if (handles.ContainsKey( handle.Start ))
						handles[handle.Start].Add( handle.Updater );
					else
						handles.Add( handle.Start, new List<HandleBase.HandleUpdater> { handle.Updater } );
				//}
				handlesCreated++;
			}
			return handle;
		}

		static void Unregister( HandleBase handle ) {
			if (handle.IsValid)
				handle.Unregister();
			else {
				//lock (handles) {
					if (!handles.ContainsKey( handle.Start ))
						return;

					handles[handle.Start].Remove( handle.Updater );
					handlesDisposed++;
				//}
			}
		}

		internal static void MoveHandles( int fromPosition, int toPosition ) {
			MoveHandles( fromPosition, toPosition );
		}

		internal static void MoveHandles( int fromPosition, int toPosition, bool ignoreExistingLists ) {
			//lock (handles) {
				if (!handles.ContainsKey( fromPosition ))
					return;
				if (!ignoreExistingLists && handles.ContainsKey( toPosition ))
					throw new ArgumentException( "Target position already contains a handles list. Those must be moved first.", "toPosition" );

				var l = handles[fromPosition];
				handles.Remove( fromPosition );
				if (handles.ContainsKey( toPosition ))
					handles[toPosition].AddRange( l );
				else
					handles.Add( toPosition, l );
				l.ForEach( h => h( toPosition ) );
			//}
		}

		static void DumpHandles() {
			int total = 0;
			handles.OrderBy( p => p.Value.Count ).Select( p => {
				var cls = ((VMObjects.AppObject) p.Key).ToHandle().Class();
				total += p.Value.Count;
				if (cls == KnownClasses.SystemString.Start)
					return p.Value.Count + " => " + p.Key + " (" + ((VM.VMObjects.String) p.Key).ToString() + ")";
				return p.Value.Count + " => " + p.Key + " (" + ((VMObjects.AppObject) p.Key).ToHandle().Class() + ")";
			} ).ForEach( s => VirtualMachine.Logger.PostLine( "DEBUG", s ) );
			System.GC.Collect();
			VirtualMachine.Logger.PostLine( "DEBUG", "Handles: " + handles.Values.Aggregate( 0, ( c, l ) => c + l.Count ) );
			VirtualMachine.Logger.PostLine( "DEBUG", "Handles created: " + handlesCreated );
			VirtualMachine.Logger.PostLine( "DEBUG", "Handles disposed: " + handlesDisposed );
			VirtualMachine.Logger.PostLine( "DEBUG", "Handles alive: " + (handlesCreated - handlesDisposed) );
		}
	}
}

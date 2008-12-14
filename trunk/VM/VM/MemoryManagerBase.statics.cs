using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VM.VMObjects;

namespace VM {
	partial class MemoryManagerBase {
		public static bool MakeWeakHandles { get; set; }
		static Dictionary<int, List<HandleBase>> handles = new Dictionary<int, List<HandleBase>>();
		public static IEnumerable<int> Handles { get { return handles.Where( p => p.Value.Count > 0 ).Select( p => p.Key ); } }

		internal static Handle<T> CreateHandle<T>( T obj, bool isWeak ) where T : struct, IVMObject<T> {
			var handle = new Handle<T>( obj, isWeak || MakeWeakHandles );

			if (!handle.IsWeak) {
				lock (handles) {
					List<HandleBase> l;
					if (handles.TryGetValue( handle.Start, out l ))
						l.Add( handle );
					else {
						l = new List<HandleBase>();
						l.Add( handle );
						handles.Add( handle.Start, l );
					}
				}
				AssertHandle( handle );
			}

			return handle;
		}

		internal static void Unregister( HandleBase handle ) {
			if (handle.IsValid)
				handle.Unregister();
			else {
				lock (handles) {
					List<HandleBase> l;
					if (handles.TryGetValue( handle.Start, out l )) {
						var idx = l.FindIndex( p => object.ReferenceEquals( p, handle ) );
						if (idx != -1)
							l.RemoveAt( idx );
					}
				}
			}
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		internal static void AssertHandle( HandleBase h ) {
#if DEBUG
			if (!h.IsValid)
				throw new global::System.ArgumentException( "Should only be called with valid handles." );
			if (h.IsWeak)
				throw new global::System.ArgumentException( "Should only be called with non-weak handles." );
			lock (handles) {
				Sekhmet.Assert.IsTrue( handles.ContainsKey( h.Start ) );
				Sekhmet.Assert.IsTrue( handles[h.Start].Any( ih => ih.id == h.id ) );
			}
#endif
		}

		internal static void MoveHandles( int fromPosition, int toPosition ) {
			MoveHandles( fromPosition, toPosition );
		}

		internal static void MoveHandles( int fromPosition, int toPosition, bool ignoreExistingLists ) {
			if (!handles.ContainsKey( fromPosition ))
				return;
			if (!ignoreExistingLists && handles.ContainsKey( toPosition ))
				throw new ArgumentException( "Target position already contains a handles list. Those must be moved first.".ToVMString().ToHandle(), "toPosition".ToVMString().ToHandle() );

			var l = handles[fromPosition];
			handles.Remove( fromPosition );
			if (handles.ContainsKey( toPosition ))
				handles[toPosition].AddRange( l );
			else
				handles.Add( toPosition, l );
			l.ForEach( h => h.Start = toPosition );
		}

		//static void DumpHandles() {
		//    int total = 0;
		//    handles.OrderBy( p => p.Value.Count ).Select( p => {
		//        var cls = ((VMObjects.AppObject) p.Key).ToHandle().Class();
		//        total += p.Value.Count;
		//        if (cls == KnownClasses.System_String.Start)
		//            return p.Value.Count + " => " + p.Key + " (" + ((VM.VMObjects.String) p.Key).ToString() + ")";
		//        return p.Value.Count + " => " + p.Key + " (" + ((VMObjects.AppObject) p.Key).ToHandle().Class() + ")";
		//    } ).ForEach( s => VirtualMachine.Logger.PostLine( "DEBUG", s ) );
		//    System.GC.Collect();
		//    VirtualMachine.Logger.PostLine( "DEBUG", "Handles: " + handles.Values.Aggregate( 0, ( c, l ) => c + l.Count ) );
		//    VirtualMachine.Logger.PostLine( "DEBUG", "Handles created: " + handlesCreated );
		//    VirtualMachine.Logger.PostLine( "DEBUG", "Handles disposed: " + handlesDisposed );
		//    VirtualMachine.Logger.PostLine( "DEBUG", "Handles alive: " + (handlesCreated - handlesDisposed) );
		//}
	}
}

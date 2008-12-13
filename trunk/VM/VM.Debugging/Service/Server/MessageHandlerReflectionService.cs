using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM.Debugging.Service.Server {
	static class MessageHandlerReflectionService {
		static int nextId = 1;
		static Dictionary<int, Handle<MessageHandlerBase>> idToMH = new Dictionary<int, Handle<MessageHandlerBase>>();
		static Dictionary<Handle<MessageHandlerBase>, int> mhToId = new Dictionary<Handle<MessageHandlerBase>, int>();

		static int Add( Handle<MessageHandlerBase> handler ) {
			var id = nextId++;
			idToMH.Add( id, handler );
			mhToId.Add( handler, id );
			return id;
		}

		public static Handle<MessageHandlerBase> Get( int id ) {
			Handle<MessageHandlerBase> mh;
			if (!idToMH.TryGetValue( id, out mh ))
				throw new System.ArgumentOutOfRangeException( "id" );
			return mh;
		}

		public static int Get( Handle<MessageHandlerBase> h ) {
			int id;
			if (!mhToId.TryGetValue( h, out id ))
				id = Add( h );
			return id;
		}

		public static string Name( int id ) {
			return Get( id ).Name().ToString();
		}

		public static VMILLib.VisibilityModifier Visibility( int id ) {
			return Get( id ).Visibility();
		}

		public static bool IsExternal( int id ) {
			return Get( id ).IsExternal();
		}

		public static bool IsDefault( int id ) {
			return Get( id ).Name().IsNull();
		}

		public static bool IsEntrypoint( int id ) {
			return Get( id ).IsEntrypoint();
		}

		public static int ArgumentCount( int id ) {
			return Get( id ).ArgumentCount();
		}

		public static int Class( int id ) {
			return ClassReflectionService.Get( Get( id ).Class().ToWeakHandle() );
		}

		public static int LocalCount( int id ) {
			var h = Get( id );
			if (!h.IsExternal())
				using (var hH = h.To<VMILMessageHandler>())
					return hH.LocalCount();
			return 0;
		}
	}
}

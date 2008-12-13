using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM.Debugging.Service.Server {
	class ObjectReflectionService {
		static int nextId = 1;
		static Dictionary<int, Handle<AppObject>> idToObj = new Dictionary<int, Handle<AppObject>>();
		static Dictionary<Handle<AppObject>, int> objToId = new Dictionary<Handle<AppObject>, int>();

		static int Add( Handle<AppObject> obj ) {
			if (!obj.IsWeak)
				obj = obj.Value.ToWeakHandle();
			var id = nextId++;
			idToObj.Add( id, obj );
			objToId.Add( obj, id );
			return id;
		}

		public static int Get( Handle<AppObject> obj ) {
			int id;
			if (!objToId.TryGetValue( obj, out id ))
				id = Add( obj );
			return id;
		}

		public static Handle<AppObject> Get( int id ) {
			Handle<AppObject> obj;
			if (!idToObj.TryGetValue( id, out obj ))
				throw new System.ArgumentOutOfRangeException( "id" );
			return obj;
		}

		public Value GetField( int objectId, int classId, int index ) {
			var obj = Get( objectId );
			var cls = ClassReflectionService.Get( classId );
			var val = obj.GetField( obj.GetFieldOffset( cls ) + index ).ToWeakHandle();

			if (val is IntHandle)
				return new Value { Type = ValueType.Integer, Data = val.Start };
			return new Value {
				Type = ValueType.Object,
				Class = ClassReflectionService.Get( val.Class().ToWeakHandle() ),
				Data = Get( val )
			};
		}

		public int Class( int objectId ) {
			return ClassReflectionService.Get( Get( objectId ).Class().ToWeakHandle() );
		}
	}
}

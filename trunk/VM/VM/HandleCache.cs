using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	internal class HandleCache<T> where T : struct, IVMObject<T> {
		Dictionary<Word, Handle<T>> handles = new Dictionary<Word, Handle<T>>();

		public Handle<T> this[Word value] {
			get {
				Handle<T> v;
				if (!handles.TryGetValue( value, out v )) {
					v = New(value);
					handles.Add( value, v );
					return v;
				}
				return v;
			}
		}

		public void Clear() {
			handles.Values.ForEach( h => h.Unregister() );
			handles.Clear();
		}

		protected virtual Handle<T> New(Word v){
			return new T().New(v).ToHandle();
		}
	}

	internal class IntHandleCache : HandleCache<AppObject> {
		protected override Handle<AppObject> New( Word v ) {
			return new IntHandle( v );
		}
	}
}

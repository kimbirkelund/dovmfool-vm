using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public interface IVMObject<T> where T : struct, IVMObject<T> {
		int Start { get; }
		T New( int start );
		Handle<Class> VMClass { get; }
		bool Equals( Handle<T> obj1, Handle<T> obj2 );
		Word this[int index] { get; set; }
	}
}

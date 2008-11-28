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
	}
}

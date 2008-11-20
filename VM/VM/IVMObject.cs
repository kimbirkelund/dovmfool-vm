using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM {
	public interface IVMObject<T> where T : struct, IVMObject<T> {
		int Start { get; }
		Word this[int index] { get; set; }
		int Size { get; }
		TypeId TypeId { get; }
		T New( int startPosition );
		bool IsNull { get; }
	}

	public static class ExtIVMObject {
		public static Handle<T> ToHandle<T>( this T value ) where T : struct, IVMObject<T> {
			if (value.Start == 0)
				return null;
			return (Handle<T>) value;
		}
	}
}

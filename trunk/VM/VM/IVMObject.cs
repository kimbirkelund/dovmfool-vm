using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM {
	public interface IVMObject {
		int Start { get; set; }
		Word this[int index] { get; set; }
		int Size { get; }
		TypeId TypeId { get; }
	}

	public static class ExtIVMObject {
		public static Handle<T> ToHandle<T>( this T value ) where T : struct, IVMObject {
			if (value.Start == 0)
				return null;
			return (Handle<T>) value;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public abstract class ObjectBase : HandleBase {
		public const uint OBJECT_TYPE_MASK = 0x00000100;

		/// <summary>
		/// Get the object type stored in the header. Constant time operation.
		/// </summary>
		public uint ObjectType {
			get { return this[0] & OBJECT_TYPE_MASK; }
		}

		/// <summary>
		/// Gets a value indicating if <paramref name="h"/> is an application object. Constant time operation.
		/// </summary>
		public bool IsAppObject {
			get { return this.ObjectType == AppObject.TypeId; }
		}

		/// <summary>
		/// Gets a value indicating if <paramref name="h"/> is an application object set. Constant time operation.
		/// </summary>
		public bool IsAppObjectSet {
			get { return this.ObjectType == AppObjectSet.TypeId; }
		}

		/// <summary>
		/// Gets a value indicating if <paramref name="h"/> is an internal object. Constant time operation.
		/// </summary>
		public bool IsInternalObject {
			get { return this.ObjectType >= InternalObjectBase.TypeId; }
		}

		/// <summary>
		/// Gets the total size of <paramref name="h"/> in words. Constant time, but will possibly create new object.
		/// </summary>
		public abstract uint Size { get; }

		protected ObjectBase( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }
	}
}

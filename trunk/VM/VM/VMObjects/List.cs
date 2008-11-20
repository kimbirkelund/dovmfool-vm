using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct List : IVMObject<List> {
		#region Constants
		public const int ARRAY_SIZE_OFFSET = 1;
		public const int LIST_COUNT_OFFSET = 2;
		public const int ARRAY_OFFSET = 3;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return VMILLib.TypeId.List; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
		}

		public int Count {
			get { return this[LIST_COUNT_OFFSET]; }
			private set { this[LIST_COUNT_OFFSET] = value; }
		}

		public int Capacity {
			get { return this[ARRAY_SIZE_OFFSET]; }
			private set { this[ARRAY_SIZE_OFFSET] = value; }
		}
		#endregion

		#region Cons
		public List( int start ) {
			this.start = start;
		}

		public List New( int startPosition ) {
			return new List( startPosition );
		}
		#endregion

		#region Casts
		public static implicit operator int( List list ) {
			return list.start;
		}

		public static explicit operator List( int list ) {
			return new List { start = list };
		}
		#endregion

		#region Instance methods
		public int Add<T>( T obj ) where T : struct, IVMObject<T> {
			if (Count >= Capacity)
				Expand();

			((Array) this[ARRAY_OFFSET]).Set( Count++, obj );
			return Count - 1;
		}

		public void Set<T>( int index, T obj ) where T : struct, IVMObject<T> {
			if (index >= Count)
				throw new ArgumentOutOfBoundsException( "index" );

			((Array) this[ARRAY_OFFSET]).Set( index, obj );
		}

		public T Get<T>( int index ) where T : struct, IVMObject<T> {
			if (index >= Count)
				throw new ArgumentOutOfBoundsException( "index" );

			return ((Array) this[ARRAY_OFFSET]).Get<T>( index );
		}

		public void Remove<T>( T obj ) where T : struct, IVMObject<T> {
			var index = IndexOf( obj );
			if (index != -1)
				RemoveAt( index );
		}

		public void RemoveAt( int index ) {
			if (index < 0 || Count <= index)
				throw new ArgumentOutOfBoundsException( "index" );

			Array.CopyTo( (Array) this[ARRAY_OFFSET], index + 1, (Array) this[ARRAY_OFFSET], index, Count - index - 1 );
		}

		public void InsertAt<T>( int index, T obj ) where T : struct, IVMObject<T> {
			if (index < 0 || Count < index)
				throw new ArgumentOutOfBoundsException( "index" );

			Add( Get<ObjectBase>( Count - 1 ) );
			for (int i = Count - 2; i >= index; i--)
				Set( i + 1, Get<ObjectBase>( i ) );

			Set( index, obj );
		}

		public int IndexOf<T>( T obj ) where T : struct, IVMObject<T> {
			for (var i = 0; i < Count; i++)
				if (Get<ObjectBase>( i ).Start == obj.Start)
					return i;

			return -1;
		}

		public Array ToArray() {
			Trim();
			return (Array) this[ARRAY_OFFSET];
		}

		public void Trim() {
			if (Capacity == Count)
				return;
			var oldArr = (Array) this[ARRAY_OFFSET];
			var newArr = Array.CreateInstance( Count );
			Array.CopyTo( oldArr, 0, newArr, 0, Count );

			this[ARRAY_OFFSET] = newArr;
			Capacity = Count;
		}

		void Expand() {
			var oldArr = (Array) this[ARRAY_OFFSET];
			var newArr = Array.CreateInstance( Capacity * 2 );
			this[ARRAY_OFFSET] = newArr;
			Capacity *= 2;

			Array.CopyTo( oldArr, 0, newArr, 0, Count );
		}

		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return "List{Count: " + Count + "}";
		}
		#endregion

		#region Static method
		public static List CreateInstance( int initialSize ) {
			var list = VirtualMachine.MemoryManager.Allocate<List>( 3 );

			list[ARRAY_SIZE_OFFSET] = initialSize;
			list[LIST_COUNT_OFFSET] = 0;
			list[ARRAY_OFFSET] = Array.CreateInstance( initialSize );

			return list;
		}

		public static List CreateInstance() {
			return CreateInstance( 8 );
		}
		#endregion
	}
}

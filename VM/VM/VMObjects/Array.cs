using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class ArrayConsts {
		public const int ELEMENT_COUNT_OFFSET = 1;
		public const int FIRST_ELEMENT_OFFSET = 2;
	}

	public struct Array : IVMObject<Array> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.System_Array; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}
		#endregion

		#region Cons
		public Array( int start ) {
			this.start = start;
		}

		public Array New( int start ) {
			return new Array( start );
		}
		#endregion

		#region Casts
		public static implicit operator int( Array arr ) {
			return arr.start;
		}

		public static explicit operator Array( int arr ) {
			return new Array { start = arr };
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			return ExtArray.ToString( this.ToHandle() );
		}

		public bool Equals( Handle<Array> obj1, Handle<Array> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion

		#region Static methods
		public static Array CreateInstance( int elementCount ) {
			var arr = VirtualMachine.MemoryManager.Allocate<Array>( ArrayConsts.FIRST_ELEMENT_OFFSET - 1 + elementCount * 2 );
			arr[ArrayConsts.ELEMENT_COUNT_OFFSET] = elementCount;
			elementCount.ForEach( i => arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2] = arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2 + 1] = 0 );
			return arr;
		}

		public static void Copy( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString() );
			if (sourceArray.Length() < sourceIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString() );
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString() );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString() );

			var sourceOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + sourceIndex * 2;
			var destOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + destinationIndex * 2;
			count.ForEach( i => {
				destinationArray[destOffset + i * 2] = sourceArray[sourceOffset + i * 2];
				destinationArray[destOffset + i * 2 + 1] = sourceArray[sourceOffset + i * 2 + 1];
			} );
		}

		public static void CopyDescending( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString() );
			if (sourceArray.Length() <= sourceIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString() );
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString() );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString() );

			var sourceOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + sourceIndex * 2;
			var destOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + destinationIndex * 2;
			for (int i = count - 1; i >= 0; i--) {
				destinationArray[destOffset + i * 2] = sourceArray[sourceOffset + i * 2];
				destinationArray[destOffset + i * 2 + 1] = sourceArray[sourceOffset + i * 2 + 1];
			}
		}
		#endregion
	}

	public static class ExtArray {
		public static int Length( this Handle<Array> obj ) {
			return obj[ArrayConsts.ELEMENT_COUNT_OFFSET];
		}

		internal static UValue Get( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString() );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == KnownClasses.System_Integer.Start)
				return UValue.Int( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 && obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return UValue.Null();
			return UValue.Ref( (Class) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2], obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
		}

		public static Handle<T> Get<T>( this Handle<Array> obj, int arrayIndex ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString() );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 && obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return null;
			return new T().New( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] ).ToHandle();
		}

		public static Class GetType( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString() );

			return (Class) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2];
		}

		public static void Set<T>( this Handle<Array> obj, int arrayIndex, Handle<T> value ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString() );

			if (value == null) {
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = 0;
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = 0;
			} else {
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = value.Class().Start;
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = value.Start;
			}
		}

		internal static void Set( this Handle<Array> obj, int arrayIndex, UValue value ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString() );

			obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = value.Type;
			obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = value.Value;
		}

		public static string ToString( this Handle<Array> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Array{Length: " + obj.Length() + "}";
		}

		public static Array ToVMArray<T>( this List<Handle<T>> list ) where T : struct, IVMObject<T> {
			var arr = Array.CreateInstance( list.Count ).ToHandle();
			list.ForEach( ( e, i ) => arr.Set( i, e ) );
			return arr;
		}

		public static IEnumerable<Handle<T>> GetEnumerator<T>( this Handle<Array> obj ) where T : struct, IVMObject<T> {
			for (int i = 0; i < obj.Length(); i++)
				yield return obj.Get<T>( i );
		}

		internal static IEnumerable<UValue> GetEnumerator( this Handle<Array> obj ) {
			for (int i = 0; i < obj.Length(); i++)
				yield return obj.Get( i );
		}
	}
}

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
		public Handle<Class> VMClass { get { return KnownClasses.SystemArray; } }
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
			return ExtArray.ToString( this );
		}
		#endregion

		#region Static methods
		public static Handle<Array> CreateInstance( int elementCount ) {
			var arr = VirtualMachine.MemoryManager.Allocate<Array>( ArrayConsts.FIRST_ELEMENT_OFFSET - 1 + elementCount * 2 );
			arr[ArrayConsts.ELEMENT_COUNT_OFFSET] = elementCount;
			elementCount.ForEach( i => arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2] = arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2 + 1] = 0 );
			return arr;
		}

		public static void Copy( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (sourceArray.Length() < sourceIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );
			if (destinationIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );

			var sourceOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + sourceIndex * 2;
			var destOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + destinationIndex * 2;
			count.ForEach( i => {
				destinationArray[destOffset + i * 2] = sourceArray[sourceOffset + i * 2];
				destinationArray[destOffset + i * 2 + 1] = sourceArray[sourceOffset + i * 2 + 1];
			} );
		}

		public static void CopyDescending( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (sourceArray.Length() <= sourceIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );
			if (destinationIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );

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

		public static Handle<AppObject> Get( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 || obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return null;
			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == KnownClasses.SystemInteger.Start)
				return new IntHandle( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
			return (AppObject) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1];
		}

		public static Handle<T> Get<T>( this Handle<Array> obj, int arrayIndex ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 || obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return null;
			return new T().New( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
		}

		public static Handle<Class> GetType( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			return (Class) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2];
		}

		public static void Set<T>( this Handle<Array> obj, int arrayIndex, Handle<T> value ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			if (value == null) {
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = 0;
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = 0;
			} else {
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = value.Class().Start;
				obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = value.Start;
			}
		}

		public static string ToString( this Handle<Array> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Array{Length: " + obj.Length() + "}";
		}

		public static Handle<Array> ToVMArray<T>( this List<Handle<T>> list ) where T : struct, IVMObject<T> {
			var arr = Array.CreateInstance( list.Count );
			list.ForEach( ( e, i ) => arr.Set( i, e ) );
			return arr;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct Array : IVMObject<Array> {
		#region Constants
		public const int FIRST_ELEMENT_OFFSET_OFFSET = 1;
		public const int FIRST_MAP_WORD_OFFSET = 2;
		#endregion

		#region Properties
		int start;
		public int Start { get { return start; } }

		public TypeId TypeIdAtInstancing { get { return TypeId.Array; } }
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
			var wordCount = (elementCount + 31) / 32;
			var arr = VirtualMachine.MemoryManager.Allocate<Array>( Array.FIRST_MAP_WORD_OFFSET - 1 + wordCount + elementCount );
			arr[Array.FIRST_ELEMENT_OFFSET_OFFSET] = wordCount + Array.FIRST_MAP_WORD_OFFSET;

			return arr;
		}

		public static void Copy( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (sourceArray.Length() <= sourceIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );
			if (destinationIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );

			var sourceOffset = sourceArray[Array.FIRST_ELEMENT_OFFSET_OFFSET] + sourceIndex;
			var destOffset = destinationArray[Array.FIRST_ELEMENT_OFFSET_OFFSET] + destinationIndex;
			count.ForEach( i => {
				destinationArray[destOffset + i] = sourceArray[sourceOffset + i];
				destinationArray.SetReference( destinationIndex + i, sourceArray.IsReference( sourceIndex + i ) );
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

			var sourceOffset = sourceArray[Array.FIRST_ELEMENT_OFFSET_OFFSET] + sourceIndex;
			var destOffset = destinationArray[Array.FIRST_ELEMENT_OFFSET_OFFSET] + destinationIndex;
			for (int i = count - 1; i >= 0; i--) {
				destinationArray[destOffset + i] = sourceArray[sourceOffset + i];
				destinationArray.SetReference( destinationIndex + i, sourceArray.IsReference( sourceIndex + i ) );
			}
		}
		#endregion
	}

	public static class ExtArray {
		public static int Length( this Handle<Array> obj ) {
			return obj.Size() - obj[Array.FIRST_ELEMENT_OFFSET_OFFSET];
		}

		public static Word Get( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			return obj[obj[Array.FIRST_ELEMENT_OFFSET_OFFSET] + arrayIndex];
		}

		public static void Set( this Handle<Array> obj, int arrayIndex, Word value, bool isReference ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			obj[obj[Array.FIRST_ELEMENT_OFFSET_OFFSET] + arrayIndex] = value;
			obj.SetReference( arrayIndex, isReference );
		}

		public static bool IsReference( this Handle<Array> obj, int arrayIndex ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;
			return (obj[Array.FIRST_MAP_WORD_OFFSET + word] & (1 << bit)) != 0;
		}

		internal static void SetReference( this Handle<Array> obj, int arrayIndex, bool isReference ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;

			if (isReference)
				obj[Array.FIRST_MAP_WORD_OFFSET + word] = obj[Array.FIRST_MAP_WORD_OFFSET + word] | (1 << bit);
			else
				obj[Array.FIRST_MAP_WORD_OFFSET + word] = obj[Array.FIRST_MAP_WORD_OFFSET + word] ^ (1 << bit);
		}

		public static string ToString( this Handle<Array> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Array{Length: " + obj.Length() + "}";
		}
	}
}

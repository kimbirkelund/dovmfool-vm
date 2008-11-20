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
			set { start = value; }
		}

		public int Length { get { return Size - this[FIRST_ELEMENT_OFFSET_OFFSET]; } }
		#endregion

		#region Cons
		public Array( int start ) {
			this.start = start;
		}

		public Array New( int startPosition ) {
			return new Array( startPosition );
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
		public T Get<T>( int arrayIndex ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || Length <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			return new T().New( this[this[FIRST_ELEMENT_OFFSET_OFFSET] + arrayIndex] );
		}

		public void Set<T>( int arrayIndex, T obj ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || Length <= arrayIndex)
				throw new ArgumentOutOfBoundsException( "arrayIndex" );

			this[this[FIRST_ELEMENT_OFFSET_OFFSET] + arrayIndex] = obj.Start;
			SetReference( arrayIndex, obj.TypeId != TypeId.Integer );
		}

		bool IsReference( int arrayIndex ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;
			return (this[FIRST_MAP_WORD_OFFSET + word] & (1 << bit)) != 0;
		}

		void SetReference( int arrayIndex, bool isReference ) {
			var word = arrayIndex / 32;
			var bit = arrayIndex % 32;

			if (isReference)
				this[FIRST_MAP_WORD_OFFSET + word] = this[FIRST_MAP_WORD_OFFSET + word] | (1 << bit);
			else
				this[FIRST_MAP_WORD_OFFSET + word] = this[FIRST_MAP_WORD_OFFSET + word] ^ (1 << bit);
		}

		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return "Array{Length: " + Length + "}";
		}
		#endregion

		#region Static methods
		public static Array CreateInstance( int elementCount ) {
			var wordCount = (elementCount + 31) / 32;
			var arr = VirtualMachine.MemoryManager.Allocate<Array>( FIRST_MAP_WORD_OFFSET - 1 + wordCount + elementCount );
			arr[FIRST_ELEMENT_OFFSET_OFFSET] = wordCount + FIRST_MAP_WORD_OFFSET;

			return arr;
		}

		public static void CopyTo( Array sourceArray, int sourceIndex, Array destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (sourceArray.Length <= sourceIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );
			if (destinationIndex < 0)
				throw new ArgumentOutOfBoundsException( "sourceIndex" );
			if (destinationArray.Length < destinationIndex + count)
				throw new ArgumentOutOfBoundsException( "count" );

			var sourceOffset = sourceArray[FIRST_ELEMENT_OFFSET_OFFSET] + sourceIndex;
			var destOffset = destinationArray[FIRST_ELEMENT_OFFSET_OFFSET] + destinationIndex;
			count.ForEach( i => {
				destinationArray[destOffset + i] = sourceArray[sourceOffset + i];
				destinationArray.SetReference( destinationIndex + i, sourceArray.IsReference( sourceIndex + i ) );
			} );
		}
		#endregion
	}
}

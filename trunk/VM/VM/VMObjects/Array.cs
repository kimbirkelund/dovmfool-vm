using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class ArrayConsts {
		public const int ELEMENT_COUNT_OFFSET = 0;
		public const int FIRST_ELEMENT_OFFSET = 1;
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
			return new Array( arr );
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			using (var hThis = this.ToHandle())
				return ExtArray.ToString( hThis );
		}

		public bool Equals( Handle<Array> obj1, Handle<Array> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion

		#region Static methods
		public static Array CreateInstance( int elementCount ) {
			var arr = VirtualMachine.MemoryManager.Allocate<Array>( ArrayConsts.FIRST_ELEMENT_OFFSET + elementCount * 2 );
			arr[ArrayConsts.ELEMENT_COUNT_OFFSET] = elementCount;
			elementCount.ForEach( i => arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2] = arr[ArrayConsts.FIRST_ELEMENT_OFFSET + i * 2 + 1] = 0 );
			return arr;
		}

		public static void Copy( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString().ToHandle() );
			if (sourceArray.Length() < sourceIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString().ToHandle() );
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString().ToHandle() );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString().ToHandle() );

			var sourceOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + sourceIndex * 2;
			var destOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + destinationIndex * 2;
			count.ForEach( i => {
				destinationArray[destOffset + i * 2] = sourceArray[sourceOffset + i * 2];
				destinationArray[destOffset + i * 2 + 1] = sourceArray[sourceOffset + i * 2 + 1];
			} );
		}

		public static void CopyDescending( Handle<Array> sourceArray, int sourceIndex, Handle<Array> destinationArray, int destinationIndex, int count ) {
			if (sourceIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString().ToHandle() );
			if (sourceArray.Length() <= sourceIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString().ToHandle() );
			if (destinationIndex < 0)
				throw new ArgumentOutOfRangeException( "sourceIndex".ToVMString().ToHandle() );
			if (destinationArray.Length() < destinationIndex + count)
				throw new ArgumentOutOfRangeException( "count".ToVMString().ToHandle() );

			var sourceOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + sourceIndex * 2;
			var destOffset = ArrayConsts.FIRST_ELEMENT_OFFSET + destinationIndex * 2;
			for (int i = count - 1; i >= 0; i--) {
				destinationArray[destOffset + i * 2] = sourceArray[sourceOffset + i * 2];
				destinationArray[destOffset + i * 2 + 1] = sourceArray[sourceOffset + i * 2 + 1];
			}
		}

		internal static int[] GetReferences( int adr ) {
			int length = VirtualMachine.MemoryManager[adr + ArrayConsts.ELEMENT_COUNT_OFFSET];

			List<int> refs = new List<int>();
			for (int i = 0; i < length; i += 2) {
				int cls = VirtualMachine.MemoryManager[adr + ArrayConsts.FIRST_ELEMENT_OFFSET + i];
				if (cls < 0)
					cls = KnownClasses.Resolve( cls ).Start;
				if (cls != 0)
					refs.Add( cls );
				if (cls != KnownClasses.System_Integer.Start) {
					var val = VirtualMachine.MemoryManager[adr + ArrayConsts.FIRST_ELEMENT_OFFSET + i + 1];
					if (val != 0)
						refs.Add( val );
				}
			}
			return refs.ToArray();
		}
		#endregion
	}

	public static class ExtArray {
		public static int Length( this Handle<Array> obj ) {
			return obj[ArrayConsts.ELEMENT_COUNT_OFFSET];
		}

		public static int IndexOf( this Handle<Array> obj, Handle<AppObject> element ) {
			for (int i = 0; i < obj.Length(); i++) {
				UValue v = obj.Get( i );
				Handle<AppObject> h = ExtUValue.ToHandle( v );
				if (h.Send( KnownStrings.equals_1, element ).Value > 0) return i;
			}

			return -1;
		}

		internal static UValue Get( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString().ToHandle() );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == KnownClasses.System_Integer.Start)
				return UValue.Int( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 && obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return UValue.Null();
			return UValue.Ref( (Class) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2], obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
		}

		public static T Get<T>( this Handle<Array> obj, int arrayIndex ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString().ToHandle() );

			if (obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] == 0 && obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] == 0)
				return new T().New( 0 );
			return new T().New( obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] );
		}

		public static Class GetType( this Handle<Array> obj, int arrayIndex ) {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString().ToHandle() );

			return (Class) obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2];
		}

		public static void Set<T>( this Handle<Array> obj, int arrayIndex, Handle<T> value ) where T : struct, IVMObject<T> {
			if (arrayIndex < 0 || obj.Length() <= arrayIndex)
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString().ToHandle() );

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
				throw new ArgumentOutOfRangeException( "arrayIndex".ToVMString().ToHandle() );

			obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2] = value.Type;
			obj[ArrayConsts.FIRST_ELEMENT_OFFSET + arrayIndex * 2 + 1] = value.Value;
		}

		public static string ToString( this Handle<Array> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Array{Length: " + obj.Length() + "}";
		}

		public static Array ToVMArray<T>( this List<Handle<T>> list ) where T : struct, IVMObject<T> {
			using (var arr = Array.CreateInstance( list.Count ).ToHandle()) {
				list.ForEach( ( e, i ) => arr.Set( i, e ) );
				return arr.Value;
			}
		}

		public static IEnumerable<T> GetEnumerator<T>( this Handle<Array> obj ) where T : struct, IVMObject<T> {
			for (int i = 0; i < obj.Length(); i++)
				yield return obj.Get<T>( i );
		}

		internal static IEnumerable<UValue> GetEnumerator( this Handle<Array> obj ) {
			for (int i = 0; i < obj.Length(); i++)
				yield return obj.Get( i );
		}
	}
}

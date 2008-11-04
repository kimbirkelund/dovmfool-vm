using System;
using coll = System.Collections;
using gen = System.Collections.Generic;
using System.Text;

namespace Sekhmet.Collections {
    public partial class List<T> {
        class ArrayWrapper<TArray> : gen.IList<TArray>, gen.ICollection<TArray> {
            public TArray[] arr;
            public int count;
            int initialSize;
            bool changed;

            ArrayWrapper( TArray[] arr ) {
                this.arr = arr;
                count = 0;
                this.initialSize = arr.Length;
            }

            public ArrayWrapper( int initialSize )
                : this( new TArray[initialSize] ) {
            }

            public ArrayWrapper()
                : this( 1 ) {
            }

            void TrimSize() {
                if (this.count >= this.arr.Length)
                    Array.Resize<TArray>( ref arr, Math.Max( this.count * 2, 1 ) );
                else if (this.count < this.arr.Length / 2 && this.count > 0)
                    Array.Resize<TArray>( ref arr, this.count );
            }

            public void TrimExcess() {
                if (this.count >= arr.Length * 0.9)
                    return;

                if (this.count == 0)
                    arr = new TArray[1];
                else
                    Array.Resize<TArray>( ref arr, this.count );
            }

            public List<TOutput>.ArrayWrapper<TOutput> ConvertAll<TOutput>( Converter<TArray, TOutput> converter ) {
                return new List<TOutput>.ArrayWrapper<TOutput>( Array.ConvertAll<TArray, TOutput>( arr, converter ) );
            }

            #region IList<TArr> Members

            public int IndexOf( TArray item ) {
                return Array.IndexOf<TArray>( arr, item, 0, count );
            }

            public void Insert( int index, TArray item ) {
                if (index > this.count)
                    throw new ArgumentOutOfRangeException( "index" );

                TrimSize();

                for (int i = count; i > index; i--)
                    this.arr[i] = this.arr[i - 1];

                this.arr[index] = item;
                this.count++;

                changed = true;
            }

            public void RemoveAt( int index ) {
                if (index >= this.count)
                    throw new ArgumentOutOfRangeException( "index" );
                for (int i = index; i < this.count - 1; i++)
                    arr[i] = arr[i + 1];
                count--;
                TrimSize();

                changed = true;
            }

            public TArray this[int index] {
                get {
                    if (index >= this.count)
                        throw new ArgumentOutOfRangeException( "index" );
                    return arr[index];
                }
                set {
                    if (index >= this.count)
                        throw new ArgumentOutOfRangeException( "index" );
                    arr[index] = value;
                }
            }

            #endregion

            #region ICollection<TArr> Members

            public void Add( TArray item ) {
                this.Insert( this.count, item );
            }

            public void Clear() {
                arr = new TArray[initialSize];
                count = 0;

                changed = true;
            }

            public bool Contains( TArray item ) {
                return this.IndexOf( item ) != -1;
            }

            public void CopyTo( TArray[] array, int arrayIndex ) {
                if (array == null)
                    throw new ArgumentNullException( "array" );
                if (arrayIndex < 0)
                    throw new ArgumentOutOfRangeException( "arrayIndex" );
                if (array.Rank != 1)
                    throw new ArgumentException( "Array can't be multi-dimensional.", "rank" );
                if (arrayIndex >= array.Length)
                    throw new ArgumentOutOfRangeException( "arrayIndex" );
                if (array.Length - arrayIndex < this.count)
                    throw new ArgumentException( "Array is too small to hold all elements.", "array" );

                for (int i = 0; i < this.count; i++)
                    array[i + arrayIndex] = arr[i];
            }

            public int Count {
                get { return this.count; }
            }

            public bool IsReadOnly {
                get { return false; }
            }

            public bool Remove( TArray item ) {
                int index = this.IndexOf( item );
                if (index != -1)
                    RemoveAt( index );

                return index != -1;
            }

            #endregion

            #region IEnumerable<TArr> Members

            public gen.IEnumerator<TArray> GetEnumerator() {
                changed = false;
                for (int i = 0; i < this.count; i++) {
                    if (changed)
                        throw new InvalidOperationException( "Collection was modified; enumeration operation may not execute." );
                    yield return arr[i];
                }
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
                return this.GetEnumerator();
            }

            #endregion
        }
    }
}

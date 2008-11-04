#region Using directives

using System;
using coll = System.Collections;
using gen = System.Collections.Generic;
using System.Text;

#endregion

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents a strongly typed list of objects that can be accessed by index. Provides methods to search, sort, and manipulate lists.
    /// </summary>
    /// <typeparam name="T">The type of the contained elements.</typeparam>
    public partial class List<T> : ReadOnlyBase, gen.IList<T>, gen.ICollection<T>, coll.IList, coll.ICollection, coll.IEnumerable {
        #region Events
        /// <summary>
        /// Raised before an action that will change the list is 
        /// performed allowing that action to be cancelled.
        /// </summary>
        public event ListChangingEventHandler<T> ListChanging;
        bool OnListChanging( ListChangingEventArgs<T> args ) {
            if (ListChanging != null)
                ListChanging( this, args );
            return args.Cancel;
        }

        /// <summary>
        /// Raised after the list has changed to allow others to track these changes.
        /// </summary>
        public event ListChangedEventHandler<T> ListChanged;
        void OnListChanged( ListChangedEventArgs<T> args ) {
            if (ListChanged != null)
                ListChanged( this, args );
        }
        #endregion

        #region Fields & props
        ArrayWrapper<T> arr;

        /// <summary>
        /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <value></value>
        /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</returns>
        public int Count {
            get { return arr.Count; }
        }

        /// <summary>
        /// Gets or sets the <see cref="T:T"/> at the specified index.
        /// </summary>
        /// <value></value>
        public T this[int index] {
            get { return arr[index]; }
            set {
                AssertReadOnly();

                T oldItem = arr[index];
                if (OnListChanging( new ListChangingEventArgs<T>( Change.Replace, oldItem, value ) ))
                    return;

                arr[index] = value;

                OnListChanged( new ListChangedEventArgs<T>( Change.Replace, oldItem, value ) );
            }
        }
        #endregion

        #region Cons
        /// <summary>
        /// Initializes a new Holion.Collections.List'1
        /// </summary>
        public List()
            : this( new ArrayWrapper<T>(), false ) {
        }

        private List( ArrayWrapper<T> arr, bool isReadOnly ) {
            this.arr = arr;
            this.IsReadOnly = isReadOnly;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public void Add( T item ) {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Add, item ) ))
                return;

            arr.Add( item );

            OnListChanged( new ListChangedEventArgs<T>( Change.Add, item ) );
        }

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="range">The range.</param>
        public void AddRange( gen.IEnumerable<T> range ) {
            AssertReadOnly();

            foreach (T item in range)
                this.Add( item );
        }

        /// <summary>
        /// Returns a read-only <see cref="T:System.Collections.Generic.IList&lt;T&gt;"/> wrapper for the current collection. 
        /// </summary>
        /// <returns></returns>
        public gen.IList<T> AsReadOnly() {
            return new List<T>( this.arr, true );
        }

        /// <summary>
        /// Performs a binary search for the specified item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        public int BinarySearch( int index, int count, T item, gen.IComparer<T> comparer ) {
            if (index >= this.arr.count)
                throw new ArgumentOutOfRangeException( "index" );
            if (index + count >= this.arr.count)
                throw new ArgumentOutOfRangeException( "count" );

            return Array.BinarySearch<T>( arr.arr, index, count, item, comparer );
        }

        /// <summary>
        /// Performs a binary search for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int BinarySearch( T item ) {
            return BinarySearch( 0, this.Count, item, gen.Comparer<T>.Default );
        }

        /// <summary>
        /// Performs a binary search for the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns></returns>
        public int BinarySearch( T item, gen.IComparer<T> comparer ) {
            return BinarySearch( 0, this.Count, item, comparer );
        }

        /// <summary>
        /// Performs a binary search for the specified item.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int BinarySearch( int index, int count, T item ) {
            return BinarySearch( index, count, item, gen.Comparer<T>.Default );
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only. </exception>
        public void Clear() {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>() ))
                return;

            arr.Clear();

            OnListChanged( new ListChangedEventArgs<T>() );
        }

        /// <summary>
        /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"></see> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item is found in the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false.
        /// </returns>
        public bool Contains( T item ) {
            return arr.Contains( item );
        }

        /// <summary>
        /// Converts all elements from type <see cref="T:T"/> to type <see cref="T:TOutput"/>.
        /// </summary>
        /// <param name="converter">The converter.</param>
        /// <returns></returns>
        public List<TOutput> ConvertAll<TOutput>( Converter<T, TOutput> converter ) {
            return new List<TOutput>(
                this.arr.ConvertAll<TOutput>( converter ),
                false );
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"></see> to an <see cref="T:System.Array"></see>, starting at a particular <see cref="T:System.Array"></see> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array"></see> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"></see>. The <see cref="T:System.Array"></see> must have zero-based indexing.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">arrayIndex is less than 0.</exception>
        /// <exception cref="T:System.ArgumentNullException">array is null.</exception>
        /// <exception cref="T:System.ArgumentException">array is multidimensional.-or-arrayIndex is equal to or greater than the length of array.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"></see> is greater than the available space from arrayIndex to the end of the destination array.-or-Type T cannot be cast automatically to the type of the destination array.</exception>
        public void CopyTo( T[] array, int arrayIndex ) {
            if (this.Count == 0)
                return;

            arr.CopyTo( array, arrayIndex );
        }

        /// <summary>
        /// Evaluates whether an elements satisfying the specified predicate exists.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public bool Exists( Predicate<T> match ) {
            if (match == null)
                throw new ArgumentNullException( "match" );

            return FindIndex( match ) != -1;
        }

        /// <summary>
        /// Finds the first element satisfying the specified predicate.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public T Find( Predicate<T> match ) {
            if (match == null)
                throw new ArgumentNullException( "match" );

            int index = FindIndex( match );
            if (index != -1)
                return this[index];

            return default( T );
        }

        /// <summary>
        /// Finds all elements satisfying the specified predicate.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public List<T> FindAll( Predicate<T> match ) {
            if (match == null)
                throw new ArgumentNullException( "match" );

            List<T> matches = new List<T>();
            foreach (T item in this)
                if (match( item ))
                    matches.Add( item );

            return matches;
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the entire collection. 
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindIndex( Predicate<T> match ) {
            return FindIndex( 0, this.Count, match );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the collection that extends from the specified index to the last element. 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindIndex( int index, Predicate<T> match ) {
            return FindIndex( index, this.Count - index, match );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the first occurrence within the range of elements in the collection that starts at the specified index and contains the specified number of elements. 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindIndex( int index, int count, Predicate<T> match ) {
            return Array.FindIndex<T>( this.arr.arr, index, count, match );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the last occurrence within the entire collection. 
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public T FindLast( Predicate<T> match ) {
            if (match == null)
                throw new ArgumentNullException( "match" );

            int index = FindLastIndex( match );
            if (index != -1)
                return this[index];

            return default( T );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the entire collection. 
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindLastIndex( Predicate<T> match ) {
            return FindLastIndex( this.Count - 1, this.Count, match );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the collection that extends from the first element to the specified index. 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindLastIndex( int index, Predicate<T> match ) {
            return FindLastIndex( index, index + 1, match );
        }

        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the range of elements in the collection that contains the specified number of elements and ends at the specified index. 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int FindLastIndex( int index, int count, Predicate<T> match ) {
            if (index >= this.arr.count)
                throw new ArgumentOutOfRangeException( "index" );

            return Array.FindLastIndex<T>( this.arr.arr, index, count, match );
        }

        /// <summary>
        /// Performs the specified action on each element of the collection. 
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach( Action<T> action ) {
            if (action == null)
                throw new ArgumentNullException( "action" );

            foreach (T item in this)
                action( item );
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"></see> that can be used to iterate through the collection.
        /// </returns>
        public gen.IEnumerator<T> GetEnumerator() {
            return arr.GetEnumerator();
        }

        /// <summary>
        /// Creates a shallow copy of a range of elements in the source List.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public List<T> GetRange( int index, int count ) {
            List<T> items = new List<T>();
            for (int i = 0; i < index + count; i++)
                items.Add( this[i] );

            return items;
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"></see>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf( T item ) {
            return arr.IndexOf( item );
        }

        /// <summary>
        /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"></see> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"></see>.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void Insert( int index, T item ) {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Add, item ) ))
                return;

            arr.Insert( index, item );

            OnListChanged( new ListChangedEventArgs<T>( Change.Add, item ) );
        }

        /// <summary>
        /// Inserts the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="range">The range.</param>
        public void InsertRange( int index, gen.IEnumerable<T> range ) {
            AssertReadOnly();

            int i = index;
            foreach (T item in range)
                Insert( i++, item );
        }

        /// <summary>
        /// Searches for the specified object and returns the index of the last occurrence within the entire collection. 
        /// </summary>
        /// <param name="item">The item.</param>
        /// <returns></returns>
        public int LastIndexOf( T item ) {
            return LastIndexOf( item, this.Count - 1, this.Count );
        }

        /// <summary>
        /// Searches for the specified object and returns the index of the last occurrence within the range of elements in the collection that extends from the first element to the specified index. 
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public int LastIndexOf( T item, int index ) {
            return LastIndexOf( item, index, index + 1 );
        }

        /// <summary>
        /// Searches for the specified object and returns the index of the last occurrence within the range of elements in the collection that contains the specified number of elements and ends at the specified index.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <returns></returns>
        public int LastIndexOf( T item, int index, int count ) {
            if (index >= this.arr.count)
                throw new ArgumentOutOfRangeException( "index" );
            return Array.LastIndexOf<T>( this.arr.arr, item, index, count );
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"></see>.</param>
        /// <returns>
        /// true if item was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"></see>; otherwise, false. This method also returns false if item is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"></see>.
        /// </returns>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"></see> is read-only.</exception>
        public bool Remove( T item ) {
            AssertReadOnly();

            if (!this.Contains( item ))
                return false;

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Remove, item ) ))
                return false;

            bool removed = arr.Remove( item );

            if (removed)
                OnListChanged( new ListChangedEventArgs<T>( Change.Remove, item ) );

            return removed;
        }

        /// <summary>
        /// Removes all.
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public int RemoveAll( Predicate<T> match ) {
            AssertReadOnly();

            List<T> items = new List<T>();
            foreach (T item in this)
                if (match( item ))
                    items.Add( item );

            foreach (T item in items)
                this.Remove( item );

            return items.Count;
        }

        /// <summary>
        /// Removes the <see cref="T:System.Collections.Generic.IList`1"></see> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"></see> is read-only.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">index is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"></see>.</exception>
        public void RemoveAt( int index ) {
            AssertReadOnly();

            T item = this[index];
            if (OnListChanging( new ListChangingEventArgs<T>( Change.Remove, item ) ))
                return;

            arr.RemoveAt( index );

            OnListChanged( new ListChangedEventArgs<T>( Change.Remove, item ) );
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public void RemoveRange( int index, int count ) {
            AssertReadOnly();

            for (int i = 0; i < count; i++)
                this.RemoveAt( index );
        }

        /// <summary>
        /// Reverses the sequence of the elements in a range of elements in the collection. 
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        public void Reverse( int index, int count ) {
            AssertReadOnly();

            if (index >= this.arr.count)
                throw new ArgumentOutOfRangeException( "index" );
            if (index + count >= this.arr.count)
                throw new ArgumentOutOfRangeException( "count" );

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Rearrange ) ))
                return;

            Array.Reverse( this.arr.arr, index, Count );

            OnListChanged( new ListChangedEventArgs<T>( Change.Rearrange ) );
        }

        /// <summary>
        /// Reverses the sequence of the elements in the entire collection. 
        /// </summary>
        public void Reverse() {
            AssertReadOnly();

            Reverse( 0, this.Count );
        }

        /// <summary>
        /// Sorts this instance.
        /// </summary>
        public void Sort() {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Rearrange ) ))
                return;

            Array.Sort<T>( this.arr.arr, 0, this.Count );

            OnListChanged( new ListChangedEventArgs<T>( Change.Rearrange ) );
        }

        /// <summary>
        /// Sorts this instance using the specified comparer.
        /// </summary>
        /// <param name="comparer">The comparer.</param>
        public void Sort( gen.IComparer<T> comparer ) {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Rearrange ) ))
                return;

            Array.Sort<T>( this.arr.arr, 0, this.Count, comparer );

            OnListChanged( new ListChangedEventArgs<T>( Change.Rearrange ) );
        }

        /// <summary>
        /// Sorts this instance using the specified comparison.
        /// </summary>
        /// <param name="comparison">The comparison.</param>
        public void Sort( Comparison<T> comparison ) {
            AssertReadOnly();

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Rearrange ) ))
                return;

            Sort( new ComparisonWrapper( comparison ) );

            OnListChanged( new ListChangedEventArgs<T>( Change.Rearrange ) );
        }

        /// <summary>
        /// Sorts a subset of this instance using the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="count">The count.</param>
        /// <param name="comparer">The comparer.</param>
        public void Sort( int index, int count, gen.IComparer<T> comparer ) {
            AssertReadOnly();

            if (index >= this.arr.count)
                throw new ArgumentOutOfRangeException( "index" );
            if (index + count >= this.arr.count)
                throw new ArgumentOutOfRangeException( "count" );

            if (OnListChanging( new ListChangingEventArgs<T>( Change.Rearrange ) ))
                return;

            Array.Sort<T>( this.arr.arr, index, count, comparer );

            OnListChanged( new ListChangedEventArgs<T>( Change.Rearrange ) );
        }

        /// <summary>
        /// Converts this instance to an array.
        /// </summary>
        /// <returns></returns>
        public T[] ToArray() {
            T[] arr = new T[this.Count];

            if (this.Count > 0)
                this.arr.CopyTo( arr, 0 );

            return arr;
        }

        /// <summary>
        /// Trims the excess capacity of the collection.
        /// </summary>
        public void TrimExcess() {
            arr.TrimExcess();
        }

        /// <summary>
        /// Determines whether every element in the collection matches the conditions defined by the specified predicate. 
        /// </summary>
        /// <param name="match">The match.</param>
        /// <returns></returns>
        public bool TrueForAll( Predicate<T> match ) {
            return Array.TrueForAll<T>( arr.arr, match );
        }
        #endregion

        #region Interface methods implemented explicitly
        int System.Collections.IList.Add( object value ) {
            this.Add( (T) value );
            return this.Count;
        }

        void System.Collections.IList.Clear() {
            this.Clear();
        }

        bool System.Collections.IList.Contains( object value ) {
            return this.Contains( (T) value );
        }

        int System.Collections.IList.IndexOf( object value ) {
            return this.IndexOf( (T) value );
        }

        void System.Collections.IList.Insert( int index, object value ) {
            this.Insert( index, (T) value );
        }

        bool System.Collections.IList.IsFixedSize {
            get { return this.IsReadOnly; }
        }

        bool System.Collections.IList.IsReadOnly {
            get { return this.IsReadOnly; }
        }

        void System.Collections.IList.Remove( object value ) {
            this.Remove( (T) value );
        }

        void System.Collections.IList.RemoveAt( int index ) {
            this.RemoveAt( index );
        }
        object System.Collections.IList.this[int index] {
            get { return this[index]; }

            set { this[index] = (T) value; }
        }

        void System.Collections.ICollection.CopyTo( Array array, int index ) {
            this.CopyTo( (T[]) array, index );
        }

        int System.Collections.ICollection.Count {
            get { return this.Count; }
        }

        bool System.Collections.ICollection.IsSynchronized {
            get { return false; }
        }

        object System.Collections.ICollection.SyncRoot {
            get { return this; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return this.GetEnumerator();
        }
        #endregion
    }
}

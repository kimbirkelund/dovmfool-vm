#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents the method that handles the ListChanged event.
    /// </summary>
    public delegate void ListChangedEventHandler<T>(object sender, ListChangedEventArgs<T> args);

    /// <summary>
    /// Provides data for the ListChanged event.
    /// </summary>
    public class ListChangedEventArgs<T> : EventArgs {
        Change change;
        /// <summary>
        /// Gets the kind of change the collection in question has experienced
        /// </summary>
        public Change Change {
            get { return change; }
        }

        private T oldItem;
        /// <summary>
        /// Gets the item in question for Change.Remove and the replaced item for Change.Replace
        /// </summary>
        public T OldItem {
            get { return oldItem; }
        }

        private T newItem;
        /// <summary>
        /// Gets the item in question for Change.Add and the replacing item for Change.Replace
        /// </summary>
        public T NewItem {
            get { return newItem; }
        }

        /// <summary>
        /// Initializes a new instance of ListChangedEventArgs 
        /// </summary>
        /// <param name="change">The kind of change this instance represent</param>
        /// <param name="oldItem">The item in question for Change.Remove and the replaced item for Change.Replace</param>
        /// <param name="newItem">The item in question for Change.Add and the replacing item for Change.Replace</param>
        public ListChangedEventArgs(Change change, T oldItem, T newItem) {
            this.change = change;
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        /// <summary>
        /// Initializes a new instance of ListChangedEventArgs
        /// </summary>
        /// <param name="change">The kind of change this instance represent. This can't be Change.Replace when using this constructor.</param>
        /// <param name="item">The item in question for Change.Add and Change.Remove events</param>
        public ListChangedEventArgs(Change change, T item) {
            if (change == Change.Replace)
                throw new ArgumentException( "Argument can't be Replace.", "change" );

            this.change = change;
            if (change == Change.Add)
                this.newItem = item;
            else if (change == Change.Remove)
                this.oldItem = item;
        }

        /// <summary>
        /// Initializes a new instance of ListChangedEventArgs
        /// </summary>
        /// <param name="change">The kind of change this instance represent. This can only be Change.Clear or Change.Rearrange when using this constructor</param>
        public ListChangedEventArgs(Change change) {
            if (change != Change.Clear && change != Change.Rearrange)
                throw new ArgumentException( "Argument can only be Change.Clear or Change.Rearrange.", "change" );

            this.change = change;
        }

        /// <summary>
        /// Initializes a new instance ListChangedEventArgs representing that the collection has been cleared
        /// </summary>
        public ListChangedEventArgs()
            : this( Change.Clear ) {
        }
    }
}

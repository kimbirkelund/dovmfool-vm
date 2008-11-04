#region Using directives

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents the method that handles the ListChaging event.
    /// </summary>
    public delegate void ListChangingEventHandler<T>(object sender, ListChangingEventArgs<T> args);

    /// <summary>
    /// Provides data for the ListChanging event.
    /// </summary>
    public class ListChangingEventArgs<T> : EventArgs {
        Change change;
        /// <summary>
        /// Gets the kind of change the collection in question has experienced
        /// </summary>
        public Change Change {
            get { return change; }
        }

        private T oldItem;
        /// <summary>
        /// Gets the old item if the change was a replacement
        /// </summary>
        public T OldItem {
            get { return oldItem; }
        }

        private T newItem;
        /// <summary>
        /// Gets the new item for any type of change
        /// </summary>
        public T NewItem {
            get { return newItem; }
        }

        bool cancel;
        /// <summary>
        /// Gets or sets a value indicating to the collection whether the action triggering the event should be cancelled
        /// </summary>
        public bool Cancel {
            get { return cancel; }
            set { cancel = value; }
        }

        /// <summary>
        /// Initializes a new instance of ListChangingEventArgs 
        /// </summary>
        /// <param name="change">The kind of change this instance represent</param>
        /// <param name="oldItem">The replaced item if change is Change.Replace</param>
        /// <param name="newItem">The new item for any type of change</param>
        public ListChangingEventArgs(Change change, T oldItem, T newItem) {
            this.change = change;
            this.oldItem = oldItem;
            this.newItem = newItem;
        }

        /// <summary>
        /// Initializes a new instance of ListChangingEventArgs
        /// </summary>
        /// <param name="change">The kind of change this instance represent. This can't be Change.Replace when using this constructor.</param>
        /// <param name="item">The new item for any type of change</param>
        public ListChangingEventArgs(Change change, T item) {
            if (change == Change.Replace)
                throw new ArgumentException( "Argument can't be Replace.", "change" );

            this.change = change;
            if (change == Change.Add)
                this.newItem = item;
            else if (change == Change.Remove)
                this.oldItem = item;
        }

        /// <summary>
        /// Initializes a new instance of ListChangingEventArgs
        /// </summary>
        /// <param name="change">The kind of change this instance represent. This can only be Change.Clear or Change.Rearrange when using this constructor</param>
        public ListChangingEventArgs(Change change) {
            if (change != Change.Clear && change != Change.Rearrange)
                throw new ArgumentException( "Argument can only be Change.Clear or Change.Rearrange.", "change" );

            this.change = change;
        }

        /// <summary>
        /// Initializes a new instance ListChangingEventArgs representing that the collection has been cleared
        /// </summary>
        public ListChangingEventArgs()
            : this( Change.Clear ) {
        }
    }
}

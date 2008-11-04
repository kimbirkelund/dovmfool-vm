using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents a general tree. Should be specialized.
    /// </summary>
    /// <typeparam name="TNode"></typeparam>
    public class Tree<TNode> where TNode : Tree<TNode> {
        TNode parent;
        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public TNode Parent {
            get { return parent; }
            set { value.Children.Add( (TNode) this ); }
        }

        /// <summary>
        /// Gets the left sibling.
        /// </summary>
        /// <value>The left sibling.</value>
        public TNode LeftSibling {
            get {
                if (parent == null)
                    return null;
                int index = parent.Children.IndexOf( (TNode) this );
                if (index == 0)
                    return null;
                return parent.Children[index - 1];
            }
        }

        /// <summary>
        /// Gets the right sibling.
        /// </summary>
        /// <value>The right sibling.</value>
        public TNode RightSibling {
            get {
                if (parent == null)
                    return null;
                int index = parent.Children.IndexOf( (TNode) this );
                if (index == parent.Children.Count - 1)
                    return null;
                return parent.Children[index + 1];
            }
        }

        IList<TNode> children;
        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <value>The children.</value>
        public IList<TNode> Children {
            get { return children; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree&lt;TNode&gt;"/> class.
        /// </summary>
        public Tree() {
            var children = new Sekhmet.Collections.List<TNode>();
            children.ListChanged += children_Changed;
            this.children = children;
        }

        void children_Changed( object sender, Sekhmet.Collections.ListChangedEventArgs<TNode> args ) {
            if (args.OldItem != null)
                args.OldItem.parent = null;

            if (args.NewItem != null) {
                if (args.NewItem.parent != null)
                    args.NewItem.parent.Children.Remove( args.NewItem );
                args.NewItem.parent = (TNode) this;
            }
        }


        /// <summary>
        /// Performs a preorder walk of the tree performing <c>preAction</c> at each node.
        /// </summary>
        public void PreorderWalk( Action<TNode> preAction ) {
            Walk( preAction, null );
        }

        /// <summary>
        /// Performs a postorder walk of the tree performing <c>preAction</c> at each node.
        /// </summary>
        public void PostorderWalk( Action<TNode> postAction ) {
            Walk( null, postAction );
        }

        /// <summary>
        /// Walks the tree performing the specified actions on each node.
        /// </summary>
        /// <param name="postAction">The action to be performed on a node when leaving it.</param>
        /// <param name="preAction">The action to be performed on a node when entering it.</param>
        public void Walk( Action<TNode> preAction, Action<TNode> postAction ) {
            Stack<TNode> stack = new Stack<TNode>();
            stack.Push( (TNode) this );

            if (preAction != null)
                preAction( (TNode) this );

            foreach (var child in Children)
                child.Walk( preAction, postAction );

            if (postAction != null)
                postAction( (TNode) this );
        }

        /// <summary>
        /// Removes this instance from its parent. Does nothing if this instance is the root.
        /// </summary>
        public void Remove() {
            if (parent == null)
                return;
            parent.Children.Remove( (TNode) this );
        }

        /// <summary>
        /// Replaces the subtree root at this instance with the subtree rooted at the specified node.
        /// </summary>
        /// <param name="with">The subtree to replace with.</param>
        public void Replace( TNode with ) {
            if (this.Parent == null)
                throw new InvalidOperationException( "Can't replace root" );

            this.Parent.Children[this.Parent.Children.IndexOf( (TNode) this )] = with;
        }

        /// <summary>
        /// Inserts the specified node just before/to the left of this instance.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        public void InsertBefore( TNode node ) {
            if (this.Parent == null)
                throw new InvalidOperationException( "Can't insert next to root." );

            int index = this.Parent.Children.IndexOf( (TNode) this );
            this.Parent.Children.Insert( index, node );
        }

        /// <summary>
        /// Inserts the specified node just after/to the right of this instance.
        /// </summary>
        /// <param name="node">The node to insert.</param>
        public void InsertAfter( TNode node ) {
            if (this.Parent == null)
                throw new InvalidOperationException( "Can't insert next to root." );

            int index = this.Parent.Children.IndexOf( (TNode) this );
            this.Parent.Children.Insert( index + 1, node );
        }
    }
}

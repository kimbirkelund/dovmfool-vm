using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Collections {
    /// <summary>
    /// Represents a node in an undirected graph. Should be specialized.
    /// </summary>
    /// <typeparam name="TGraph"></typeparam>
    public class Graph<TGraph> where TGraph : Graph<TGraph> {
        IList<TGraph> neighbors;
        /// <summary>
        /// Gets the neighbors.
        /// </summary>
        /// <value>The neighbors.</value>
        public IList<TGraph> Neighbors {
            get { return neighbors; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Graph&lt;TGraph&gt;"/> class.
        /// </summary>
        public Graph() {
            Sekhmet.Collections.List<TGraph> neighbors = new Sekhmet.Collections.List<TGraph>();
            neighbors.ListChanged += new ListChangedEventHandler<TGraph>( neighbors_ListChanged );
            this.neighbors = neighbors;
        }

        void neighbors_ListChanged( object sender, ListChangedEventArgs<TGraph> args ) {
            if (args.OldItem != null)
                args.OldItem.Neighbors.Remove( (TGraph) this );

            if (args.NewItem != null && !args.NewItem.Neighbors.Contains( (TGraph) this ))
                args.NewItem.Neighbors.Add( (TGraph) this );
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Collections {
    public partial class List<T> {
        class ComparisonWrapper : IComparer<T> {
            Comparison<T> comparison;

            public ComparisonWrapper( Comparison<T> comparison ) {
                this.comparison = comparison;
            }

            public int Compare( T x, T y ) {
                return comparison( x, y );
            }
        }
    }
}

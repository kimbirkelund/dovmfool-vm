using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet {
    /// <summary>
    /// Provides access to creating tuples.
    /// </summary>
    public static class Tuple {
        /// <summary>
        /// Creates a 1 element tuple.
        /// </summary>
        public static Tuple<T1> New<T1>( T1 first ) {
            return new Tuple<T1>( first );
        }

        /// <summary>
        /// Creates a 2 element tuple.
        /// </summary>
        public static Tuple<T1, T2> New<T1, T2>( T1 first, T2 second ) {
            return new Tuple<T1, T2>( first, second );
        }

        /// <summary>
        /// Creates a 3 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3> New<T1, T2, T3>( T1 first, T2 second, T3 third ) {
            return new Tuple<T1, T2, T3>( first, second, third );
        }

        /// <summary>
        /// Creates a 4 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4> New<T1, T2, T3, T4>( T1 first, T2 second, T3 third, T4 fourth ) {
            return new Tuple<T1, T2, T3, T4>( first, second, third, fourth );
        }

        /// <summary>
        /// Creates a 5 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5> New<T1, T2, T3, T4, T5>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth ) {
            return new Tuple<T1, T2, T3, T4, T5>( first, second, third, fourth, fifth );
        }

        /// <summary>
        /// Creates a 6 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5, T6> New<T1, T2, T3, T4, T5, T6>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth ) {
            return new Tuple<T1, T2, T3, T4, T5, T6>( first, second, third, fourth, fifth, sixth );
        }

        /// <summary>
        /// Creates a 7 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5, T6, T7> New<T1, T2, T3, T4, T5, T6, T7>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh ) {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7>( first, second, third, fourth, fifth, sixth, seventh );
        }

        /// <summary>
        /// Creates a 8 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8> New<T1, T2, T3, T4, T5, T6, T7, T8>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth ) {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8>( first, second, third, fourth, fifth, sixth, seventh, eigth );
        }

        /// <summary>
        /// Creates a 9 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> New<T1, T2, T3, T4, T5, T6, T7, T8, T9>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth, T9 ninth ) {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>( first, second, third, fourth, fifth, sixth, seventh, eigth, ninth );
        }

        /// <summary>
        /// Creates a 10 element tuple.
        /// </summary>
        public static Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> New<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth, T9 ninth, T10 tenth ) {
            return new Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>( first, second, third, fourth, fifth, sixth, seventh, eigth, ninth, tenth );
        }
    }

    /// <summary>
    /// Represents a 1 element tuple.
    /// </summary>
    public sealed class Tuple<T1> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        public Tuple( T1 first ) {
            this.First = first;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1> value ) {
            return First.Equals( value.First );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return First.GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1> value1, Tuple<T1> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1> value1, Tuple<T1> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 2 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        public Tuple( T1 first, T2 second ) {
            this.First = first;
            this.Second = second;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2> value1, Tuple<T1, T2> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2> value1, Tuple<T1, T2> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 3 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        public Tuple( T1 first, T2 second, T3 third ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3> value1, Tuple<T1, T2, T3> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3> value1, Tuple<T1, T2, T3> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 4 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4> value1, Tuple<T1, T2, T3, T4> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4> value1, Tuple<T1, T2, T3, T4> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 5 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5> value1, Tuple<T1, T2, T3, T4, T5> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5> value1, Tuple<T1, T2, T3, T4, T5> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 6 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5, T6> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;
        /// <summary>
        /// The sixth element.
        /// </summary>
        public readonly T6 Sixth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5, T6&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
            this.Sixth = sixth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5, T6> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth )
                && Sixth.Equals( value.Sixth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                + Sixth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5, T6>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5, T6> value1, Tuple<T1, T2, T3, T4, T5, T6> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5, T6> value1, Tuple<T1, T2, T3, T4, T5, T6> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 7 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;
        /// <summary>
        /// The sixth element.
        /// </summary>
        public readonly T6 Sixth;
        /// <summary>
        /// The seventh element.
        /// </summary>
        public readonly T7 Seventh;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5, T6, T7&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">The seventh.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
            this.Sixth = sixth;
            this.Seventh = seventh;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5, T6, T7> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth )
                && Sixth.Equals( value.Sixth )
                && Seventh.Equals( value.Seventh );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                + Sixth.GetHashCode()
                + Seventh.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5, T6, T7>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5, T6, T7> value1, Tuple<T1, T2, T3, T4, T5, T6, T7> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5, T6, T7> value1, Tuple<T1, T2, T3, T4, T5, T6, T7> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 8 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;
        /// <summary>
        /// The sixth element.
        /// </summary>
        public readonly T6 Sixth;
        /// <summary>
        /// The seventh element.
        /// </summary>
        public readonly T7 Seventh;
        /// <summary>
        /// The eight element.
        /// </summary>
        public readonly T8 Eigth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5, T6, T7, T8&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">The seventh.</param>
        /// <param name="eigth">The eigth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
            this.Sixth = sixth;
            this.Seventh = seventh;
            this.Eigth = eigth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5, T6, T7, T8> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth )
                && Sixth.Equals( value.Sixth )
                && Seventh.Equals( value.Seventh )
                && Eigth.Equals( value.Eigth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                + Sixth.GetHashCode()
                + Seventh.GetHashCode()
                + Eigth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5, T6, T7, T8>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5, T6, T7, T8> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5, T6, T7, T8> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 9 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;
        /// <summary>
        /// The sixth element.
        /// </summary>
        public readonly T6 Sixth;
        /// <summary>
        /// The seventh element.
        /// </summary>
        public readonly T7 Seventh;
        /// <summary>
        /// The eight element.
        /// </summary>
        public readonly T8 Eigth;
        /// <summary>
        /// The ninth element.
        /// </summary>
        public readonly T9 Ninth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">The seventh.</param>
        /// <param name="eigth">The eigth.</param>
        /// <param name="ninth">The ninth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth, T9 ninth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
            this.Sixth = sixth;
            this.Seventh = seventh;
            this.Eigth = eigth;
            this.Ninth = ninth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth )
                && Sixth.Equals( value.Sixth )
                && Seventh.Equals( value.Seventh )
                && Eigth.Equals( value.Eigth )
                && Ninth.Equals( value.Ninth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                + Sixth.GetHashCode()
                + Seventh.GetHashCode()
                + Eigth.GetHashCode()
                + Ninth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }

    /// <summary>
    /// Represents a 10 element tuple.
    /// </summary>
    public sealed class Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> {
        /// <summary>
        /// The first element.
        /// </summary>
        public readonly T1 First;
        /// <summary>
        /// The second element.
        /// </summary>
        public readonly T2 Second;
        /// <summary>
        /// The third element.
        /// </summary>
        public readonly T3 Third;
        /// <summary>
        /// The fourth element.
        /// </summary>
        public readonly T4 Fourth;
        /// <summary>
        /// The fifth element.
        /// </summary>
        public readonly T5 Fifth;
        /// <summary>
        /// The sixth element.
        /// </summary>
        public readonly T6 Sixth;
        /// <summary>
        /// The seventh element.
        /// </summary>
        public readonly T7 Seventh;
        /// <summary>
        /// The eight element.
        /// </summary>
        public readonly T8 Eigth;
        /// <summary>
        /// The ninth element.
        /// </summary>
        public readonly T9 Ninth;
        /// <summary>
        /// The tenth element.
        /// </summary>
        public readonly T10 Tenth;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tuple&lt;T1, T2, T3, T4, T5, T6, T7, T8, T9, T10&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="third">The third.</param>
        /// <param name="fourth">The fourth.</param>
        /// <param name="fifth">The fifth.</param>
        /// <param name="sixth">The sixth.</param>
        /// <param name="seventh">The seventh.</param>
        /// <param name="eigth">The eigth.</param>
        /// <param name="ninth">The ninth.</param>
        /// <param name="tenth">The tenth.</param>
        public Tuple( T1 first, T2 second, T3 third, T4 fourth, T5 fifth, T6 sixth, T7 seventh, T8 eigth, T9 ninth, T10 tenth ) {
            this.First = first;
            this.Second = second;
            this.Third = third;
            this.Fourth = fourth;
            this.Fifth = fifth;
            this.Sixth = sixth;
            this.Seventh = seventh;
            this.Eigth = eigth;
            this.Ninth = ninth;
            this.Tenth = tenth;
        }

        #region Equals
        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public bool Equals( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value ) {
            return First.Equals( value.First )
                && Second.Equals( value.Second )
                && Third.Equals( value.Third )
                && Fourth.Equals( value.Fourth )
                && Fifth.Equals( value.Fifth )
                && Sixth.Equals( value.Sixth )
                && Seventh.Equals( value.Seventh )
                && Eigth.Equals( value.Eigth )
                && Ninth.Equals( value.Ninth )
                && Tenth.Equals( value.Tenth );
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            return (First.GetHashCode()
                + Second.GetHashCode()
                + Third.GetHashCode()
                + Fourth.GetHashCode()
                + Fifth.GetHashCode()
                + Sixth.GetHashCode()
                + Seventh.GetHashCode()
                + Eigth.GetHashCode()
                + Ninth.GetHashCode()
                + Tenth.GetHashCode()
                ).GetHashCode();
        }

        /// <summary>
        /// Computes equality of the current instance and <c>value</c>.
        /// </summary>
        public override bool Equals( object value ) {
            return this == (value as Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>);
        }

        /// <summary>
        /// Computes equality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator ==( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value2 ) {
            if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
                return true;
            if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
                return false;
            return value1.Equals( value2 );
        }

        /// <summary>
        /// Computes inequality of <c>value1</c> and <c>value2</c>.
        /// </summary>
        public static bool operator !=( Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value1, Tuple<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> value2 ) {
            return !(value1 == value2);
        }
        #endregion
    }
}

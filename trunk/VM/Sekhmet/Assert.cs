using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Globalization;

namespace Sekhmet {
    /// <summary>
    /// Easy access to express assertions about ones code. For debugging purposes only.
    /// </summary>
    public static class Assert {
        /// <summary>
        /// Asserts the specified action.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void Ensure( Action action ) {
            action();
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( object expected, object actual ) {
            AreEqual( expected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual<T>( T expected, T actual ) {
            AreEqual<T>( expected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( double expected, double actual, double delta ) {
            AreEqual( expected, actual, delta, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( object expected, object actual, string message ) {
            AreEqual( expected, actual, message, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> to within <c>delta</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( float expected, float actual, float delta ) {
            AreEqual( expected, actual, delta, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual<T>( T expected, T actual, string message ) {
            AreEqual<T>( expected, actual, message, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase ) {
            AreEqual( expected, actual, ignoreCase, string.Empty, (object[]) null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> to within <c>delta</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( double expected, double actual, double delta, string message ) {
            AreEqual( expected, actual, delta, message, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( object expected, object actual, string message, params object[] parameters ) {
            AreEqual<object>( expected, actual, message, parameters );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> to within <c>delta</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( float expected, float actual, float delta, string message ) {
            AreEqual( expected, actual, delta, message, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual<T>( T expected, T actual, string message, params object[] parameters ) {
            if (!object.Equals( expected, actual ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase, CultureInfo culture ) {
            AreEqual( expected, actual, ignoreCase, culture, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase, string message ) {
            AreEqual( expected, actual, ignoreCase, message, (object[]) null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> to within <c>delta</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( double expected, double actual, double delta, string message, params object[] parameters ) {
            if (Math.Abs( (double) (expected - actual) ) > delta)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> to within <c>delta</c>. 
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( float expected, float actual, float delta, string message, params object[] parameters ) {
            if (Math.Abs( (float) (expected - actual) ) > delta)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase, CultureInfo culture, string message ) {
            AreEqual( expected, actual, ignoreCase, culture, message, null );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase, string message, params object[] parameters ) {
            AreEqual( expected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters );
        }

        /// <summary>
        /// Asserts that <c>actual</c> equals <c>expected</c> optionally ignoring case.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreEqual( string expected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters ) {
            CheckParameterNotNull( culture, "Assert.AreEqual", "culture", string.Empty, new object[0] );
            if (string.Compare( expected, actual, ignoreCase, culture ) != 0)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual<T>( T notExpected, T actual ) {
            AreNotEqual<T>( notExpected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( object notExpected, object actual ) {
            AreNotEqual( notExpected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( double notExpected, double actual, double delta ) {
            AreNotEqual( notExpected, actual, delta, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( object notExpected, object actual, string message ) {
            AreNotEqual( notExpected, actual, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual<T>( T notExpected, T actual, string message ) {
            AreNotEqual<T>( notExpected, actual, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( float notExpected, float actual, float delta ) {
            AreNotEqual( notExpected, actual, delta, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase ) {
            AreNotEqual( notExpected, actual, ignoreCase, string.Empty, (object[]) null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( double notExpected, double actual, double delta, string message ) {
            AreNotEqual( notExpected, actual, delta, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( float notExpected, float actual, float delta, string message ) {
            AreNotEqual( notExpected, actual, delta, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase, string message ) {
            AreNotEqual( notExpected, actual, ignoreCase, message, (object[]) null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual<T>( T notExpected, T actual, string message, params object[] parameters ) {
            if (object.Equals( notExpected, actual ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( object notExpected, object actual, string message, params object[] parameters ) {
            AreNotEqual<object>( notExpected, actual, message, parameters );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase, CultureInfo culture ) {
            AreNotEqual( notExpected, actual, ignoreCase, culture, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( double notExpected, double actual, double delta, string message, params object[] parameters ) {
            if (Math.Abs( (double) (notExpected - actual) ) <= delta)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( float notExpected, float actual, float delta, string message, params object[] parameters ) {
            if (Math.Abs( (float) (notExpected - actual) ) <= delta)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message ) {
            AreNotEqual( notExpected, actual, ignoreCase, culture, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase, string message, params object[] parameters ) {
            AreNotEqual( notExpected, actual, ignoreCase, CultureInfo.InvariantCulture, message, parameters );
        }

        /// <summary>
        /// Asserts the <c>actual</c> does not equal <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotEqual( string notExpected, string actual, bool ignoreCase, CultureInfo culture, string message, params object[] parameters ) {
            CheckParameterNotNull( culture, "Assert.AreNotEqual", "culture", string.Empty, new object[0] );
            if (string.Compare( notExpected, actual, ignoreCase, culture ) == 0)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is not the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotSame( object notExpected, object actual ) {
            AreNotSame( notExpected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is not the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotSame( object notExpected, object actual, string message ) {
            AreNotSame( notExpected, actual, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is not the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreNotSame( object notExpected, object actual, string message, params object[] parameters ) {
            if (object.ReferenceEquals( notExpected, actual ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreSame( object expected, object actual ) {
            AreSame( expected, actual, string.Empty, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreSame( object expected, object actual, string message ) {
            AreSame( expected, actual, message, null );
        }

        /// <summary>
        /// Asserts the <c>actual</c> is the same object as <c>notExpected</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void AreSame( object expected, object actual, string message, params object[] parameters ) {
            if (!object.ReferenceEquals( expected, actual ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        [Conditional( "DEBUG" )]
        internal static void CheckParameterNotNull( object param, string assertionName, string parameterName, string message, params object[] parameters ) {
            if (param == null)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>false</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsFalse( bool condition ) {
            IsFalse( condition, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>false</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsFalse( bool condition, string message ) {
            IsFalse( condition, message, null );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>false</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsFalse( bool condition, string message, params object[] parameters ) {
            if (condition)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>value</c> is of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsInstanceOfType( object value, Type expectedType ) {
            IsInstanceOfType( value, expectedType, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsInstanceOfType( object value, Type expectedType, string message ) {
            IsInstanceOfType( value, expectedType, message, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsInstanceOfType( object value, Type expectedType, string message, params object[] parameters ) {
            if (expectedType == null)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
            if (!expectedType.IsInstanceOfType( value ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotInstanceOfType( object value, Type wrongType ) {
            IsNotInstanceOfType( value, wrongType, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotInstanceOfType( object value, Type wrongType, string message ) {
            IsNotInstanceOfType( value, wrongType, message, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not of type <c>expectedType</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotInstanceOfType( object value, Type wrongType, string message, params object[] parameters ) {
            if (wrongType == null)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
            if ((value != null) && wrongType.IsInstanceOfType( value ))
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotNull( object value ) {
            IsNotNull( value, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotNull( object value, string message ) {
            IsNotNull( value, message, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is not <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNotNull( object value, string message, params object[] parameters ) {
            if (value == null)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>value</c> is <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNull( object value ) {
            IsNull( value, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNull( object value, string message ) {
            IsNull( value, message, null );
        }

        /// <summary>
        /// Asserts that <c>value</c> is <c>null</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsNull( object value, string message, params object[] parameters ) {
            if (value != null)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>true</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsTrue( bool condition ) {
            IsTrue( condition, string.Empty, null );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>true</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsTrue( bool condition, string message ) {
            IsTrue( condition, message, null );
        }

        /// <summary>
        /// Asserts that <c>condition</c> is <c>true</c>.
        /// </summary>
        [Conditional( "DEBUG" )]
        public static void IsTrue( bool condition, string message, params object[] parameters ) {
            if (!condition)
                throw new AssertionException( string.Format( message ?? "", parameters ?? new object[0] ) );
        }

        static string ReplaceNullChars( string input ) {
            if (string.IsNullOrEmpty( input )) {
                return input;
            }
            List<int> list = new List<int>();
            for (int i = 0; i < input.Length; i++) {
                if (input[i] == '\0') {
                    list.Add( i );
                }
            }
            if (list.Count <= 0) {
                return input;
            }
            StringBuilder builder = new StringBuilder( input.Length + list.Count );
            int startIndex = 0;
            foreach (int num3 in list) {
                builder.Append( input.Substring( startIndex, num3 - startIndex ) );
                builder.Append( @"\0" );
                startIndex = num3 + 1;
            }
            builder.Append( input.Substring( startIndex ) );
            return builder.ToString();
        }

        static string ReplaceNulls( object input ) {
            if (input == null) {
                return "Null in message.";
            }
            string str = input.ToString();
            if (str == null) {
                return "Null in message.";
            }
            return ReplaceNullChars( str );
        }


        /// <summary>
        /// The expection thrown when an assertion fails.
        /// </summary>
        [global::System.Serializable]
        public class AssertionException : Exception {
            /// <summary>
            /// Initializes a new instance of the <see cref="AssertionException"/> class.
            /// </summary>
            public AssertionException() : this( "Assertion failed." ) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="AssertionException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            public AssertionException( string message ) : base( message ) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="AssertionException"/> class.
            /// </summary>
            /// <param name="message">The message.</param>
            /// <param name="inner">The inner.</param>
            public AssertionException( string message, Exception inner ) : base( message, inner ) { }
            /// <summary>
            /// Initializes a new instance of the <see cref="AssertionException"/> class.
            /// </summary>
            /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
            /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
            /// <exception cref="T:System.ArgumentNullException">The <paramref name="info"/> parameter is null. </exception>
            /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"/> is zero (0). </exception>
            protected AssertionException(
              System.Runtime.Serialization.SerializationInfo info,
              System.Runtime.Serialization.StreamingContext context )
                : base( info, context ) { }
        }
    }
}

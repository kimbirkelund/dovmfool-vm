using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sekhmet.IO {
    /// <summary>
    /// Represents a rotating wrapper around a normal <see cref="TextWriter"/>.
    /// </summary>
    public class RotatingTextWriter : TextWriter {
        bool isDisposed;
        Func<TextWriter> createTextWriter;
        Func<bool> doRotation;
        bool rotationRequested;
        TextWriter currentWriter;

        bool rotationEnabled;
        /// <summary>
        /// Gets or sets a value indicating whether rotation is enabled.
        /// </summary>
        /// <value><c>true</c> if rotation is enabled; otherwise, <c>false</c>.</value>
        public bool RotationEnabled {
            get { return rotationEnabled; }
            set {
                if (!value)
                    Check();
                rotationEnabled = value;
                if (value)
                    Check();
            }
        }

        #region Overridden properties
        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Text.Encoding"/> in which the output is written.
        /// </summary>
        /// <value></value>
        /// <returns>The Encoding in which the output is written.</returns>
        public override Encoding Encoding {
            get {
                Check();
                return currentWriter.Encoding;
            }
        }

        /// <summary>
        /// Gets an object that controls formatting.
        /// </summary>
        /// <value></value>
        /// <returns>An <see cref="T:System.IFormatProvider"/> object for a specific culture, or the formatting of the current culture if no other culture is specified.</returns>
        public override IFormatProvider FormatProvider {
            get {
                Check();
                return currentWriter.FormatProvider;
            }
        }

        string newLine;
        /// <summary>
        /// Gets or sets the line terminator string used by the current TextWriter.
        /// </summary>
        /// <value></value>
        /// <returns>The line terminator string for the current TextWriter.</returns>
        public override string NewLine {
            get {
                Check();
                if (newLine == null)
                    newLine = currentWriter.NewLine;
                return newLine;
            }
            set {
                Check();
                this.newLine = value;
                currentWriter.NewLine = newLine;
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RotatingTextWriter"/> class.
        /// </summary>
        /// <param name="doRotation">The do rotation.</param>
        /// <param name="createTextWriter">The create text writer.</param>
        public RotatingTextWriter( Func<bool> doRotation, Func<TextWriter> createTextWriter ) {
            this.createTextWriter = createTextWriter;
            this.doRotation = doRotation;
            rotationEnabled = true;
            currentWriter = createTextWriter();
        }

        void Check() {
            if (isDisposed)
                throw new InvalidOperationException( "This instance has been disposed." );

            var rotate = doRotation();
            if (!RotationEnabled)
                rotationRequested |= rotate;
            else if (rotate || rotationRequested) {
                if (currentWriter != null)
                    currentWriter.Dispose();
                currentWriter = createTextWriter();
                if (newLine != null)
                    currentWriter.NewLine = newLine;
                rotationRequested = false;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="T:System.IO.TextWriter"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected override void Dispose( bool disposing ) {
            if (isDisposed)
                return;
            lock (this)
                isDisposed = true;

            base.Dispose( disposing );
            if (disposing && currentWriter != null) {
                currentWriter.Dispose();
                currentWriter.Close();
            }
        }

        /// <summary>
        /// Closes the current writer and releases any system resources associated with the writer.
        /// </summary>
        public override void Close() {
            Check();
            Dispose();
        }

        /// <summary>
        /// Creates an object that contains all the relevant information required to generate a proxy used to communicate with a remote object.
        /// </summary>
        /// <param name="requestedType">The <see cref="T:System.Type"/> of the object that the new <see cref="T:System.Runtime.Remoting.ObjRef"/> will reference.</param>
        /// <returns>
        /// Information required to generate a proxy.
        /// </returns>
        /// <exception cref="T:System.Runtime.Remoting.RemotingException">This instance is not a valid remoting object. </exception>
        /// <exception cref="T:System.Security.SecurityException">The immediate caller does not have infrastructure permission. </exception>
        /// <PermissionSet>
        /// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="Infrastructure"/>
        /// </PermissionSet>
        public override System.Runtime.Remoting.ObjRef CreateObjRef( Type requestedType ) {
            Check();
            return currentWriter.CreateObjRef( requestedType );
        }

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be written to the underlying device.
        /// </summary>
        public override void Flush() {
            Check();
            currentWriter.Flush();
        }

        /// <summary>
        /// Writes the text representation of a Boolean value to the text stream.
        /// </summary>
        /// <param name="value">The Boolean to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( bool value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes a character to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( char value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes a character array to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write to the text stream.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( char[] buffer ) {
            Check();
            currentWriter.Write( buffer );
        }

        /// <summary>
        /// Writes a subarray of characters to the text stream.
        /// </summary>
        /// <param name="buffer">The character array to write data from.</param>
        /// <param name="index">Starting index in the buffer.</param>
        /// <param name="count">The number of characters to write.</param>
        /// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index"/> is less than <paramref name="count"/>. </exception>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer"/> parameter is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( char[] buffer, int index, int count ) {
            Check();
            currentWriter.Write( buffer, index, count );
        }

        /// <summary>
        /// Writes the text representation of a decimal value to the text stream.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( decimal value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of an 8-byte floating-point value to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( double value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( float value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( int value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( long value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of an object to the text stream by calling ToString on that object.
        /// </summary>
        /// <param name="value">The object to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( object value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">An object to write into the formatted string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void Write( string format, object arg0 ) {
            Check();
            currentWriter.Write( format, arg0 );
        }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">An object to write into the formatted string.</param>
        /// <param name="arg1">An object to write into the formatted string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void Write( string format, object arg0, object arg1 ) {
            Check();
            currentWriter.Write( format, arg0, arg1 );
        }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">An object to write into the formatted string.</param>
        /// <param name="arg1">An object to write into the formatted string.</param>
        /// <param name="arg2">An object to write into the formatted string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void Write( string format, object arg0, object arg1, object arg2 ) {
            Check();
            currentWriter.Write( format, arg0, arg1, arg2 );
        }

        /// <summary>
        /// Writes out a formatted string, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg">The object array to write into the formatted string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> or <paramref name="arg"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to <paramref name="arg"/>. Length. </exception>
        public override void Write( string format, params object[] arg ) {
            Check();
            currentWriter.Write( format, arg );
        }

        /// <summary>
        /// Writes a string to the text stream.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( string value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( uint value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void Write( ulong value ) {
            Check();
            currentWriter.Write( value );
        }

        /// <summary>
        /// Writes a line terminator to the text stream.
        /// </summary>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine() {
            Check();
            currentWriter.WriteLine();
        }

        /// <summary>
        /// Writes the text representation of a Boolean followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The Boolean to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( bool value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes a character followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The character to write to the text stream.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( char value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes an array of characters followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="buffer">The character array from which data is read.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( char[] buffer ) {
            Check();
            currentWriter.WriteLine( buffer );
        }

        /// <summary>
        /// Writes a subarray of characters followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="buffer">The character array from which data is read.</param>
        /// <param name="index">The index into <paramref name="buffer"/> at which to begin reading.</param>
        /// <param name="count">The maximum number of characters to write.</param>
        /// <exception cref="T:System.ArgumentException">The buffer length minus <paramref name="index"/> is less than <paramref name="count"/>. </exception>
        /// <exception cref="T:System.ArgumentNullException">The <paramref name="buffer"/> parameter is null. </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">
        /// 	<paramref name="index"/> or <paramref name="count"/> is negative. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( char[] buffer, int index, int count ) {
            Check();
            currentWriter.WriteLine( buffer, index, count );
        }

        /// <summary>
        /// Writes the text representation of a decimal value followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The decimal value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( decimal value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of a 8-byte floating-point value followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( double value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte floating-point value followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte floating-point value to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( float value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte signed integer followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( int value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of an 8-byte signed integer followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte signed integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( long value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of an object by calling ToString on this object, followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The object to write. If <paramref name="value"/> is null, only the line termination characters are written.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( object value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatted string.</param>
        /// <param name="arg0">The object to write into the formatted string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void WriteLine( string format, object arg0 ) {
            Check();
            currentWriter.WriteLine( format, arg0 );
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">The object to write into the format string.</param>
        /// <param name="arg1">The object to write into the format string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void WriteLine( string format, object arg0, object arg1 ) {
            Check();
            currentWriter.WriteLine( format, arg0, arg1 );
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg0">The object to write into the format string.</param>
        /// <param name="arg1">The object to write into the format string.</param>
        /// <param name="arg2">The object to write into the format string.</param>
        /// <exception cref="T:System.ArgumentNullException">
        /// 	<paramref name="format"/> is null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to the number of provided objects to be formatted. </exception>
        public override void WriteLine( string format, object arg0, object arg1, object arg2 ) {
            Check();
            currentWriter.WriteLine( format, arg0, arg1, arg2 );
        }

        /// <summary>
        /// Writes out a formatted string and a new line, using the same semantics as <see cref="M:System.String.Format(System.String,System.Object)"/>.
        /// </summary>
        /// <param name="format">The formatting string.</param>
        /// <param name="arg">The object array to write into format string.</param>
        /// <exception cref="T:System.ArgumentNullException">A string or object is passed in as null. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        /// <exception cref="T:System.FormatException">The format specification in format is invalid.-or- The number indicating an argument to be formatted is less than zero, or larger than or equal to arg.Length. </exception>
        public override void WriteLine( string format, params object[] arg ) {
            Check();
            currentWriter.WriteLine( format, arg );
        }

        /// <summary>
        /// Writes a string followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The string to write. If <paramref name="value"/> is null, only the line termination characters are written.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( string value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of a 4-byte unsigned integer followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 4-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( uint value ) {
            Check();
            currentWriter.WriteLine( value );
        }

        /// <summary>
        /// Writes the text representation of an 8-byte unsigned integer followed by a line terminator to the text stream.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer to write.</param>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.IO.TextWriter"/> is closed. </exception>
        /// <exception cref="T:System.IO.IOException">An I/O error occurs. </exception>
        public override void WriteLine( ulong value ) {
            Check();
            currentWriter.WriteLine( value );
        }
    }
}

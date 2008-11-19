using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	[global::System.Serializable]
	public class VMException : ApplicationException {
		public VMException() { }
		public VMException( string message ) : base( message ) { }
		public VMException( string message, Exception inner ) : base( message, inner ) { }
		protected VMException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class InvalidVMProgramException : VMException {
		public InvalidVMProgramException() : base( "The executing program is invalid." ) { }
		public InvalidVMProgramException( string message ) : base( message ) { }
		public InvalidVMProgramException( string message, Exception inner ) : base( message, inner ) { }
		protected InvalidVMProgramException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class OutOfMemoryException : VMException {
		public OutOfMemoryException() : base( "Heap memory has been exhausted." ) { }
		public OutOfMemoryException( string message ) : base( message ) { }
		public OutOfMemoryException( string message, Exception inner ) : base( message, inner ) { }
		protected OutOfMemoryException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class InterpretorException : VMException {
		public InterpretorException() { }
		public InterpretorException( string message ) : base( message ) { }
		public InterpretorException( string message, Exception inner ) : base( message, inner ) { }
		protected InterpretorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class InterpretorFailedToStopException : InterpretorException {
		public InterpretorFailedToStopException() : base( "Interpretor failed to stop." ) { }
		public InterpretorFailedToStopException( string message ) : base( message ) { }
		public InterpretorFailedToStopException( string message, Exception inner ) : base( message, inner ) { }
		protected InterpretorFailedToStopException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class VMAppException : VMException {
		public VMAppException() { }
		public VMAppException( string message ) : base( message ) { }
		public VMAppException( string message, Exception inner ) : base( message, inner ) { }
		protected VMAppException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class ArgumentException : VMAppException {
		public readonly string Argument;

		public ArgumentException() { }
		public ArgumentException( string message ) { }
		public ArgumentException( string message, string argument ) : base( message ) { this.Argument = argument; }
		public ArgumentException( string message, Exception inner ) : base( message, inner ) { }
		protected ArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class ArgumentOutOfBoundsException : ArgumentException {
		public ArgumentOutOfBoundsException() { }
		public ArgumentOutOfBoundsException( string message, string argument ) : base( message, argument ) { }
		public ArgumentOutOfBoundsException( string argument ) : this( "The argument was out of bounds for the specified operation.", argument ) { }
		public ArgumentOutOfBoundsException( string message, Exception inner ) : base( message, inner ) { }
		protected ArgumentOutOfBoundsException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class MessageNotUnderstoodException : VMAppException {
		/// <summary>
		/// Gets the message that was not understood.
		/// </summary>
		public Handle<VMObjects.String> InvalidMessage { get; private set; }
		/// <summary>
		/// Gets the object that did not understand <c>InvalidMessage</c>.
		/// </summary>
		public Handle<VMObjects.AppObject> Object { get; private set; }

		public MessageNotUnderstoodException() : base( "Message not understood." ) { }
		public MessageNotUnderstoodException( string message ) : base( message ) { }
		public MessageNotUnderstoodException( string message, Exception inner ) : base( message, inner ) { }
		protected MessageNotUnderstoodException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public MessageNotUnderstoodException( Handle<VMObjects.String> invalidMessage, Handle<VMObjects.AppObject> obj )
			: this() {
			this.InvalidMessage = invalidMessage;
			this.Object = obj;
		}

		public MessageNotUnderstoodException( Handle<VMObjects.String> invalidMessage )
			: this( invalidMessage, null ) {
		}
	}

	[global::System.Serializable]
	public class ClassNotFoundException : VMAppException {
		public readonly Handle<VMObjects.String> ClassName;
		public ClassNotFoundException() { }
		public ClassNotFoundException( Handle<VMObjects.String> className ) : base( "Class not found" ) { ClassName = className; }
		public ClassNotFoundException( string message, Handle<VMObjects.String> className ) : base( message ) { ClassName = className; }
		public ClassNotFoundException( string message ) : base( message ) { }
		public ClassNotFoundException( string message, Exception inner ) : base( message, inner ) { }
		protected ClassNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class UnknownSystemCallException : VMAppException {
		public Handle<VMObjects.String> SystemCallName { get; private set; }

		public UnknownSystemCallException() : base( "Unknown system call." ) { }
		public UnknownSystemCallException( Handle<VMObjects.String> systemCall ) : this( systemCall, "Unknown system call." ) { }
		public UnknownSystemCallException( Handle<VMObjects.String> systemCall, string message ) : base( message ) { SystemCallName = systemCall; }
		public UnknownSystemCallException( string message, Exception inner ) : base( message, inner ) { }
		protected UnknownSystemCallException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}
}

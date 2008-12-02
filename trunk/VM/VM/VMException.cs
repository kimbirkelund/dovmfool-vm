using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

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

		public static VMException MakeDotNetException( Handle<AppObject> obj ) {
			return null;
		}

		internal virtual Handle<AppObject> ToVMException() {
			var eCls = VirtualMachine.ResolveClass( null, "System.Exception".ToVMString(), false ).ToHandle();
			var eInit = eCls.ResolveMessageHandler( null, KnownStrings.initialize_1 ).ToHandle();
			var e = AppObject.CreateInstance( eCls ).ToHandle();
			var interp = VirtualMachine.Fork( eInit, e, Message.ToVMString().To<AppObject>() );
			interp.Start();
			interp.Join();
			return e;
		}
	}

	[global::System.Serializable]
	public class InvalidThreadIdException : VMException {
		public InvalidThreadIdException() : base( "Specified thread id is invalid." ) { }
		public InvalidThreadIdException( string message ) : base( message ) { }
		public InvalidThreadIdException( string message, Exception inner ) : base( message, inner ) { }
		protected InvalidThreadIdException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class ClassLoaderException : VMException {
		public ClassLoaderException() : base( "An exception occured while loading a class." ) { }
		public ClassLoaderException( string message ) : base( message ) { }
		public ClassLoaderException( string message, Exception inner ) : base( message, inner ) { }
		protected ClassLoaderException(
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
	public class InvalidCastException : VMAppException {
		public InvalidCastException() : base( "Object can not be cast to specified type." ) { }
		public InvalidCastException( string message ) : base( message ) { }
		public InvalidCastException( string message, Exception inner ) : base( message, inner ) { }
		protected InvalidCastException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class ArgumentException : VMAppException {
		public readonly string Argument;

		public ArgumentException() { }
		public ArgumentException( string message ) : base( message ) { }
		public ArgumentException( string message, string argument ) : base( message ) { this.Argument = argument; }
		public ArgumentException( string message, Exception inner ) : base( message, inner ) { }
		protected ArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}

	[global::System.Serializable]
	public class ArgumentOutOfRangeException : ArgumentException {
		public ArgumentOutOfRangeException() { }
		public ArgumentOutOfRangeException( string message, string argument ) : base( message, argument ) { }
		public ArgumentOutOfRangeException( string argument ) : this( "The argument was out of bounds for the specified operation.", argument ) { }
		public ArgumentOutOfRangeException( string message, Exception inner ) : base( message, inner ) { }
		protected ArgumentOutOfRangeException(
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
			: base( "Message '" + invalidMessage.ToString() + "' not understood." ) {
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
		public ClassNotFoundException( Handle<VMObjects.String> className ) : base( "Class '" + className.ToString() + "' not found" ) { ClassName = className; }
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
		public UnknownSystemCallException( Handle<VMObjects.String> systemCall ) : this( systemCall, "Unknown system call: '" + systemCall.ToString() + "'." ) { }
		public UnknownSystemCallException( Handle<VMObjects.String> systemCall, string message ) : base( message ) { SystemCallName = systemCall; }
		public UnknownSystemCallException( string message, Exception inner ) : base( message, inner ) { }
		protected UnknownSystemCallException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }
	}
}

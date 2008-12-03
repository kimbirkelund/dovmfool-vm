using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	[global::System.Serializable]
	public class VMException : ApplicationException {
		public new Handle<VM.VMObjects.String> Message { get; private set; }

		public VMException() { }
		public VMException( Handle<VM.VMObjects.String> message ) : this( message, null ) { }
		public VMException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message.Value.ToString(), inner ) { this.Message = message; }
		protected VMException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public static VMException MakeDotNetException( Handle<AppObject> obj ) {
			var cls = obj.Class().ToHandle();
			var name = cls.Name().ToString();
			var eType = Type.GetType( "VM." + name, false );

			if (eType != null && typeof( VMException ).IsAssignableFrom( eType )) {
				var e = (VMException) eType.GetConstructor( Type.EmptyTypes ).Invoke( null );
				e.InitializeFromVMException( obj );
				return e;
			}

			return new VMException( obj.Send( KnownStrings.to_string_0 ).To<VM.VMObjects.String>() );
		}

		internal virtual Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_Exception ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}

		protected virtual void InitializeFromVMException( Handle<AppObject> ex ) {
			this.Message = ex.Send( KnownStrings.message_0 ).To<VMObjects.String>();
		}

		public override string ToString() {
			return Message.Value.ToString();
		}
	}

	[global::System.Serializable]
	public class InvalidThreadIdException : VMException {
		public InvalidThreadIdException() : base( "Specified thread id is invalid.".ToVMString() ) { }
		public InvalidThreadIdException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidThreadIdException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidThreadIdException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_InvalidThreadIdException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class ClassLoaderException : VMException {
		public ClassLoaderException() : base( "An exception occured while loading a class.".ToVMString() ) { }
		public ClassLoaderException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public ClassLoaderException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ClassLoaderException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_ClassLoaderException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class InvalidVMProgramException : VMException {
		public InvalidVMProgramException() : base( "The executing program is invalid.".ToVMString() ) { }
		public InvalidVMProgramException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidVMProgramException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidVMProgramException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_InvalidVMProgramException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class OutOfMemoryException : VMException {
		public OutOfMemoryException() : base( "Heap memory has been exhausted.".ToVMString() ) { }
		public OutOfMemoryException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public OutOfMemoryException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected OutOfMemoryException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_OutOfMemoryException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class InterpretorException : VMException {
		public InterpretorException() : base( "An unknown error occured in the interpretor.".ToVMString() ) { }
		public InterpretorException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InterpretorException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InterpretorException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_InterpretorException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class InterpretorFailedToStopException : InterpretorException {
		public InterpretorFailedToStopException() : base( "Interpretor failed to stop.".ToVMString() ) { }
		public InterpretorFailedToStopException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InterpretorFailedToStopException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InterpretorFailedToStopException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_InterpretorFailedToStopException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class VMAppException : VMException {
		public VMAppException() { }
		public VMAppException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public VMAppException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected VMAppException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_ApplicationException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class InvalidCastException : VMAppException {
		public InvalidCastException() : base( "Object can not be cast to specified type.".ToVMString() ) { }
		public InvalidCastException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidCastException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidCastException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_InvalidCastException ).ToHandle();
			e.Send( KnownStrings.initialize_1, Message.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class ArgumentException : VMAppException {
		public readonly Handle<VMObjects.String> Argument;

		public ArgumentException() { }
		public ArgumentException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public ArgumentException( Handle<VM.VMObjects.String> message, Handle<VM.VMObjects.String> argument ) : base( message ) { this.Argument = argument; }
		public ArgumentException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_ArgumentException ).ToHandle();
			e.Send( KnownStrings.initialize_2, Message.To<AppObject>(), Argument.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class ArgumentOutOfRangeException : ArgumentException {
		public ArgumentOutOfRangeException() { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> message, Handle<VM.VMObjects.String> argument ) : base( message, argument ) { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> argument ) : this( "The argument was out of bounds for the specified operation.".ToVMString(), argument ) { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ArgumentOutOfRangeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_ArgumentOutOfRangeException ).ToHandle();
			e.Send( KnownStrings.initialize_2, Message.To<AppObject>(), Argument.To<AppObject>() );
			return e;
		}
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

		public MessageNotUnderstoodException() : base( "Message not understood.".ToVMString() ) { }
		public MessageNotUnderstoodException( Handle<VM.VMObjects.String> errorMessage ) : base( errorMessage ) { }
		public MessageNotUnderstoodException( Handle<VM.VMObjects.String> errorMessage, Exception inner ) : base( errorMessage, inner ) { }
		protected MessageNotUnderstoodException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public MessageNotUnderstoodException( Handle<VMObjects.String> invalidMessage, Handle<VMObjects.AppObject> obj )
			: base( ("Message '" + invalidMessage.ToString() + "' not understood.").ToVMString() ) {
			this.InvalidMessage = invalidMessage;
			this.Object = obj;
		}

		public MessageNotUnderstoodException( Handle<VMObjects.String> errorMessage, Handle<VMObjects.String> invalidMessage )
			: this( errorMessage ) {
			this.InvalidMessage = invalidMessage;
		}

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_MessageNotUnderstoodException ).ToHandle();
			e.Send( KnownStrings.initialize_3, Message.To<AppObject>(), InvalidMessage.To<AppObject>(), Object );
			return e;
		}
	}

	[global::System.Serializable]
	public class ClassNotFoundException : VMAppException {
		public readonly Handle<VMObjects.String> ClassName;

		public ClassNotFoundException() { }
		public ClassNotFoundException( Handle<VMObjects.String> className ) : base( ("Class '" + className.ToString() + "' not found").ToVMString() ) { ClassName = className; }
		public ClassNotFoundException( Handle<VM.VMObjects.String> message, Handle<VMObjects.String> className ) : base( message ) { ClassName = className; }
		public ClassNotFoundException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ClassNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_ClassNotFoundException ).ToHandle();
			e.Send( KnownStrings.initialize_2, Message.To<AppObject>(), ClassName.To<AppObject>() );
			return e;
		}
	}

	[global::System.Serializable]
	public class UnknownExternalCallException : VMAppException {
		public Handle<VMObjects.String> ExternalCallName { get; private set; }

		public UnknownExternalCallException() : base( "Unknown external call.".ToVMString() ) { }
		public UnknownExternalCallException( Handle<VMObjects.String> externalCall ) : this( externalCall, ("Unknown external call: '" + externalCall.ToString() + "'.").ToVMString() ) { }
		public UnknownExternalCallException( Handle<VMObjects.String> externalCall, Handle<VM.VMObjects.String> message ) : base( message ) { ExternalCallName = externalCall; }
		public UnknownExternalCallException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected UnknownExternalCallException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override Handle<AppObject> ToVMException() {
			var e = AppObject.CreateInstance( KnownClasses.System_UnknownExternalCallException ).ToHandle();
			e.Send( KnownStrings.initialize_2, Message.To<AppObject>(), ExternalCallName.To<AppObject>() );
			return e;
		}
	}
}

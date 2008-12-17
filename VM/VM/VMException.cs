using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	[global::System.Serializable]
	class VMException : ApplicationException, IDisposable {
		public new Handle<VM.VMObjects.String> Message { get; private set; }

		public VMException() { }
		public VMException( Handle<VM.VMObjects.String> message ) : this( message, null ) { }
		public VMException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message.ToString(), inner ) { this.Message = message; }
		protected VMException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public static VMException MakeDotNetException( Handle<AppObject> obj ) {
			using (var cls = obj.Class().ToHandle()) {
				var name = cls.Name().ToString();
				var eType = Type.GetType( "VM." + name, false );

				if (eType != null && typeof( VMException ).IsAssignableFrom( eType )) {
					var e = (VMException) eType.GetConstructor( Type.EmptyTypes ).Invoke( null );
					e.InitializeFromVMException( obj );
					return e;
				}

				return new VMException( obj.Send( KnownStrings.to_string_0 ).To<VM.VMObjects.String>() );
			}
		}

		internal virtual AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_Exception ).ToHandle())
			using (var hMessage = Message.To<AppObject>()) {
				e.Send( KnownStrings.initialize_1, hMessage );
				return e.Value;
			}
		}

		protected virtual void InitializeFromVMException( Handle<AppObject> ex ) {
			this.Message = ex.Send( KnownStrings.message_0 ).To<VMObjects.String>();
		}

		public override string ToString() {
			return Message.Value.ToString();
		}

		public virtual void Dispose() {
			if (Message != null)
				Message.Dispose();
			Message = null;
		}
	}

	[global::System.Serializable]
	class InvalidThreadIdException : VMException {
		public InvalidThreadIdException() : base( "Specified thread id is invalid.".ToVMString().ToHandle() ) { }
		public InvalidThreadIdException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidThreadIdException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidThreadIdException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_InvalidThreadIdException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class ClassLoaderException : VMException {
		public ClassLoaderException() : base( "An exception occured while loading a class.".ToVMString().ToHandle() ) { }
		public ClassLoaderException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public ClassLoaderException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ClassLoaderException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_ClassLoaderException ).ToHandle())
			using (var hMessage = Message.To<AppObject>()) {
				e.Send( KnownStrings.initialize_1, hMessage );
				return e.Value;
			}
		}
	}

	[global::System.Serializable]
	class InvalidVMProgramException : VMException {
		public InvalidVMProgramException() : base( "The executing program is invalid.".ToVMString().ToHandle() ) { }
		public InvalidVMProgramException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidVMProgramException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidVMProgramException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_InvalidVMProgramException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class OutOfMemoryException : VMException {
		Handle<AppObject> oomExcep;

		public OutOfMemoryException( bool createVMInstance )
			: base() {
			if (createVMInstance) {
				oomExcep = AppObject.CreateInstance( KnownClasses.System_OutOfMemoryException ).ToHandle();
				using (var str = "Heap memory has been exhausted.".ToVMString().ToHandle())
				using (var hMessage = str.To<AppObject>())
					oomExcep.Send( KnownStrings.initialize_1, hMessage );
			}
		}
		public OutOfMemoryException() : this( true ) { }

		internal override AppObject ToVMException() {
			if (oomExcep == null) {
				oomExcep = AppObject.CreateInstance( KnownClasses.System_OutOfMemoryException ).ToHandle();
				using (var hMessage = Message.To<AppObject>())
					oomExcep.Send( KnownStrings.initialize_1, hMessage );
			}
			return oomExcep;
		}

		public override void Dispose() {
			base.Dispose();

			if (oomExcep != null)
				oomExcep.Dispose();
			oomExcep = null;
		}

		public override string ToString() {
			return "Heap memory has been exhausted.";
		}
	}

	[global::System.Serializable]
	class InterpreterException : VMException {
		public InterpreterException() : base( "An unknown error occured in the interpreter.".ToVMString().ToHandle() ) { }
		public InterpreterException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InterpreterException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InterpreterException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_InterpreterException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class InterpreterFailedToStopException : InterpreterException {
		public InterpreterFailedToStopException() : base( "Interpreter failed to stop.".ToVMString().ToHandle() ) { }
		public InterpreterFailedToStopException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InterpreterFailedToStopException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InterpreterFailedToStopException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_InterpreterFailedToStopException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class VMAppException : VMException {
		public VMAppException() { }
		public VMAppException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public VMAppException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected VMAppException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_ApplicationException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class InvalidCastException : VMAppException {
		public InvalidCastException() : base( "Object can not be cast to specified type.".ToVMString().ToHandle() ) { }
		public InvalidCastException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public InvalidCastException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected InvalidCastException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_InvalidCastException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
					e.Send( KnownStrings.initialize_1, hMessage );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class ArgumentException : VMAppException {
		public Handle<VMObjects.String> Argument { get; private set; }

		public ArgumentException() { }
		public ArgumentException( Handle<VM.VMObjects.String> message ) : base( message ) { }
		public ArgumentException( Handle<VM.VMObjects.String> message, Handle<VM.VMObjects.String> argument ) : base( message ) { this.Argument = argument; }
		public ArgumentException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ArgumentException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }


		public override void Dispose() {
			base.Dispose();

			if (Argument != null)
				Argument.Dispose();
			Argument = null;
		}

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_ArgumentException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
				using (var hArgument = Argument.To<AppObject>())
					e.Send( KnownStrings.initialize_2, hMessage, hArgument );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class ArgumentOutOfRangeException : ArgumentException {
		public ArgumentOutOfRangeException() { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> message, Handle<VM.VMObjects.String> argument ) : base( message, argument ) { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> argument ) : this( "The argument was out of bounds for the specified operation.".ToVMString().ToHandle(), argument ) { }
		public ArgumentOutOfRangeException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ArgumentOutOfRangeException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_ArgumentOutOfRangeException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
				using (var hArgument = Argument.To<AppObject>())
					e.Send( KnownStrings.initialize_2, hMessage, hArgument );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class MessageNotUnderstoodException : VMAppException {
		/// <summary>
		/// Gets the message that was not understood.
		/// </summary>
		public Handle<VMObjects.String> InvalidMessage { get; private set; }
		/// <summary>
		/// Gets the object that did not understand <c>InvalidMessage</c>.
		/// </summary>
		public Handle<VMObjects.AppObject> Object { get; private set; }

		public MessageNotUnderstoodException() : base( "Message not understood.".ToVMString().ToHandle() ) { }
		public MessageNotUnderstoodException( Handle<VM.VMObjects.String> errorMessage ) : base( errorMessage ) { }
		public MessageNotUnderstoodException( Handle<VM.VMObjects.String> errorMessage, Exception inner ) : base( errorMessage, inner ) { }
		protected MessageNotUnderstoodException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public MessageNotUnderstoodException( Handle<VMObjects.String> invalidMessage, Handle<VMObjects.AppObject> obj )
			: base( ("Message '" + invalidMessage.ToString() + "' not understood.").ToVMString().ToHandle() ) {
			this.InvalidMessage = invalidMessage;
			this.Object = obj;
		}

		public MessageNotUnderstoodException( Handle<VMObjects.String> errorMessage, Handle<VMObjects.String> invalidMessage )
			: this( errorMessage ) {
			this.InvalidMessage = invalidMessage;
		}

		public override void Dispose() {
			base.Dispose();

			if (InvalidMessage != null)
				InvalidMessage.Dispose();
			InvalidMessage = null;
			if (Object != null)
				Object.Dispose();
			Object = null;
		}

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_MessageNotUnderstoodException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
				using (var hInvalidMessage = InvalidMessage.To<AppObject>())
					e.Send( KnownStrings.initialize_3, hMessage, hInvalidMessage, Object );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class ClassNotFoundException : VMAppException {
		public Handle<VMObjects.String> ClassName { get; private set; }

		public ClassNotFoundException() { }
		public ClassNotFoundException( Handle<VMObjects.String> className ) : base( ("Class '" + className.ToString() + "' not found").ToVMString().ToHandle() ) { ClassName = className; }
		public ClassNotFoundException( Handle<VM.VMObjects.String> message, Handle<VMObjects.String> className ) : base( message ) { ClassName = className; }
		public ClassNotFoundException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected ClassNotFoundException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public override void Dispose() {
			base.Dispose();

			if (ClassName != null)
				ClassName.Dispose();
			ClassName = null;
		}

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_ClassNotFoundException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
				using (var hClassName = ClassName.To<AppObject>())
					e.Send( KnownStrings.initialize_2, hMessage, hClassName );
				return e;
			}
		}
	}

	[global::System.Serializable]
	class UnknownExternalCallException : VMAppException {
		public Handle<VMObjects.String> ExternalCallName { get; private set; }

		public UnknownExternalCallException() : base( "Unknown external call.".ToVMString().ToHandle() ) { }
		public UnknownExternalCallException( Handle<VMObjects.String> externalCall ) : this( externalCall, ("Unknown external call: '" + externalCall.ToString() + "'.").ToVMString().ToHandle() ) { }
		public UnknownExternalCallException( Handle<VMObjects.String> externalCall, Handle<VM.VMObjects.String> message )
			: base( message ) {
			ExternalCallName = externalCall;
		}
		public UnknownExternalCallException( Handle<VM.VMObjects.String> message, Exception inner ) : base( message, inner ) { }
		protected UnknownExternalCallException(
		  System.Runtime.Serialization.SerializationInfo info,
		  System.Runtime.Serialization.StreamingContext context )
			: base( info, context ) { }

		public override void Dispose() {
			base.Dispose();

			if (ExternalCallName != null)
				ExternalCallName.Dispose();
			ExternalCallName = null;
		}

		internal override AppObject ToVMException() {
			using (var e = AppObject.CreateInstance( KnownClasses.System_UnknownExternalCallException ).ToHandle()) {
				using (var hMessage = Message.To<AppObject>())
				using (var hExternalCallName = ExternalCallName.To<AppObject>())
					e.Send( KnownStrings.initialize_2, hMessage, hExternalCallName );
				return e;
			}
		}

		protected override void InitializeFromVMException( Handle<AppObject> ex ) {
			ExternalCallName = ex.Send( KnownStrings.external_call_0 ).To<VMObjects.String>();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public enum HandleType {
		Normal,
		Weak,
	}

	public abstract class HandleBase : IDisposable {
#if DEBUG
		static int nextId;
		internal readonly int id = nextId++;
#endif

		protected int start;
		public virtual int Start {
			get { return start; }
			internal set { start = value; }
		}

		public bool IsWeak { get; private set; }
		public virtual bool IsValid { get; private set; }

		internal virtual void Unregister() {
			System.GC.SuppressFinalize( this );
			InternalUnregister();
		}

		protected virtual void InternalUnregister() {
			IsValid = false;
			MemoryManagerBase.Unregister( this );
		}

		protected HandleBase( int start, bool isWeak ) {
			Init( start, isWeak );
		}

		protected virtual void Init( int start, bool isWeak ) {
			this.start = start;
			this.IsWeak = isWeak;
			IsValid = true;
			if (IsWeak)
				System.GC.SuppressFinalize( this );
		}

		internal delegate void HandleUpdater( int newPosition );

		public void Dispose() {
			Unregister();
		}
	}

	[System.Diagnostics.DebuggerDisplay( "handle[{ToString()}]" )]
	public class Handle<T> : HandleBase where T : struct, IVMObject<T> {
		public T Value { get { return new T().New( Start ); } }
		internal Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		public Handle( T value, bool isDebug ) : base( value.Start, isDebug ) { }

		public static implicit operator T( Handle<T> handle ) {
			if (handle == null)
				return new T().New( 0 );
			return handle.Value;
		}

		public static implicit operator Word( Handle<T> handle ) {
			if (handle == null)
				return 0;
			return handle.Start;
		}

		public override string ToString() {
			return Value.ToString();
		}

		public Handle<TTo> To<TTo>() where TTo : struct, IVMObject<TTo> {
			var h = MemoryManagerBase.CreateHandle( new TTo().New( Start ), IsWeak );
			return h;
		}

		#region Equals
		public virtual bool Equals( Handle<T> value ) {
			return this.IsNull() || value.IsNull() ? this.IsNull() && value.IsNull() : new T().Equals( this, value );
		}

		public override bool Equals( object value ) {
			return this == (value as Handle<T>);
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}

		public static bool operator ==( Handle<T> value1, Handle<T> value2 ) {
			if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
				return true;
			if (object.ReferenceEquals( value1, null ))
				return value2.Equals( value1 );
			return value1.Equals( value2 );
		}

		public static bool operator !=( Handle<T> value1, Handle<T> value2 ) {
			return !(value1 == value2);
		}
		#endregion
	}

	public class IntHandle : Handle<VMObjects.AppObject> {
		int value;
		public new int Value { get { return value; } }
		public override bool IsValid { get { return false; } }
		public override int Start { get { return Value; } }

		public IntHandle( int value )
			: base( (VMObjects.AppObject) 0, false ) {
			this.value = value;
			System.GC.SuppressFinalize( this );
		}

		protected override void Init( int value, bool isDebug ) {
		}

		protected override void InternalUnregister() { }
		internal override void Unregister() { }

		public static implicit operator int( IntHandle value ) {
			return value.Value;
		}

		public static implicit operator IntHandle( int value ) {
			return new IntHandle( value );
		}

		public override bool Equals( Handle<VM.VMObjects.AppObject> value ) {
			if (object.ReferenceEquals( value, null ))
				return false;
			if (!(value is IntHandle))
				return false;
			return Value == ((IntHandle) value).Value;
		}

		public override string ToString() {
			return "inthandle[" + Value + "]";
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}
	}
}

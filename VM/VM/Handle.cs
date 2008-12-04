using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	partial class MemoryManagerBase {
		public abstract class HandleBase {
			static int createdHandles, disposedHandles;
			Container container;
			public bool IsDebug { get { return container.IsDebug; } }
			public virtual bool IsValid { get; private set; }
			public virtual int Start { get { return container.Start; } }
			internal virtual HandleUpdater Updater { get { return container.Updater; } }

			internal virtual void Unregister() {
				System.GC.SuppressFinalize( this );
				InternalUnregister();
			}

			protected virtual void InternalUnregister() {
				IsValid = false;
				MemoryManagerBase.Unregister( this );
				disposedHandles++;
			}

			protected HandleBase( int start, bool isDebug ) {
				Init( start, isDebug );
			}

			protected virtual void Init( int start, bool isDebug ) {
				container = new Container( start, isDebug );
				IsValid = true;
				createdHandles++;
			}

			~HandleBase() {
				InternalUnregister();
			}

			internal delegate void HandleUpdater( int newPosition );

			#region Container
			class Container {
				public readonly bool IsDebug;
				public int Start;
				public HandleUpdater Updater;

				public Container( int start, bool isDebug ) {
					this.Start = start;
					this.IsDebug = isDebug;
					Updater = newPosition => Start = newPosition;
				}
			}
			#endregion
		}
	}

	[System.Diagnostics.DebuggerDisplay( "handle[{ToString()}]" )]
	public class Handle<T> : MemoryManagerBase.HandleBase where T : struct, IVMObject<T> {
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
			return new Handle<TTo>( new TTo().New( Start ), IsDebug );
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
		internal override MemoryManagerBase.HandleBase.HandleUpdater Updater { get { return null; } }

		public IntHandle( int value )
			: base( (VMObjects.AppObject) 0, false ) {
			this.value = value;
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class MemoryManagerBase {
		public abstract class HandleBase {
			Container container;
			public virtual bool IsValid { get; private set; }
			public virtual int Start { get { return container.Start; } }
			public virtual HandleUpdater Updater { get { return container.Updater; } }

			public virtual void Unregister() {
				System.GC.SuppressFinalize( this );
				InternalUnregister();
			}

			protected virtual void InternalUnregister() {
				IsValid = false;
				VirtualMachine.MemoryManager.Unregister( this );
			}

			protected HandleBase( int start ) {
				Init( start );
			}

			protected virtual void Init( int start ) {
				container = new Container( start );
				IsValid = true;
			}

			~HandleBase() {
				InternalUnregister();
			}

			public delegate void HandleUpdater( int newPosition );

			#region Container
			class Container {
				public int Start;
				public HandleUpdater Updater;

				public Container( int start ) {
					this.Start = start;
					Updater = newPosition => Start = newPosition;
				}
			}
			#endregion
		}
	}

	public class Handle<T> : MemoryManagerBase.HandleBase where T : struct, IVMObject<T> {
		public T Value { get { return new T().New( Start ); } }
		internal Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		public Handle( T value ) : base( value.Start ) { }

		public static implicit operator Handle<T>( T obj ) {
			return VirtualMachine.MemoryManager.CreateHandle( obj );
		}

		public static implicit operator T( Handle<T> handle ) {
			if (handle == null)
				return new T();
			return handle.Value;
		}

		public static implicit operator Word( Handle<T> handle ) {
			if (handle == null)
				return 0;
			return handle.Start;
		}

		public override string ToString() {
			return "handle[" + Value.ToString() + "]";
		}

		public Handle<TTo> To<TTo>() where TTo : struct, IVMObject<TTo> {
			return new Handle<TTo>( new TTo().New( Start ) );
		}

		#region Equals
		public virtual bool Equals( Handle<T> value ) {
			return this.IsNull() || value.IsNull() ? this.IsNull() && value.IsNull() : Value.Equals( value.Value );
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
		public override MemoryManagerBase.HandleBase.HandleUpdater Updater { get { return null; } }

		public IntHandle( int value )
			: base( (VMObjects.AppObject) 0 ) {
			Init( value );
		}

		protected override void Init( int value ) {
			this.value = value;
		}

		protected override void InternalUnregister() { }
		public override void Unregister() { }

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
	}
}

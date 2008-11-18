using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	partial class MemoryManagerBase {
		public abstract class HandleBase {
			public bool IsValid { get; private set; }
			public int Start { get; internal set; }
			public readonly HandleUpdater Updater;

			public void Unregister() {
				System.GC.SuppressFinalize( this );
				InternalUnregister();
			}

			void InternalUnregister() {
				IsValid = false;
				VirtualMachine.MemoryManager.Unregister( this );
			}

			protected HandleBase( int start ) {
				this.Start = start;
				Updater = newPosition => Start = newPosition;
			}

			~HandleBase() {
				Unregister();
			}

			public delegate void HandleUpdater( int newPosition );
		}
	}

	public class Handle<T> : MemoryManagerBase.HandleBase where T : struct, IVMObject {
		public T Value { get { return new T { Start = Start }; } }
		internal Word this[int index] {
			get { return Value[index]; }
			set { Value[index] = value; }
		}

		public Handle( T value ) : base( value.Start ) { }

		public static explicit operator Handle<T>( T obj ) {
			return VirtualMachine.MemoryManager.CreateHandle( obj );
		}

		public static implicit operator T( Handle<T> handle ) {
			return handle.Value;
		}

		public static implicit operator Word( Handle<T> handle ) {
			return handle.Value.Start;
		}

		public override int GetHashCode() {
			return Value.GetHashCode();
		}
	}
}

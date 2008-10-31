using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public abstract class Handle<T> where T : VMObject {
		public abstract IVirtualMachine VirtualMachine { get; }
		internal abstract uint this[uint index] { get; set; }

		internal static Handle<T> Create( MemoryManagerBase memManager, uint start ) {
			return new NormalHandle( memManager, start );
		}

		public abstract Handle<TTo> To<TTo>() where TTo : VMObject;

		class NormalHandle : Handle<T> {
			uint start;
			MemoryManagerBase memManager;

			public override IVirtualMachine VirtualMachine { get { return memManager.VirtualMachine; } }

			internal override uint this[uint index] {
				get { return memManager[start + index]; }
				set { memManager[start + index] = value; }
			}

			public NormalHandle( MemoryManagerBase memManager, uint start ) {
				this.start = start;
				this.memManager = memManager;
			}

			public override Handle<TTo> To<TTo>() {
				return new Handle<TTo>.LinkedHandle<T>( this );
			}
		}

		class LinkedHandle<TFrom> : Handle<T> where TFrom : VMObject {
			public Handle<TFrom>.NormalHandle handle;

			internal override uint this[uint index] {
				get { return handle[index]; }
				set { handle[index] = value; }
			}

			public override IVirtualMachine VirtualMachine { get { return handle.VirtualMachine; } }

			public LinkedHandle( Handle<TFrom>.NormalHandle handle ) {
				this.handle = handle;
			}

			public static implicit operator Handle<TFrom>.NormalHandle( Handle<T>.LinkedHandle<TFrom> h ) {
				return h.handle;
			}

			public override Handle<TTo> To<TTo>() {
				return new Handle<TTo>.LinkedHandle<TFrom>( handle );
			}
		}
	}
}

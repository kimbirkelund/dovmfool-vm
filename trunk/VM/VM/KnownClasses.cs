using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public static class KnownClasses {
		public static Handle<Class> Object { get; private set; }
		public static Handle<Class> ObjectSet { get; private set; }
		public static Handle<Class> System { get; private set; }
		public static Handle<Class> SystemArray { get; private set; }
		public static Handle<Class> SystemInteger { get; private set; }
		public static Handle<Class> SystemString { get; private set; }
		public static Handle<Class> SystemReflectionClass { get; private set; }
		public static Handle<Class> SystemReflectionMessageHandler { get; private set; }
		public static Handle<Class> SystemReflectionVisibility { get; private set; }
		public static Handle<Class> SystemException { get; private set; }

		static KnownClasses() {
			Object = new DummyClassHandle( -1 );
			ObjectSet = new DummyClassHandle( -2 );
			System = new DummyClassHandle( -3 );
			SystemArray = new DummyClassHandle( -4 );
			SystemInteger = new DummyClassHandle( -5 );
			SystemString = new DummyClassHandle( -6 );
			SystemReflectionClass = new DummyClassHandle( -7 );
			SystemReflectionMessageHandler = new DummyClassHandle( -8 );
			SystemReflectionVisibility = new DummyClassHandle( -9 );
			SystemException = new DummyClassHandle( -10 );
		}

		internal static void Update() {
			Object = VirtualMachine.ResolveClass( null, "Object".ToVMString() ).ToHandle();
			ObjectSet = VirtualMachine.ResolveClass( null, "ObjectSet".ToVMString() ).ToHandle();
			System = VirtualMachine.ResolveClass( null, "System".ToVMString() ).ToHandle();
			SystemArray = VirtualMachine.ResolveClass( null, "System.Array".ToVMString() ).ToHandle();
			SystemInteger = VirtualMachine.ResolveClass( null, "System.Integer".ToVMString() ).ToHandle();
			SystemString = VirtualMachine.ResolveClass( null, "System.String".ToVMString() ).ToHandle();
			SystemReflectionClass = VirtualMachine.ResolveClass( null, "System.Reflection.Class".ToVMString() ).ToHandle();
			SystemReflectionMessageHandler = VirtualMachine.ResolveClass( null, "System.Reflection.MessageHandler".ToVMString() ).ToHandle();
			SystemReflectionVisibility = VirtualMachine.ResolveClass( null, "System.Reflection.Visibility".ToVMString() ).ToHandle();
			SystemException = VirtualMachine.ResolveClass( null, "System.Exception".ToVMString() ).ToHandle();
		}

		public static Class Resolve( int start ) {
			if (start > 0)
				return (Class) start;

			switch (start) {
				case -1: return Object;
				case -2: return ObjectSet;
				case -3: return System;
				case -4: return SystemArray;
				case -5: return SystemInteger;
				case -6: return SystemString;
				case -7: return SystemReflectionClass;
				case -8: return SystemReflectionMessageHandler;
				case -9: return SystemReflectionVisibility;
				default:
					throw new InvalidVMProgramException( "Trying to resolve unknown special class." );
			}
		}

		#region DummyClassHandle
		class DummyClassHandle : Handle<VMObjects.Class> {
			int value;
			public new int Value { get { return value; } }
			public override bool IsValid { get { return true; } }
			public override int Start { get { return Value; } }
			internal override MemoryManagerBase.HandleBase.HandleUpdater Updater { get { return null; } }

			public DummyClassHandle( int value )
				: base( (VMObjects.Class) 0 ) {
				Init( value );
			}

			protected override void Init( int value ) {
				this.value = value;
			}

			protected override void InternalUnregister() { }
			internal override void Unregister() { }
		}
		#endregion
	}
}

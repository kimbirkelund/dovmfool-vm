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

		static KnownClasses() {
			Object = new Handle<Class>( new Class( -1 ) );
			ObjectSet = new Handle<Class>( new Class( -2 ) );
			System = new Handle<Class>( new Class( -3 ) );
			SystemArray = new Handle<Class>( new Class( -4 ) );
			SystemInteger = new Handle<Class>( new Class( -5 ) );
			SystemString = new Handle<Class>( new Class( -6 ) );
			SystemReflectionClass = new Handle<Class>( new Class( -7 ) );
			SystemReflectionMessageHandler = new Handle<Class>( new Class( -8 ) );
			SystemReflectionVisibility = new Handle<Class>( new Class( -9 ) );
		}

		internal static void Update() {
			Object = VirtualMachine.ResolveClass( null, "Object".ToVMString() );
			ObjectSet = VirtualMachine.ResolveClass( null, "ObjectSet".ToVMString() );
			System = VirtualMachine.ResolveClass( null, "System".ToVMString() );
			SystemArray = VirtualMachine.ResolveClass( null, "System.Array".ToVMString() );
			SystemInteger = VirtualMachine.ResolveClass( null, "System.Integer".ToVMString() );
			SystemString = VirtualMachine.ResolveClass( null, "System.String".ToVMString() );
			SystemReflectionClass = VirtualMachine.ResolveClass( null, "System.Reflection.Class".ToVMString() );
			SystemReflectionMessageHandler = VirtualMachine.ResolveClass( null, "System.Reflection.MessageHandler".ToVMString() );
			SystemReflectionVisibility = VirtualMachine.ResolveClass( null, "System.Reflection.Visibility".ToVMString() );
		}

		public static Handle<Class> Resolve( int start ) {
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

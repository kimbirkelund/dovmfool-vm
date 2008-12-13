using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using System.Reflection;

namespace VM {
	internal static class KnownClasses {
		public static Handle<Class> Void { get; private set; }
		public static Handle<Class> Object { get; private set; }
		public static Handle<Class> ObjectSet { get; private set; }
		public static Handle<Class> System { get; private set; }
		public static Handle<Class> System_Array { get; private set; }
		public static Handle<Class> System_Integer { get; private set; }
		public static Handle<Class> System_String { get; private set; }
		public static Handle<Class> System_Reflection_Class { get; private set; }
		public static Handle<Class> System_Reflection_MessageHandler { get; private set; }
		public static Handle<Class> System_Reflection_Visibility { get; private set; }
		public static Handle<Class> System_Threading { get; private set; }
		public static Handle<Class> System_Threading_Thread { get; private set; }
		public static Handle<Class> System_Exception { get; private set; }
		public static Handle<Class> System_OutOfMemoryException { get; private set; }
		public static Handle<Class> System_InvalidVMProgramException { get; private set; }
		public static Handle<Class> System_InvalidThreadIdException { get; private set; }
		public static Handle<Class> System_ClassLoaderException { get; private set; }
		public static Handle<Class> System_InterpreterException { get; private set; }
		public static Handle<Class> System_InterpreterFailedToStopException { get; private set; }
		public static Handle<Class> System_ApplicationException { get; private set; }
		public static Handle<Class> System_InvalidCastException { get; private set; }
		public static Handle<Class> System_ArgumentException { get; private set; }
		public static Handle<Class> System_ArgumentOutOfRangeException { get; private set; }
		public static Handle<Class> System_ArgumentNullException { get; private set; }
		public static Handle<Class> System_MessageNotUnderstoodException { get; private set; }
		public static Handle<Class> System_ClassNotFoundException { get; private set; }
		public static Handle<Class> System_UnknownExternalCallException { get; private set; }

		static Handle<Class>[] handles;

		public static void Initialize() {
			var dummyId = -1;
			var props = typeof( KnownClasses ).GetProperties( BindingFlags.Static | BindingFlags.Public );
			handles = new Handle<Class>[props.Count() + 1];
			props.ForEach( p => {
				var h = new DummyClassHandle( p.Name, dummyId-- );
				handles[h.Start * -1] = h;
				p.SetValue( null, h, null );
			} );
		}

		internal static void Update() {
			var props = typeof( KnownClasses ).GetProperties( BindingFlags.Static | BindingFlags.Public );

			foreach (var prop in props) {
				var h = (DummyClassHandle) prop.GetValue( null, null );
				using (var hHName = h.Name.ToVMString().ToHandle())
					handles[h.Value * -1] = VirtualMachine.ResolveClass( null, hHName ).ToHandle();
				prop.SetValue( null, handles[h.Value * -1], null );
			}
			handles.Where( h => h != null ).ForEach( h => MemoryManagerBase.AssertHandle( h ) );
		}

		internal static void Dispose() {
			handles.Where( h => h != null ).ForEach( h => h.Dispose() );
		}

		public static Class Resolve( int start ) {
			if (start > 0)
				return (Class) start;
			return handles[start * -1];
		}

		#region DummyClassHandle
		class DummyClassHandle : Handle<VMObjects.Class> {
			public readonly string Name;
			int value;
			public new int Value { get { return value; } }
			public override bool IsValid { get { return true; } }
			public override int Start { get { return Value; } }

			public DummyClassHandle( string name, int value )
				: base( (VMObjects.Class) 0, false ) {
				this.Name = name.Replace( "_", "." );
				this.value = value;
				global::System.GC.SuppressFinalize( this );
			}

			protected override void Init( int value, bool isDebug ) {
			}

			protected override void InternalUnregister() { }
			internal override void Unregister() { }

			public override string ToString() {
				return Value + ": " + Name;
			}
		}
		#endregion
	}
}

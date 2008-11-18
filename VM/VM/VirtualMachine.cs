using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public static class VirtualMachine {
		internal static MemoryManagerBase MemoryManager { get; private set; }
		internal static ConstantPool ConstantPool { get; private set; }
		internal static Class StringClass { get; private set; }
		internal static Class IntegerClass { get; private set; }

		static VirtualMachine() {
			dot = ConstantPool.RegisterString( "." );
		}

		static Handle<VMObjects.String> dot;

		static Dictionary<Handle<VMObjects.String>, Handle<VMObjects.Class>> classes = new Dictionary<Handle<VM.VMObjects.String>, Handle<Class>>();

		internal static Handle<VMObjects.Class> ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className ) {
			var name = className.ToString();
			if (name.IsNullOrEmpty())
				return null;
			var names = className.Value.Split( dot );

			if (!classes.ContainsKey( names.Get<VMObjects.String>( 0 ).ToHandle() ))
				throw new ClassNotFoundException( names.Get<VMObjects.String>( 0 ).ToHandle() );
			var current = classes[names.Get<VMObjects.String>( 0 ).ToHandle()];


			for (int i = 1; i < names.Length; i++) {
				current = current.Value.ResolveClass( referencer, names.Get<VMObjects.String>( i ).ToHandle() ).ToHandle();
				if (current == null)
					throw new ClassNotFoundException( names.Get<VMObjects.String>( i ).ToHandle() );
			}

			return current;
		}

		internal static void RegisterClass( Handle<VMObjects.Class> cls ) {
			classes.Add( cls.Value.Name.ToHandle(), cls );
		}
	}
}

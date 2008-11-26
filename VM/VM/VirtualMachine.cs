using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using VM.Properties;
using System.IO;

namespace VM {
	public static class VirtualMachine {
		internal static MemoryManagerBase MemoryManager { get; private set; }
		internal static ConstantPool ConstantPool { get; private set; }
		internal static Handle<Class> SystemClass { get; private set; }
		internal static Handle<AppObject> SystemInstance { get; private set; }
		internal static Handle<Class> StringClass { get; private set; }
		internal static Handle<Class> IntegerClass { get; private set; }
		internal static Handle<Class> ArrayClass { get; private set; }
		internal static IInterpretorFactory InterpretorFactory { get; private set; }

		static VirtualMachine() {
			MemoryManager = new NoncollectingMemoryManager( 5000 );
			ConstantPool = new ConstantPool();
			InterpretorFactory = new BasicInterpretor.Factory();

			using (var loader = new ClassLoader( new MemoryStream( Resources.BaseTypes ) ))
				loader.Read();

			SystemClass = ResolveClass( null, "System".ToVMString() );
			StringClass = ResolveClass( null, "System.String".ToVMString() );
			IntegerClass = ResolveClass( null, "System.Integer".ToVMString() );
			ArrayClass = ResolveClass( null, "System.Array".ToVMString() );
		}

		static Dictionary<Handle<VMObjects.String>, Handle<VMObjects.Class>> classes = new Dictionary<Handle<VM.VMObjects.String>, Handle<Class>>();

		internal static Handle<VMObjects.Class> ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className ) {
			var name = className.ToString();
			if (name.IsNullOrEmpty())
				return null;
			var names = className.Split( VMObjects.String.Dot );

			if (!classes.ContainsKey( ((VMObjects.String) names.Get( 0 )).ToHandle() ))
				throw new ClassNotFoundException( ((VMObjects.String) names.Get( 0 )).ToHandle() );
			var current = classes[((VMObjects.String) names.Get( 0 )).ToHandle()];

			for (int i = 1; i < names.Length(); i++) {
				current = current.ResolveClass( referencer, ((VMObjects.String) names.Get( i )).ToHandle() );
				if (current == null)
					throw new ClassNotFoundException( ((VMObjects.String) names.Get( i )).ToHandle() );
			}

			return current;
		}

		internal static void RegisterClass( Handle<VMObjects.Class> cls ) {
			classes.Add( cls.Name(), cls );
		}

		public static void Execute( string inputFile ) {
			var loader = new ClassLoader( inputFile );
			var entrypoint = loader.Read();
			if (entrypoint == null)
				throw new VMException( "No entry point specified." );

			var obj = AppObject.CreateInstance( entrypoint.Class() );

			if (SystemInstance == null)
				SystemInstance = AppObject.CreateInstance( SystemClass.Value );
			var intp = InterpretorFactory.CreateInstance( obj, entrypoint, SystemInstance );

			intp.Send( ConstantPool.RegisterString( "initialize:0" ), SystemInstance );

			intp.Start();
		}
	}
}

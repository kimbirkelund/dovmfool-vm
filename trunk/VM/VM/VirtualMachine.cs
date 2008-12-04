using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using VM.Properties;
using System.IO;
using Sekhmet.Logging;

namespace VM {
	public static class VirtualMachine {
		static bool initialized;
		static int nextInterpretorId = 0;
		static Dictionary<int, InterpretorThread> interpretors = new Dictionary<int, InterpretorThread>();

		internal static MemoryManagerBase MemoryManager { get; private set; }
		internal static Handle<AppObject> SystemInstance { get; private set; }
		static Dictionary<Handle<VMObjects.String>, Handle<VMObjects.Class>> classes = new Dictionary<Handle<VM.VMObjects.String>, Handle<Class>>();
		internal static IEnumerable<Handle<Class>> Classes { get { return classes.Values; } }
		internal static Logger Logger { get; private set; }

		static HandleCache<Class> cacheClass = new HandleCache<Class>();

		internal static event EventHandler<NewThreadEventArgs> NewThread;
		static void OnNewInterpretor( InterpretorThread thread ) {
			if (NewThread != null)
				NewThread( null, new NewThreadEventArgs( thread ) );
		}

		static void Initialize() {
			if (initialized)
				return;
			initialized = true;
			Logger = new Logger();
			Logger.Handlers.Add( new ConsoleLogHandler() );

			MemoryManager = new NoncollectingMemoryManager( 20000 );
			InterpretorThread.InterpretorFactory = new Interpretor.Factory();

			KnownClasses.Initialize();
			KnownStrings.Initialize();

			var cb = typeof( VirtualMachine ).Assembly.CodeBase;
			var baseTypesFilename = cb.Substring( 0, cb.ToLower().LastIndexOf( "vm.dll" ) ) + "BaseTypes.vmil";

			using (var loader = new ClassLoader( new MemoryStream( Resources.BaseTypes ), baseTypesFilename ))
				loader.Read();

			KnownClasses.Update();
		}

		internal static VMObjects.Class ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className, bool throwOnError ) {
			var name = className.ToString();
			if (name.IsNullOrEmpty())
				return (Class) 0;
			var names = className.Split( VMObjects.String.Dot ).ToHandle();

			var name2 = names.Get<VMObjects.String>( 0 );
			if (!classes.ContainsKey( name2 ))
				throw new ClassNotFoundException( name2 );
			var current = classes[name2];
			for (int i = 1; i < names.Length(); i++) {
				name2 = names.Get<VMObjects.String>( i );
				current = cacheClass[current.ResolveInnerClass( referencer, name2 )];
				if (current == null) {
					if (throwOnError)
						throw new ClassNotFoundException( name2 );
					else
						return (Class) 0;
				}
			}

			return current;
		}

		internal static VMObjects.Class ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className ) {
			return ResolveClass( referencer, className, true );
		}

		internal static void RegisterClass( Handle<VMObjects.Class> cls ) {
			classes.Add( cls.Name().ToHandle(), cls );
		}

		public static void DebuggerDetached() {
			InterpretorThread.InterpretorFactory = new Interpretor.Factory();
			interpretors.ForEach( i => i.Value.Swap() );
		}

		public static void DebuggerAttached() {
			InterpretorThread.InterpretorFactory = new Interpretor.Factory();
			interpretors.ForEach( i => i.Value.Swap() );
		}

		public static Handle<AppObject> Execute( string inputFile ) {
			Initialize();
			var loader = new ClassLoader( inputFile );
			var entrypoint = loader.Read();
			if (entrypoint == null)
				throw new VMException( "No entry point specified.".ToVMString() );

			var obj = AppObject.CreateInstance( cacheClass[entrypoint.Class()] ).ToHandle();

			if (SystemInstance == null) {
				SystemInstance = AppObject.CreateInstance( KnownClasses.System ).ToHandle();
				var interp = Fork( KnownClasses.System.ResolveMessageHandler( null, KnownStrings.initialize_0 ).ToHandle(), SystemInstance );
				interp.Start();
				interp.Join();
			}

			var intp = Fork( entrypoint, obj, SystemInstance );
			intp.Start();
			return intp.Join();
		}

		internal static InterpretorThread Fork( Handle<VMObjects.MessageHandlerBase> messageHandler, Handle<VMObjects.AppObject> obj, params Handle<AppObject>[] arguments ) {
			lock (typeof( VirtualMachine )) {
				var interp = new InterpretorThread( nextInterpretorId++, messageHandler, obj, arguments );
				interpretors.Add( interp.Id, interp );
				return interp;
			}
		}

		internal static InterpretorThread GetInterpretor( int id ) {
			InterpretorThread interp;
			if (interpretors.TryGetValue( id, out interp ))
				return interp;
			throw new InvalidThreadIdException();
		}

		internal static IEnumerable<InterpretorThread> GetInterpretors() {
			return interpretors.Values;
		}
	}
}

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
		static int nextInterpreterId = 0;
		static Dictionary<int, InterpreterThread> interpreters = new Dictionary<int, InterpreterThread>();
		static int mainInterpreter;

		internal static OutOfMemoryException OutOfMemoryException { get; private set; }
		internal static MemoryManager MemoryManager { get; private set; }
		internal static Handle<AppObject> SystemInstance { get; private set; }
		static Dictionary<Handle<VMObjects.String>, Handle<VMObjects.Class>> classes = new Dictionary<Handle<VM.VMObjects.String>, Handle<Class>>();
		internal static IEnumerable<Handle<Class>> Classes { get { return classes.Values; } }

		static Logger logger=new Logger();
		public static Logger Logger { get { return logger; } }

		internal static event EventHandler<NewThreadEventArgs> NewThread;
		static void OnNewInterpreter( InterpreterThread thread ) {
			if (NewThread != null)
				NewThread( null, new NewThreadEventArgs( thread ) );
		}

		static void Initialize( bool useGC, int initialHeapSize, int maxHeapSize, int heapGrowFactor ) {
			if (initialized)
				return;
			initialized = true;
			logger.RunInSeperateThread = true;

			MemoryManager = new MemoryManager( useGC, initialHeapSize, maxHeapSize, heapGrowFactor );
			InterpreterThread.InterpreterFactory = new Interpreter.Factory();

			KnownClasses.Initialize();
			KnownStrings.Initialize();

			var cb = typeof( VirtualMachine ).Assembly.CodeBase;
			var baseTypesFilename = cb.Substring( 0, cb.ToLower().LastIndexOf( "vm.dll" ) ) + "BaseTypes.vmil";

			using (var loader = new ClassLoader( new MemoryStream( Resources.BaseTypes ), baseTypesFilename ))
				loader.Read();

			KnownClasses.Update();
			OutOfMemoryException = new OutOfMemoryException();
		}

		internal static VMObjects.Class ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className, bool throwOnError ) {
			var name = className.ToString();
			if (name.IsNullOrEmpty())
				return (Class) 0;
			using (var names = className.Split( KnownStrings.Dot ).ToHandle()) {
				var name2 = names.Get<VMObjects.String>( 0 ).ToHandle();
				Handle<Class> current;
				if (!classes.TryGetValue( name2, out current ))
					throw new ClassNotFoundException( name2.To<VMObjects.String>() );
				current = current.To<Class>();

				for (int i = 1; i < names.Length(); i++) {
					name2.Dispose();
					name2 = names.Get<VMObjects.String>( i ).ToHandle();
					using (var tempCls = current.ResolveInnerClass( referencer, name2 ).ToHandle()) {
						current.Dispose();
						current = tempCls;
					}
					if (current == null) {
						if (throwOnError)
							throw new ClassNotFoundException( name2.To<VMObjects.String>() );
						else
							return (Class) 0;
					}
				}
				name2.Dispose();
				using (current)
					return current.Value;
			}
		}

		internal static VMObjects.Class ResolveClass( Handle<Class> referencer, Handle<VMObjects.String> className ) {
			return ResolveClass( referencer, className, true );
		}

		internal static void RegisterClass( Handle<VMObjects.Class> cls ) {
			classes.Add( cls.Name().ToHandle(), cls );
		}

		public static void DebuggerDetached() {
			lock (typeof( VirtualMachine )) {
				InterpreterThread.InterpreterFactory = new Interpreter.Factory();
				interpreters.ForEach( i => i.Value.Swap() );
			}
		}

		public static void DebuggerAttached() {
			lock (typeof( VirtualMachine )) {
				InterpreterThread.InterpreterFactory = new Debugging.DebugInterpreter.Factory();
				interpreters.ForEach( i => i.Value.Swap() );
			}
		}

		public static void BeginExecuting( string inputFile, bool useGC, int initialHeapSize, int maxHeapSize, int heapGrowFactor ) {
			Initialize( useGC, initialHeapSize, maxHeapSize, heapGrowFactor );

			Handle<MessageHandlerBase> entrypoint;
			using (var loader = new ClassLoader( inputFile ))
				entrypoint = loader.Read();
			if (entrypoint == null)
				throw new VMException( "No entry point specified.".ToVMString().ToHandle() );

			using (var entrypointClass = entrypoint.Class().ToHandle())
			using (var obj = AppObject.CreateInstance( entrypointClass ).ToHandle()) {
				var hasSystem = SystemInstance != null;

				if (!hasSystem)
					SystemInstance = AppObject.CreateInstance( KnownClasses.System ).ToHandle();
				var intp = Fork( entrypoint, obj, SystemInstance );
				if (!hasSystem)
					intp.MessageQueue.Push( () => {
						intp.Send( KnownStrings.initialize_0, SystemInstance );
						return false;
					} );

				mainInterpreter = intp.Id;
				intp.Start();
			}
		}

		public static Handle<AppObject> EndExecuting() {
			var res = GetInterpreter( mainInterpreter ).Join();
			GetInterpreters().ForEach( i => i.Dispose() );
			classes.ForEach( c => {
				c.Key.Dispose();
				c.Value.Dispose();
			} );
			VMObjects.String.Strings().ForEach( s => s.Dispose() );
			Logger.Dispose();
			SystemInstance.Dispose();
			OutOfMemoryException.Dispose();
			KnownClasses.Dispose();
			KnownStrings.Dispose();
			return res;
		}

		public static Handle<AppObject> Execute( string inputFile, bool useGC, int initialHeapSize, int maxHeapSize, int heapGrowFactor ) {
			BeginExecuting( inputFile, useGC, initialHeapSize, maxHeapSize, heapGrowFactor );
			return EndExecuting();
		}

		internal static InterpreterThread Fork( Handle<VMObjects.MessageHandlerBase> messageHandler, Handle<VMObjects.AppObject> obj, params Handle<AppObject>[] arguments ) {
			lock (typeof( VirtualMachine )) {
				var interp = new InterpreterThread( nextInterpreterId++, messageHandler, obj, arguments );
				interpreters.Add( interp.Id, interp );
				return interp;
			}
		}

		internal static void InterpreterFinished( InterpreterThread thread ) {
			lock (typeof( VirtualMachine )) {
				interpreters.Remove( thread.Id );
				thread.Dispose();
			}
		}

		internal static InterpreterThread GetInterpreter( int id ) {
			InterpreterThread interp;
			if (interpreters.TryGetValue( id, out interp ))
				return interp;
			throw new InvalidThreadIdException();
		}

		internal static IEnumerable<InterpreterThread> GetInterpreters() {
			return interpreters.Values;
		}
	}
}

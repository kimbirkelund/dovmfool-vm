using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VM.VMObjects;

namespace VM {
	class InterpreterThread : IDisposable {
		[ThreadStatic]
		static InterpreterThread current;
		public static InterpreterThread Current {
			get { return current; }
			private set { current = value; }
		}

		public static IInterpreterFactory InterpreterFactory { get; set; }

		public int Id { get; private set; }
		public IInterpreter Interpreter { get; private set; }
		public IExecutionStack Stack { get; private set; }

		public MessageStack MessageQueue { get; private set; }

		InterpreterState state;
		public InterpreterState State {
			get {
				if (thread != null && (thread.ThreadState & ThreadState.WaitSleepJoin) != 0)
					return InterpreterState.Blocked;
				return state;
			}
			private set { state = value; }
		}

		Handle<AppObject> finalResult;
		Thread thread;
		object stackLock = new object();
		EventWaitHandle waitToFinish = new EventWaitHandle( false, EventResetMode.ManualReset ),
			returnedFromMessage = new EventWaitHandle( false, EventResetMode.AutoReset ),
			startNewMessage = new EventWaitHandle( true, EventResetMode.ManualReset );

		public InterpreterThread( int id, Handle<VMObjects.MessageHandlerBase> messageHandler, Handle<VMObjects.AppObject> obj, params Handle<AppObject>[] arguments ) {
			this.MessageQueue = new MessageStack();
			this.Id = id;
			this.Stack = InterpreterFactory.CreateStack();
			this.Interpreter = InterpreterFactory.CreateInstance( this, obj, messageHandler, arguments );
			MessageQueue.Push( () => {
				RunInterpreter();
				return false;
			} );
		}

		void StartNewThread() {
			thread = new Thread( Run );
			thread.Name = "Interpreter: " + Id;
			thread.IsBackground = true;
			State = InterpreterState.Running;
			thread.Start();
		}

		public void Start() {
			lock (this) {
				if (State == InterpreterState.Stopped)
					throw new InterpreterException( "Interpreter has been stopped.".ToVMString().ToHandle() );
				if (State != InterpreterState.NotStarted)
					throw new InterpreterException( "Interpreter already started.".ToVMString().ToHandle() );

				StartNewThread();
			}
		}

		public bool BeginPause() {
			if (Current != null && this.Id == Current.Id)
				return false;
			lock (this) {
				if (State == InterpreterState.Stopped)
					throw new InterpreterException( "Interpreter has been stopped.".ToVMString().ToHandle() );
				if (State != InterpreterState.Paused && State != InterpreterState.Running && State != InterpreterState.Blocked)
					throw new InterpreterException( "Interpreter is in neither of the states Paused, Running or Blocked.".ToVMString().ToHandle() );

				Interpreter.ReturnWhenPossible = true;
				startNewMessage.Reset();
				return true;
			}
		}

		public void EndPause() {
			if (Current != null && this.Id == Current.Id)
				return;

			while ((thread.ThreadState & ThreadState.WaitSleepJoin) == 0)
				Sleep( 100 );
			if (State == InterpreterState.Running)
				State = InterpreterState.Paused;
		}

		public void Resume() {
			lock (this) {
				if (State == InterpreterState.Stopped)
					throw new InterpreterException( "Interpreter has been stopped.".ToVMString().ToHandle() );

				Interpreter.ReturnWhenPossible = false;
				startNewMessage.Set();
				if (State == InterpreterState.Paused)
					State = InterpreterState.Running;
			}
		}

		public void Kill() {
			lock (this) {
				if (State == InterpreterState.Stopped)
					throw new InterpreterException( "Interpreter has been stopped.".ToVMString().ToHandle() );
				startNewMessage.Reset();
				State = InterpreterState.Stopped;
				if (returnedFromMessage.WaitOne( 5000, true ))
					return;
				thread.Abort();
				if (!thread.Join( 5000 ))
					throw new InterpreterFailedToStopException();
				thread = null;
			}
		}

		public Handle<AppObject> Join() {
			if (thread != null)
				thread.Join();
			return finalResult;
		}

		public void Sleep( int milis ) {
			lock (this) {
				returnedFromMessage.Set();
				Thread.Sleep( milis );
			}
		}

		public void Swap() {
			lock (this) {
				startNewMessage.Reset();
				this.Interpreter.ReturnWhenPossible = true;

				LockStack();

				this.Stack = InterpreterFactory.CreateStack( this.Stack );
				var oldIntp = this.Interpreter;
				this.Interpreter = InterpreterFactory.CreateInstance( this, this.Interpreter.CurrentPosition );

				UnlockStack();
				this.Interpreter.ReturnWhenPossible = false;
				returnedFromMessage.Reset();

				startNewMessage.Set();
				oldIntp.Dispose();

			}
		}


		void Run() {
			Current = this;
			foreach (var message in MessageQueue) {
				startNewMessage.WaitOne();
				var res = message();
				returnedFromMessage.Set();
				if (res)
					return;
			}
			finalResult = Interpreter.FinalResult;
			waitToFinish.Set();
			State = InterpreterState.Stopped;
			VirtualMachine.InterpreterFinished( this );
		}

		public void Dispose() {
			lock (this) {
				thread = null;
				if (Interpreter != null) {
					Interpreter.Dispose();
					Interpreter = null;
				}
				if (finalResult != null) {
					finalResult.Dispose();
					finalResult = null;
				}
			}
		}

		public UValue Send( Handle<VM.VMObjects.String> message, Handle<AppObject> to, params Handle<AppObject>[] arguments ) {
			LockStack();
			var currentPos = Interpreter.CurrentPosition;
			using (var hCls = to.Class().ToHandle())
			using (var newHandlerBase = hCls.ResolveMessageHandler( hCls, message ).ToHandle()) {
				if (newHandlerBase == null)
					throw new MessageNotUnderstoodException( message, to );
				Stack.Push( to.Class(), to );
				arguments.ForEach( a => Stack.Push( a.Class(), a ) );
				Stack.PushFrame( new ExecutionStack.ReturnAddress( currentPos.MessageHandler, currentPos.Position, true ), newHandlerBase );
				Interpreter.CurrentPosition = new InterpreterPosition( newHandlerBase, 0 );

				if (newHandlerBase.IsExternal()) {
					using (var newHandler = newHandlerBase.To<DelegateMessageHandler>()) {
						SystemCall method;
						using (var hExtName = newHandler.ExternalName().ToHandle())
							method = SystemCalls.FindMethod( hExtName );
						if (method == null) {
							Stack.PopFrame( false );
							throw new MessageNotUnderstoodException( message, to );
						}

						UValue ret = UValue.Null();
						MessageQueue.Push( () => {
							LockStack();
							Stack.PopFrame( false );
							UnlockStack();
							return true;
						} );
						MessageQueue.Push( () => {
							ret = method( to.ToUValue(), arguments.Select( a => a.ToUValue() ).ToArray() );
							return false;
						} );
						UnlockStack();
						Run();
						return ret;
					}
				} else {
					MessageQueue.Push( () => {
						RunInterpreter();
						return true;
					} );
					UnlockStack();
					Run();
					return Interpreter.FinalResult.ToUValue();
				}
			}
		}

		public void RunInterpreter() {
			this.LockStack();
			this.Interpreter.Run();
			this.UnlockStack();
		}

		public void LockStack() {
			Monitor.Enter( stackLock );
		}

		public void UnlockStack() {
			Monitor.Exit( stackLock );
		}

		#region MessageStack
		internal class MessageStack : IEnumerable<Func<bool>> {
			Stack<Func<bool>> stack = new Stack<Func<bool>>();

			public void Push( Func<bool> message ) {
				lock (this)
					stack.Push( message );
			}

			public IEnumerator<Func<bool>> GetEnumerator() {
				while (true) {
					Func<bool> a = null;
					lock (this) {
						if (stack.Count == 0)
							yield break;
						a = stack.Pop();
					}
					yield return a;
				}
			}

			System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
		}
		#endregion
	}
}

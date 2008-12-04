using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class Interpretor : IInterpretor {
		public readonly InterpretorData Data = new InterpretorData();

		Handle<AppObject> finalResult = null;

		static Action<Interpretor> handleException = ( _this ) => {
			var excep = _this.Data.Stack.Pop();
			if (!excep.Type.ToHandle().Is( KnownClasses.System_Exception ))
				throw new InvalidCastException();
			var _catch = _this.Data.Stack.PopTry();
			ExecutionStack.ReturnAddress? ra = null;
			while (_catch == null && (ra == null || !ra.Value.DoActualReturnHere)) {
				ra = _this.Data.Stack.PopFrame( false );
				_catch = _this.Data.Stack.PopTry();
			}
			if (ra != null && ra.Value.DoActualReturnHere)
				throw VMException.MakeDotNetException( ((AppObject) excep.Value).ToHandle() );

			if (ra != null) {
				_this.Data.Handler = ra.Value.Handler.ToHandle();
				_this.Data.PC = ra.Value.InstructionOffset;
			}
			_this.Data.PC = _catch.Value + 1;
			_this.Data.Stack.Push( excep.Type, excep.Value );
		};

		bool isDisposed = false;
		bool isBlocked = false;
		InterpretorState state;
		public InterpretorState State {
			get { return state; }
		}

		public int Id { get { return Data.Id; } }

		object externalWaitObject = new object();
		Thread thread;
		EventWaitHandle waitToPause = new EventWaitHandle( false, EventResetMode.AutoReset ),
			waitToResume = new EventWaitHandle( false, EventResetMode.AutoReset );

		HandleCache<Class> cacheClass = new HandleCache<Class>();
		HandleCache<VM.VMObjects.String> cacheString = new HandleCache<VM.VMObjects.String>();
		IntHandleCache cacheInt = new IntHandleCache();
		HandleCache<MessageHandlerBase> cacheMessageHandlerBase = new HandleCache<MessageHandlerBase>();
		HandleCache<DelegateMessageHandler> cacheDelegateMessageHandler = new HandleCache<DelegateMessageHandler>();
		HandleCache<VMILMessageHandler> cacheVMILMessageHandler = new HandleCache<VMILMessageHandler>();

		Interpretor( int id, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args )
			: this( id ) {
			Data.Stack.Push( entrypointObject.Class(), entrypointObject.Value );
			if (args.Length != entrypoint.ArgumentCount())
				throw new VMAppException( ("Entrypoint takes " + entrypoint.ArgumentCount() + " arguments, but " + args.Length + " was supplied.").ToVMString() );

			foreach (var arg in args)
				Data.Stack.Push( arg.Class(), arg.Value );
			Data.Stack.PushFrame( new ExecutionStack.ReturnAddress( (MessageHandlerBase) (-1), -1, true ), entrypoint );
			Data.Handler = entrypoint;
		}

		Interpretor( int id ) {
			Data.AttachedTo = this;
			Data.Id = id;
			Data.Stack = new ExecutionStack( 100 );
		}

		Handle<AppObject> Invoke() {
			{
				if (Data.Handler.IsExternal()) {
					var argCount = Data.Handler.ArgumentCount();
					var receiver = ((AppObject) Data.Stack.GetArgument( 0 ).Value).ToHandle();
					var handler = cacheDelegateMessageHandler[Data.Handler];
					var extName = cacheString[handler.ExternalName()];
					var method = SystemCalls.FindMethod( extName );
					if (method == null)
						throw new UnknownExternalCallException( extName );

					var args = new UValue[argCount];
					argCount.ForEachDescending( k => {
						var v = Data.Stack[argCount - k - 1];
						if (v.IsNull)
							args[k] = UValue.Null();
						else if (v.Type == KnownClasses.System_Integer.Value)
							args[k] = UValue.Int( v.Value );
						else
							args[k] = UValue.Ref( v.Type, v.Value ); ((AppObject) v.Value).ToHandle();
					} );

					var retVal = method( this, receiver.ToUValue(), args );
					return retVal.ToHandle();
				}
			}
			{
			entry:
				var receiver = ((AppObject) Data.Stack.GetArgument( 0 ).Value).ToHandle();
				var fieldOffset = receiver.GetFieldOffset( cacheClass[Data.Handler.Class()] );
				var handler = Data.Handler.To<VMILMessageHandler>();

				for (; Data.PC < handler.InstructionCount(); ) {
					if (handler.Start != Data.Handler.Start)
						System.Diagnostics.Debugger.Break();

					if (state == InterpretorState.Stopped)
						return null;
					if (state == InterpretorState.Paused) {
						waitToPause.Set();
						while (state == InterpretorState.Paused)
							waitToResume.WaitOne();
						waitToPause.Set();
					}

					try {
						var ins = handler.GetInstruction( Data.PC );
						var opcode = (VMILLib.OpCode) (ins >> 27);
						int operand = (int) (0x07FFFFFF & ins);

						switch (opcode) {
							case VMILLib.OpCode.StoreField: {
									var v = Data.Stack.Pop();
									receiver.SetField( operand + fieldOffset, v );
									Data.PC++;
									break;
								}
							case VMILLib.OpCode.LoadField:
								Data.Stack.Push( receiver.GetField( operand + fieldOffset ) );
								Data.PC++;
								break;
							case VMILLib.OpCode.StoreLocal:
								Data.Stack.SetLocal( operand, Data.Stack.Pop() );
								Data.PC++;
								break;
							case VMILLib.OpCode.LoadLocal:
								Data.Stack.Push( Data.Stack.GetLocal( operand ) );
								Data.PC++;
								break;
							case VMILLib.OpCode.LoadArgument:
								Data.Stack.Push( Data.Stack.GetArgument( operand ) );
								Data.PC++;
								break;
							case VMILLib.OpCode.PushLiteralInt:
								Data.Stack.Push( (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1) );
								Data.PC++;
								break;
							case VMILLib.OpCode.PushLiteralString:
								Data.Stack.Push( KnownClasses.System_String, VMObjects.String.GetString( operand ).Value );
								Data.PC++;
								break;
							case VMILLib.OpCode.PushLiteralIntExtend:
								int i = Data.Stack.Pop().Value;
								i += (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
								Data.Stack.Push( i );
								Data.PC++;
								break;
							case VMILLib.OpCode.Pop:
								Data.Stack.Pop();
								Data.PC++;
								break;
							case VMILLib.OpCode.Dup:
								Data.Stack.Push( Data.Stack.Peek() );
								Data.PC++;
								break;
							case VMILLib.OpCode.NewInstance: {
									var v = Data.Stack.Pop();
									if (v.Type != KnownClasses.System_String.Start)
										throw new InvalidCastException();
									var clsName = cacheString[v.Value];
									var cls = VirtualMachine.ResolveClass( cacheClass[receiver.Class()], clsName );
									var obj = VMObjects.AppObject.CreateInstance( cacheClass[cls] );
									Data.Stack.Push( cls, obj );
									Data.PC++;
									break;
								}
							case VMILLib.OpCode.SendMessage: {
									var messageVal = Data.Stack.Pop();
									if (messageVal.Type != KnownClasses.System_String.Value)
										throw new InvalidOperationException( "Value on top of stack is not a message." );
									var message = cacheString[messageVal.Value];
									var argCount = ParseArgumentCount( message );
									var newReceiver = Data.Stack[argCount];
									var newHandlerBase = cacheMessageHandlerBase[cacheClass[newReceiver.Type].ResolveMessageHandler( cacheClass[Data.Handler.Class()], message )];
									if (newHandlerBase == null)
										throw new UnknownExternalCallException( message );

									if (newHandlerBase.IsExternal()) {
										var newHandler = cacheDelegateMessageHandler[newHandlerBase.Start];
										var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
										if (method == null)
											throw new MessageNotUnderstoodException( message, ((AppObject) newReceiver.Value).ToHandle() );

										var args = new UValue[argCount];
										argCount.ForEachDescending( k => {
											var v = Data.Stack[argCount - k - 1];
											if (v.IsNull)
												args[k] = UValue.Null();
											else if (v.Type == KnownClasses.System_Integer.Value)
												args[k] = UValue.Int( v.Value );
											else
												args[k] = UValue.Ref( v.Type, v.Value ); ((AppObject) v.Value).ToHandle();
										} );

										Data.Stack.PushFrame( new ExecutionStack.ReturnAddress( Data.Handler, Data.PC + 1, false ), newHandlerBase );
										Data.PC = 0;
										Data.Handler = newHandlerBase;
										var retVal = method( this, newReceiver, args );

										var retadr = Data.Stack.PopFrame( false );
										Data.Handler = retadr.Handler.ToHandle();
										Data.PC = retadr.InstructionOffset;
										if (!retVal.IsVoid)
											Data.Stack.Push( retVal );
									} else {
										var newHandler = newHandlerBase.To<VMILMessageHandler>();
										if (newHandler.IsDefault()) {
											var arglist = VMObjects.Array.CreateInstance( argCount ).ToHandle();
											argCount.ForEachDescending( k => arglist.Set( k, Data.Stack.Pop() ) );
											Data.Stack.PushFrame( new ExecutionStack.ReturnAddress( Data.Handler, Data.PC + 1, false ), newHandlerBase );
											Data.Stack.Push( message.ToUValue() );
											Data.Stack.Push( arglist.ToUValue() );
										} else
											Data.Stack.PushFrame( new ExecutionStack.ReturnAddress( Data.Handler, Data.PC + 1, false ), newHandlerBase );
										Data.PC = 0;
										Data.Handler = newHandlerBase;
									}
									goto entry;
								}
							case VMILLib.OpCode.ReturnVoid:
							case VMILLib.OpCode.Return: {
									var ret = Data.Stack.PopFrame( opcode == VMILLib.OpCode.Return );
									Data.Handler = ret.Handler.ToHandle();
									Data.PC = ret.InstructionOffset;
									if (ret.DoActualReturnHere) {
										if (opcode == VMILLib.OpCode.ReturnVoid)
											return null;
										return ((AppObject) Data.Stack.Pop().Value).ToHandle();
									}
									goto entry;
								}
							case VMILLib.OpCode.Jump:
								Data.PC += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
								continue;
							case VMILLib.OpCode.JumpIfTrue: {
									var v = Data.Stack.Pop();
									if (v.IsNull)
										Data.PC++;
									else {
										var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Send( KnownStrings.is_true_0, ((AppObject) v.Value).ToHandle() ).Value;
										if (res > 0)
											Data.PC += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										else
											Data.PC++;
									}
									goto entry;
								}
							case VMILLib.OpCode.JumpIfFalse: {
									var v = Data.Stack.Pop();
									if (v.IsNull)
										Data.PC += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
									else {
										var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Send( KnownStrings.is_false_0, ((AppObject) v.Value).ToHandle() ).Value;
										if (res <= 0)
											Data.PC += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										else
											Data.PC++;
									}
									goto entry;
								}
							case VMILLib.OpCode.Throw:
								handleException( this );
								goto entry;
							case VMILLib.OpCode.Try:
								Data.Stack.PushTry( operand );
								Data.PC++;
								break;
							case VMILLib.OpCode.Catch:
								Data.Stack.PopTry();
								Data.PC = operand + 1;
								goto entry;
							case VMILLib.OpCode.EndTryCatch:
								Data.PC++;
								break;
							default:
								throw new InvalidVMProgramException( ("Unknown VMILLib.OpCode encountered: " + opcode).ToVMString() );
						}
					} catch (VMException ex) {
						var vmex = ex.ToVMException();
						Data.Stack.Push( vmex.Class(), vmex );
						handleException( this );
					}
				}

				throw new InvalidVMProgramException( "Reach end of message handler without returning.".ToVMString() );
			}
		}

		int ParseArgumentCount( Handle<VM.VMObjects.String> message ) {
			var count = 0;
			var pos = 1;
			for (var i = message.Length() - 1; i >= 0; i--) {
				var c = message.CharAt( i );
				if (Characters.Colon == c)
					return count;
				if (c < Characters.N0 || Characters.N9 < c)
					throw new MessageNotUnderstoodException( message );

				var v = c.Byte1 - Characters.N0.Byte1;
				count += v * pos;
				pos *= 10;
			}

			throw new MessageNotUnderstoodException( message );
		}

		public UValue Send( Handle<VM.VMObjects.String> message, Handle<AppObject> to, params Handle<AppObject>[] arguments ) {
			var newHandlerBase = cacheMessageHandlerBase[cacheClass[to.Class()].ResolveMessageHandler( cacheClass[to.Class()], message )];
			if (newHandlerBase == null)
				throw new MessageNotUnderstoodException( message, to );
			Data.Stack.Push( to.Class(), to );
			arguments.ForEach( a => Data.Stack.Push( a.Class(), a ) );
			Data.Stack.PushFrame( new ExecutionStack.ReturnAddress( Data.Handler, Data.PC, true ), newHandlerBase );
			Data.PC = 0;
			Data.Handler = newHandlerBase;

			if (newHandlerBase.IsExternal()) {
				var newHandler = newHandlerBase.To<DelegateMessageHandler>();
				var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
				if (method == null) {
					Data.Stack.PopFrame( false );
					throw new MessageNotUnderstoodException( message, to );
				}

				var ret = method( this, to.ToUValue(), arguments.Select( a => a.ToUValue() ).ToArray() );
				Data.Stack.PopFrame( false );
				return ret;
			} else
				return Invoke().ToUValue();
		}

		public void Start() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (externalWaitObject) {
				if (state != InterpretorState.NotStarted)
					throw new InterpretorException( "Interpretor already started.".ToVMString() );

				thread = new Thread( () => finalResult = Invoke() );
				thread.IsBackground = true;
				state = InterpretorState.Running;
				thread.Start();
			}
		}

		public void Pause() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (externalWaitObject) {
				if (state != InterpretorState.Paused && state != InterpretorState.Running && state != InterpretorState.Blocked)
					throw new InterpretorException( "Interpretor not in either of the states Paused, Running or Blocked.".ToVMString() );

				state = InterpretorState.Paused;
				if (state == InterpretorState.Running)
					waitToPause.WaitOne();
			}
		}

		public void Resume() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (externalWaitObject) {
				state = isBlocked ? InterpretorState.Blocked : InterpretorState.Running;
				if (!isBlocked)
					waitToResume.Set();
			}
		}

		public void Kill() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (externalWaitObject) {
				state = InterpretorState.Stopped;
				if (thread.Join( 5000 ))
					return;
				thread.Abort();
				if (!thread.Join( 5000 ))
					throw new InterpretorFailedToStopException();
				thread = null;
			}
		}

		public Handle<AppObject> Join() {
			if (thread != null && thread.ThreadState != ThreadState.Unstarted)
				thread.Join();

			return finalResult;
		}

		public void Dispose() {
			lock (externalWaitObject) {
				if (isDisposed)
					return;
				isDisposed = true;
			}

			if (State != InterpretorState.Stopped) {
				Kill();
				cacheClass.Clear();
				cacheClass = null;
				cacheDelegateMessageHandler.Clear();
				cacheDelegateMessageHandler = null;
				cacheInt.Clear();
				cacheInt = null;
				cacheMessageHandlerBase.Clear();
				cacheMessageHandlerBase = null;
				cacheString.Clear();
				cacheString = null;
				cacheVMILMessageHandler.Clear();
				cacheVMILMessageHandler = null;
			}
		}

		public class Factory : IInterpretorFactory {
			public IInterpretor CreateInstance( int id, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args ) {
				return new Interpretor( id, entrypointObject, entrypoint, args );
			}

			public IInterpretor CreateInstance( int id ) {
				return new Interpretor( id );
			}
		}
	}
}

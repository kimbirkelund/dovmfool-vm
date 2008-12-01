using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class BasicInterpretor : IInterpretor {
		static Handle<VM.VMObjects.String> isTrueStr = "is-true:0".ToVMString().Intern();
		static Handle<VM.VMObjects.String> isFalseStr = "is-false:0".ToVMString().Intern();
		static Action<BasicInterpretor, int> handleException = ( _this, doActualReturnAt ) => {
			var excep = _this.stack.Pop();
			if (!excep.Type.ToHandle().Is( KnownClasses.SystemException ))
				throw new InvalidCastException();
			var _catch = _this.stack.PopTry();
			ExecutionStack.ReturnAddress? ra = null;
			while (_catch == null && _this.stack.StackPointer >= doActualReturnAt) {
				ra = _this.stack.PopFrame( false );
				_catch = _this.stack.PopTry();
			}
			if (_this.stack.StackPointer < doActualReturnAt)
				throw VMException.MakeDotNetException( ((AppObject) excep.Value).ToHandle() );

			if (ra != null) {
				_this.handler = ra.Value.Handler.ToHandle();
				_this.pc = ra.Value.InstructionOffset;
			}
			_this.pc = _catch.Value;
			_this.stack.Push( excep.Type, excep.Value );
		};

		bool isBlocked = false;
		InterpretorState state;
		public InterpretorState State {
			get { return state; }
		}

		object externalWaitObject = new object();
		Thread thread;
		ExecutionStack stack = new ExecutionStack( 100 );
		Handle<VMILMessageHandler> handler;
		int pc = 0;
		EventWaitHandle waitToPause = new EventWaitHandle( false, EventResetMode.AutoReset ),
			waitToResume = new EventWaitHandle( false, EventResetMode.AutoReset );

		HandleCache<Class> cacheClass = new HandleCache<Class>();
		HandleCache<VM.VMObjects.String> cacheString = new HandleCache<VM.VMObjects.String>();
		IntHandleCache cacheInt = new IntHandleCache();
		HandleCache<MessageHandlerBase> cacheMessageHandlerBase = new HandleCache<MessageHandlerBase>();
		HandleCache<DelegateMessageHandler> cacheDelegateMessageHandler = new HandleCache<DelegateMessageHandler>();
		HandleCache<VMILMessageHandler> cacheVMILMessageHandler = new HandleCache<VMILMessageHandler>();

		BasicInterpretor( Handle<AppObject> entrypointObject, Handle<VMILMessageHandler> entrypoint, params Handle<AppObject>[] args ) {
			stack.Push( entrypointObject.Class(), entrypointObject.Value );
			if (args.Length != entrypoint.ArgumentCount())
				throw new VMAppException( "Entrypoint takes " + entrypoint.ArgumentCount() + " arguments, but " + args.Length + " was supplied." );

			foreach (var arg in args)
				stack.Push( arg.Class(), arg.Value );
			stack.PushFrame( new ExecutionStack.ReturnAddress(), entrypoint );
			handler = entrypoint;
		}

		BasicInterpretor() {
		}

		Handle<AppObject> Invoke() {
			var doActualReturnAt = stack.StackPointer;

		entry:
			var receiver = ((AppObject) stack.GetArgument( 0 ).Value).ToHandle();
			int fieldOffset = receiver.GetFieldOffset( cacheClass[handler.Class()] );

			for (; pc < handler.InstructionCount(); ) {
				if (state == InterpretorState.Stopped)
					return null;
				if (state == InterpretorState.Paused) {
					waitToPause.Set();
					while (state == InterpretorState.Paused)
						waitToResume.WaitOne();
					waitToPause.Set();
				}

				try {
					var ins = handler.GetInstruction( pc );
					var opcode = (VMILLib.OpCode) (ins >> 27);
					int operand = (int) (0x07FFFFFF & ins);

					switch (opcode) {
						case VMILLib.OpCode.StoreField: {
								var v = stack.Pop();
								receiver.SetField( operand + fieldOffset, v );
								break;
							}
						case VMILLib.OpCode.LoadField:
							stack.Push( receiver.GetField( operand + fieldOffset ) );
							break;
						case VMILLib.OpCode.StoreLocal:
							stack.SetLocal( operand, stack.Pop() );
							break;
						case VMILLib.OpCode.LoadLocal:
							stack.Push( stack.GetLocal( operand ) );
							break;
						case VMILLib.OpCode.LoadArgument:
							stack.Push( stack.GetArgument( operand ) );
							break;
						case VMILLib.OpCode.PushLiteralInt:
							stack.Push( (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1) );
							break;
						case VMILLib.OpCode.PushLiteralString:
							stack.Push( KnownClasses.SystemString, VMObjects.String.GetString( operand ).Value );
							break;
						case VMILLib.OpCode.PushLiteralIntExtend:
							int i = stack.Pop().Value;
							i += (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
							stack.Push( i );
							break;
						case VMILLib.OpCode.Pop:
							stack.Pop();
							break;
						case VMILLib.OpCode.Dup:
							stack.Push( stack.Peek() );
							break;
						case VMILLib.OpCode.NewInstance: {
								var v = stack.Pop();
								if (v.Type != KnownClasses.SystemString.Start)
									throw new InvalidCastException();
								var clsName = cacheString[v.Value];
								var cls = VirtualMachine.ResolveClass( cacheClass[receiver.Class()], clsName );
								var obj = VMObjects.AppObject.CreateInstance( cacheClass[cls] );
								stack.Push( cls, obj );
								break;
							}
						case VMILLib.OpCode.SendMessage: {
								var messageVal = stack.Pop();
								if (messageVal.Type != KnownClasses.SystemString.Value)
									throw new InvalidOperationException( "Value on top of stack is not a message." );
								var message = cacheString[messageVal.Value];
								var argCount = ParseArgumentCount( message );
								var newReceiver = stack[argCount];
								var newHandlerBase = cacheMessageHandlerBase[cacheClass[newReceiver.Type].ResolveMessageHandler( cacheClass[handler.Class()], message )];
								if (newHandlerBase == null)
									throw new MessageNotUnderstoodException( message, ((AppObject) newReceiver.Value).ToHandle() );

								if (newHandlerBase.IsExternal()) {
									var newHandler = cacheDelegateMessageHandler[newHandlerBase.Start];
									var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
									if (method == null)
										throw new MessageNotUnderstoodException( message, ((AppObject) newReceiver.Value).ToHandle() );

									var args = new UValue[argCount];
									argCount.ForEachDescending( k => {
										var v = stack.Pop();
										if (v.IsNull)
											args[k] = UValue.Null();
										else if (v.Type == KnownClasses.SystemInteger.Value)
											args[k] = UValue.Int( v.Value );
										else
											args[k] = UValue.Ref( v.Type, v.Value ); ((AppObject) v.Value).ToHandle();
									} );
									stack.Pop();
									var newReceiver2 = newReceiver.Type == KnownClasses.SystemInteger.Value ? UValue.Int( newReceiver.Value ) : UValue.Ref( newReceiver.Type, newReceiver.Value );
									var ret = method( this, newReceiver2, args );
									if (!ret.IsVoid)
										stack.Push( ret.Type, ret.Value );
									break;
								} else {
									var newHandler = newHandlerBase.To<VMILMessageHandler>();
									if (newHandler.IsDefault()) {
										var arglist = VMObjects.Array.CreateInstance( argCount ).ToHandle();
										argCount.ForEachDescending( k => arglist.Set( k, stack.Pop() ) );
										stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc + 1 ), newHandler );
										stack.Push( message.Class(), message );
										stack.Push( arglist.Class(), arglist );
									} else
										stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc + 1 ), newHandler );
									pc = 0;
									handler = newHandler;
									goto entry;
								}
							}
						case VMILLib.OpCode.ReturnVoid:
						case VMILLib.OpCode.Return: {
								var ret = stack.PopFrame( opcode == VMILLib.OpCode.Return );
								handler = ret.Handler.ToHandle();
								pc = ret.InstructionOffset;
								if (stack.StackPointer < doActualReturnAt) {
									if (opcode == VMILLib.OpCode.ReturnVoid)
										return null;
									return ((AppObject) stack.Pop().Value).ToHandle();
								}
								goto entry;
							}
						case VMILLib.OpCode.Jump:
							pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
							continue;
						case VMILLib.OpCode.JumpIfTrue: {
								var v = stack.Pop();
								if (v.IsNull) {
									break;
								} else if ((v.Type == KnownClasses.SystemInteger.Value || v.Type == KnownClasses.SystemString.Value)) {
									if (((int) v.Value) > 0) {
										pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										continue;
									}
								} else {
									var res = Send( isTrueStr, ((AppObject) v.Value).ToHandle() );
									if (res.Value > 0) {
										pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										continue;
									}
								}
								break;
							}
						case VMILLib.OpCode.JumpIfFalse: {
								var v = stack.Pop();
								if (v.IsNull) {
									pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
									continue;
								} else if ((v.Type == KnownClasses.SystemInteger.Value || v.Type == KnownClasses.SystemString.Value)) {
									if (((int) v.Value) <= 0) {
										pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										continue;
									}
								} else {
									var res = Send( isTrueStr, ((AppObject) v.Value).ToHandle() );
									if (res.Value > 0) {
										pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										continue;
									}
								}
								break;
							}
						case VMILLib.OpCode.Throw:
							handleException( this, doActualReturnAt );
							break;
						case VMILLib.OpCode.Try:
							stack.PushTry( operand );
							break;
						case VMILLib.OpCode.Catch:
							stack.PopTry();
							pc = operand;
							break;
						case VMILLib.OpCode.EndTryCatch:
							break;
						default:
							throw new InvalidVMProgramException( "Unknown VMILLib.OpCode encountered: " + opcode );
					}
				} catch (VMException ex) {
					var vmex = ex.ToVMException();
					stack.Push( vmex.Class(), vmex );
					handleException( this, doActualReturnAt );
				}
				pc++;
			}

			throw new InvalidVMProgramException( "Reach end of message handler without returning." );
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
			if (newHandlerBase.IsExternal()) {
				var newHandler = newHandlerBase.To<DelegateMessageHandler>();
				var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
				if (method == null)
					throw new MessageNotUnderstoodException( message, to );

				return method( this, to.ToUValue(), arguments.Select( a => a.ToUValue() ).ToArray() );
			} else {
				var newHandler = newHandlerBase.To<VMILMessageHandler>();
				if (newHandler == null)
					throw new MessageNotUnderstoodException( message, to );

				stack.Push( to.Class(), to );
				arguments.ForEach( a => stack.Push( a.Class(), a ) );

				stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc ), newHandler );
				pc = 0;
				handler = newHandler;
				return Invoke().ToUValue();
			}
		}

		public void Start() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped." );
			lock (externalWaitObject) {
				if (state != InterpretorState.NotStarted)
					throw new InterpretorException( "Interpretor already started." );

				thread = new Thread( () => Invoke() );
				state = InterpretorState.Running;
				thread.Start();
			}
		}

		public void Pause() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped." );
			lock (externalWaitObject) {
				if (state != InterpretorState.Paused && state != InterpretorState.Running && state != InterpretorState.Blocked)
					throw new InterpretorException( "Interpretor not in either of the states Paused, Running or Blocked." );

				state = InterpretorState.Paused;
				if (state == InterpretorState.Running)
					waitToPause.WaitOne();
			}
		}

		public void Resume() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped." );
			lock (externalWaitObject) {
				state = isBlocked ? InterpretorState.Blocked : InterpretorState.Running;
				if (!isBlocked)
					waitToResume.Set();
			}
		}

		public void Kill() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped." );
			lock (externalWaitObject) {
				state = InterpretorState.Stopped;
				if (thread.Join( 5000 ))
					return;
				thread.Abort();
				if (!thread.Join( 5000 ))
					throw new InterpretorFailedToStopException();
			}
		}

		public void Join() {
			if (thread != null && thread.ThreadState != ThreadState.Unstarted)
				thread.Join();
		}

		public class Factory : IInterpretorFactory {
			public IInterpretor CreateInstance( Handle<AppObject> entrypointObject, Handle<VMILMessageHandler> entrypoint, params Handle<AppObject>[] args ) {
				return new BasicInterpretor( entrypointObject, entrypoint, args );
			}

			public IInterpretor CreateInstance() {
				return new BasicInterpretor();
			}
		}
	}
}

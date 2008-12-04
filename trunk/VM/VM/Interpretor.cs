using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class Interpretor : IInterpretor {
		public InterpretorThread Thread { get; private set; }
		public InterpretorPosition CurrentPosition { get { return new InterpretorPosition( handlerBase, pc ); } }
		public Handle<AppObject> FinalResult { get; private set; }

		int pc;
		Handle<MessageHandlerBase> handlerBase;

		static Action<Interpretor> handleException = ( _this ) => {
			var excep = _this.stack.Pop();
			if (!excep.Type.ToHandle().Is( KnownClasses.System_Exception ))
				throw new InvalidCastException();
			var _catch = _this.stack.PopTry();
			ExecutionStack.ReturnAddress? ra = null;
			while (_catch == null && (ra == null || !ra.Value.DoActualReturnHere)) {
				ra = _this.stack.PopFrame( false );
				_catch = _this.stack.PopTry();
			}
			if (ra != null && ra.Value.DoActualReturnHere)
				throw VMException.MakeDotNetException( excep.ToHandle() );

			if (ra != null) {
				_this.handlerBase = ra.Value.Handler.ToHandle();
				_this.pc = ra.Value.InstructionOffset;
			}
			_this.pc = _catch.Value + 1;
			_this.stack.Push( excep.Type, excep.Value );
		};

		bool isDisposed = false;

		IExecutionStack stack;
		HandleCache<Class> cacheClass = new HandleCache<Class>();
		HandleCache<VM.VMObjects.String> cacheString = new HandleCache<VM.VMObjects.String>();
		IntHandleCache cacheInt = new IntHandleCache();
		HandleCache<MessageHandlerBase> cacheMessageHandlerBase = new HandleCache<MessageHandlerBase>();
		HandleCache<DelegateMessageHandler> cacheDelegateMessageHandler = new HandleCache<DelegateMessageHandler>();
		HandleCache<VMILMessageHandler> cacheVMILMessageHandler = new HandleCache<VMILMessageHandler>();

		Interpretor( InterpretorThread thread, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args )
			: this( thread ) {
			this.stack = thread.Stack;
			stack.Push( entrypointObject.Class(), entrypointObject.Value );
			if (args.Length != entrypoint.ArgumentCount())
				throw new VMAppException( ("Entrypoint takes " + entrypoint.ArgumentCount() + " arguments, but " + args.Length + " was supplied.").ToVMString() );

			foreach (var arg in args)
				stack.Push( arg.Class(), arg.Value );
			stack.PushFrame( new ExecutionStack.ReturnAddress( (MessageHandlerBase) (-1), -1, true ), entrypoint );
			handlerBase = entrypoint;
			pc = 0;
		}

		Interpretor( InterpretorThread thread ) {
			this.Thread = thread;
		}

		Interpretor( InterpretorThread thread, InterpretorPosition position )
			: this( thread ) {
			this.handlerBase = position.MessageHandler;
			this.pc = position.Position;
		}

		public void Run() {
			{
				if (handlerBase.IsExternal()) {
					var handler = cacheDelegateMessageHandler[handlerBase];
					var argCount = handler.ArgumentCount();
					var receiver = stack.GetArgument( 0 ).ToHandle();
					var extName = cacheString[handler.ExternalName()];
					var method = SystemCalls.FindMethod( extName );
					if (method == null)
						throw new UnknownExternalCallException( extName );

					var args = new UValue[argCount];
					argCount.ForEachDescending( k => {
						var v = stack.GetArgument( k + 1 );
						if (v.IsNull)
							args[k] = UValue.Null();
						else if (v.Type == KnownClasses.System_Integer.Value)
							args[k] = UValue.Int( v.Value );
						else
							args[k] = UValue.Ref( v.Type, v.Value ); v.ToHandle();
					} );

					var retVal = method( this, receiver.ToUValue(), args );
					FinalResult = retVal.ToHandle();
					return;
				}
			}
			{
			entry:
				var receiver = stack.GetArgument( 0 ).ToHandle();
				var handler = cacheVMILMessageHandler[handlerBase.Value];
				var fieldOffset = receiver.GetFieldOffset( cacheClass[handler.Class()] );

				for (; pc < handler.InstructionCount(); ) {
					try {
						var ins = handler.GetInstruction( pc );
						var opcode = (VMILLib.OpCode) (ins >> 27);
						int operand = (int) (0x07FFFFFF & ins);

						switch (opcode) {
							case VMILLib.OpCode.StoreField: {
									var v = stack.Pop();
									receiver.SetField( operand + fieldOffset, v );
									pc++;
									break;
								}
							case VMILLib.OpCode.LoadField:
								stack.Push( receiver.GetField( operand + fieldOffset ) );
								pc++;
								break;
							case VMILLib.OpCode.StoreLocal:
								stack.SetLocal( operand, stack.Pop() );
								pc++;
								break;
							case VMILLib.OpCode.LoadLocal:
								stack.Push( stack.GetLocal( operand ) );
								pc++;
								break;
							case VMILLib.OpCode.LoadArgument:
								stack.Push( stack.GetArgument( operand ) );
								pc++;
								break;
							case VMILLib.OpCode.PushLiteralInt:
								stack.Push( (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1) );
								pc++;
								break;
							case VMILLib.OpCode.PushLiteralString:
								stack.Push( KnownClasses.System_String, VMObjects.String.GetString( operand ).Value );
								pc++;
								break;
							case VMILLib.OpCode.PushLiteralIntExtend:
								int i = stack.Pop().Value;
								i += (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
								stack.Push( i );
								pc++;
								break;
							case VMILLib.OpCode.Pop:
								stack.Pop();
								pc++;
								break;
							case VMILLib.OpCode.Dup:
								stack.Push( stack.Peek() );
								pc++;
								break;
							case VMILLib.OpCode.NewInstance: {
									var v = stack.Pop();
									if (v.Type != KnownClasses.System_String.Start)
										throw new InvalidCastException();
									var clsName = cacheString[v.Value];
									var cls = VirtualMachine.ResolveClass( cacheClass[receiver.Class()], clsName );
									var obj = VMObjects.AppObject.CreateInstance( cacheClass[cls] );
									stack.Push( cls, obj );
									pc++;
									break;
								}
							case VMILLib.OpCode.SendMessage: {
									var messageVal = stack.Pop();
									if (messageVal.Type != KnownClasses.System_String.Value)
										throw new InvalidOperationException( "Value on top of stack is not a message." );
									var message = cacheString[messageVal.Value];
									var argCount = ParseArgumentCount( message );
									var newReceiver = stack[argCount];
									var newHandlerBase = cacheMessageHandlerBase[cacheClass[newReceiver.Type].ResolveMessageHandler( cacheClass[handler.Class()], message )];
									if (newHandlerBase == null)
										throw new UnknownExternalCallException( message );

									if (newHandlerBase.IsExternal()) {
										var newHandler = cacheDelegateMessageHandler[newHandlerBase.Start];
										var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
										if (method == null)
											throw new MessageNotUnderstoodException( message, newReceiver.ToHandle() );

										var args = new UValue[argCount];
										argCount.ForEachDescending( k => {
											var v = stack[argCount - k - 1];
											if (v.IsNull)
												args[k] = UValue.Null();
											else if (v.Type == KnownClasses.System_Integer.Value)
												args[k] = UValue.Int( v.Value );
											else
												args[k] = UValue.Ref( v.Type, v.Value );
										} );

										stack.PushFrame( new ExecutionStack.ReturnAddress( handlerBase, pc + 1, false ), newHandlerBase );
										pc = 0;
										handlerBase = newHandlerBase;
										var retVal = method( this, newReceiver, args );

										var retadr = stack.PopFrame( false );
										handlerBase = retadr.Handler.ToHandle();
										pc = retadr.InstructionOffset;
										if (!retVal.IsVoid)
											stack.Push( retVal );
									} else {
										var newHandler = newHandlerBase.To<VMILMessageHandler>();
										if (newHandler.IsDefault()) {
											var arglist = VMObjects.Array.CreateInstance( argCount ).ToHandle();
											argCount.ForEachDescending( k => arglist.Set( k, stack.Pop() ) );
											stack.PushFrame( new ExecutionStack.ReturnAddress( handlerBase, pc + 1, false ), newHandlerBase );
											stack.Push( message.ToUValue() );
											stack.Push( arglist.ToUValue() );
										} else
											stack.PushFrame( new ExecutionStack.ReturnAddress( handlerBase, pc + 1, false ), newHandlerBase );
										pc = 0;
										handlerBase = newHandlerBase;
									}
									goto entry;
								}
							case VMILLib.OpCode.ReturnVoid:
							case VMILLib.OpCode.Return: {
									var ret = stack.PopFrame( opcode == VMILLib.OpCode.Return );
									handlerBase = ret.Handler.ToHandle();
									pc = ret.InstructionOffset;
									if (ret.DoActualReturnHere) {
										if (opcode == VMILLib.OpCode.ReturnVoid) {
											FinalResult = null;
											return;
										}
										FinalResult = stack.Pop().ToHandle();
										return;
									}
									goto entry;
								}
							case VMILLib.OpCode.Jump:
								pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
								continue;
							case VMILLib.OpCode.JumpIfTrue: {
									var v = stack.Pop();
									if (v.IsNull)
										pc++;
									else {
										var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Send( KnownStrings.is_true_0, v.ToHandle() ).Value;
										if (res > 0)
											pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										else
											pc++;
									}
									goto entry;
								}
							case VMILLib.OpCode.JumpIfFalse: {
									var v = stack.Pop();
									if (v.IsNull)
										pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
									else {
										var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Send( KnownStrings.is_false_0, v.ToHandle() ).Value;
										if (res <= 0)
											pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										else
											pc++;
									}
									goto entry;
								}
							case VMILLib.OpCode.Throw:
								handleException( this );
								goto entry;
							case VMILLib.OpCode.Try:
								stack.PushTry( operand );
								pc++;
								break;
							case VMILLib.OpCode.Catch:
								stack.PopTry();
								pc = operand + 1;
								goto entry;
							case VMILLib.OpCode.EndTryCatch:
								pc++;
								break;
							default:
								throw new InvalidVMProgramException( ("Unknown VMILLib.OpCode encountered: " + opcode).ToVMString() );
						}
					} catch (VMException ex) {
						var vmex = ex.ToVMException();
						stack.Push( vmex.Class(), vmex );
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
			stack.Push( to.Class(), to );
			arguments.ForEach( a => stack.Push( a.Class(), a ) );
			stack.PushFrame( new ExecutionStack.ReturnAddress( handlerBase, pc, true ), newHandlerBase );
			pc = 0;
			handlerBase = newHandlerBase;

			if (newHandlerBase.IsExternal()) {
				var newHandler = newHandlerBase.To<DelegateMessageHandler>();
				var method = SystemCalls.FindMethod( cacheString[newHandler.ExternalName()] );
				if (method == null) {
					stack.PopFrame( false );
					throw new MessageNotUnderstoodException( message, to );
				}

				var ret = method( this, to.ToUValue(), arguments.Select( a => a.ToUValue() ).ToArray() );
				stack.PopFrame( false );
				return ret;
			} else {
				Run();
				return FinalResult.ToUValue();
			}
		}

		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}

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

		public class Factory : IInterpretorFactory {
			public IInterpretor CreateInstance( InterpretorThread thread, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args ) {
				return new Interpretor( thread, entrypointObject, entrypoint, args );
			}

			public IInterpretor CreateInstance( InterpretorThread thread ) {
				return new Interpretor( thread );
			}

			public IInterpretor CreateInstance( InterpretorThread thread, InterpretorPosition position ) {
				return new Interpretor( thread, position );
			}

			public IExecutionStack CreateStack() {
				return new ExecutionStack( 100 );
			}

			public IExecutionStack CreateStack( IExecutionStack stack ) {
				return new ExecutionStack( stack );
			}
		}
	}
}

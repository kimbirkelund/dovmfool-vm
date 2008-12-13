﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class Interpreter : IInterpreter {
		public InterpreterThread Thread { get; private set; }
		Handle<AppObject> finalResult;
		public Handle<AppObject> FinalResult {
			get { return finalResult; }
			set {
				if (!object.ReferenceEquals( finalResult, null ) && !object.ReferenceEquals( finalResult, value ))
					finalResult.Dispose();
				finalResult = value;
			}
		}
		public bool ReturnWhenPossible { get; set; }

		int pc;
		Handle<MessageHandlerBase> messageHandler;
		public InterpreterPosition CurrentPosition {
			get { return new InterpreterPosition( messageHandler, pc ); }
			set {
				var temp = value.MessageHandler.To<MessageHandlerBase>();
				value.MessageHandler.Dispose();
				if (messageHandler != null)
					messageHandler.Dispose();
				messageHandler = temp;
				pc = value.Position;
			}
		}

		bool isDisposed = false;

		IExecutionStack stack;

		Interpreter( InterpreterThread thread, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args )
			: this( thread ) {
			stack.Push( entrypointObject.Class(), entrypointObject.Value );
			if (args.Length != entrypoint.ArgumentCount())
				throw new VMAppException( ("Entrypoint takes " + entrypoint.ArgumentCount() + " arguments, but " + args.Length + " was supplied.").ToVMString().ToHandle() );

			foreach (var arg in args)
				stack.Push( arg.Class(), arg.Value );
			stack.PushFrame( new ExecutionStack.ReturnAddress( (MessageHandlerBase) (-1), -1, true ), entrypoint );
			CurrentPosition = new InterpreterPosition( entrypoint, 0 );
		}

		Interpreter( InterpreterThread thread ) {
			this.Thread = thread;
			this.stack = thread.Stack;
		}

		Interpreter( InterpreterThread thread, InterpreterPosition position )
			: this( thread ) {
			this.CurrentPosition = position;
		}

		public void Run() {
			if (isDisposed)
				throw new ObjectDisposedException( ToString() );
			{
				if (CurrentPosition.MessageHandler.IsExternal()) {
					using (var handler = CurrentPosition.MessageHandler.To<DelegateMessageHandler>())
					using (var receiver = stack.GetArgument( 0 ).ToHandle())
					using (var extName = handler.ExternalName().ToHandle()) {
						var argCount = handler.ArgumentCount();
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
								args[k] = UValue.Ref( v.Type, v.Value );
						} );

						Thread.MessageQueue.Push( () => {
							var retVal = method( receiver.ToUValue(), args );
							Thread.Interpreter.FinalResult = retVal.ToHandle();
							return false;
						} );
						return;
					}
				}
			}
			{
			entry:
				var isRethrow = false;
				using (var receiver = stack.GetArgument( 0 ).ToHandle())
				using (var handler = CurrentPosition.MessageHandler.To<VMILMessageHandler>())
				using (var handlerClass = handler.Class().ToHandle()) {
					var fieldOffset = receiver.GetFieldOffset( handlerClass );
					for (; pc < handler.InstructionCount(); ) {
						if (ReturnWhenPossible) {
							Thread.MessageQueue.Push( () => {
								Thread.RunInterpreter();
								return false;
							} );
							return;
						}

						var ins = handler.GetInstruction( pc );
						var opcode = (VMILLib.OpCode) (ins >> 27);
						var operand = (int) (0x07FFFFFF & ins);

					entry2:
						try {
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
										using (var clsName = v.ToHandle<VMObjects.String>())
										using (var hRecCls = receiver.Class().ToHandle())
										using (var cls = VirtualMachine.ResolveClass( hRecCls, clsName ).ToHandle()) {
											var obj = VMObjects.AppObject.CreateInstance( cls );
											stack.Push( cls, obj );
											pc++;
											break;
										}
									}
								case VMILLib.OpCode.SendMessage: {
										var messageVal = stack.Pop();
										if (messageVal.Type != KnownClasses.System_String.Value)
											throw new InvalidOperationException( "Value on top of stack is not a message." );
										using (var message = messageVal.ToHandle<VMObjects.String>()) {
											var argCount = ParseArgumentCount( message );
											var newReceiver = stack[argCount];
											using (var hNewReceiverType = newReceiver.Type.ToHandle()) {
												var newHandlerBaseValue = hNewReceiverType.ResolveMessageHandler( handlerClass, message );
												if (newHandlerBaseValue.IsNull())
													throw new MessageNotUnderstoodException( message, newReceiver.ToHandle() );
												using (var newHandlerBase = newHandlerBaseValue.ToHandle()) {
													if (newHandlerBase.IsExternal()) {
														using (var newHandler = newHandlerBase.To<DelegateMessageHandler>()) {
															using (var hNewHandlerExternalName = newHandler.ExternalName().ToHandle()) {
																var method = SystemCalls.FindMethod( hNewHandlerExternalName );
																if (method == null)
																	throw new UnknownExternalCallException( message );

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

																stack.PushFrame( new ExecutionStack.ReturnAddress( CurrentPosition.MessageHandler, pc + 1, false ), newHandlerBase );
																CurrentPosition = new InterpreterPosition( newHandlerBase, pc );

																Thread.MessageQueue.Push( () => {
																	Thread.RunInterpreter();
																	return false;
																} );
																Thread.MessageQueue.Push( () => {
																	var retVal = method( newReceiver, args );
																	Thread.LockStack();
																	var retadr = Thread.Stack.PopFrame( false );
																	Thread.Interpreter.CurrentPosition = new InterpreterPosition( retadr.Handler.ToWeakHandle(), retadr.InstructionOffset );
																	if (!retVal.IsVoid)
																		Thread.Stack.Push( retVal );
																	Thread.UnlockStack();
																	return false;
																} );
																return;
															}
														}
													} else {
														using (var newHandler = newHandlerBase.To<VMILMessageHandler>()) {
															if (newHandler.IsDefault()) {
																using (var arglist = VMObjects.Array.CreateInstance( argCount ).ToHandle()) {
																	argCount.ForEachDescending( k => arglist.Set( k, stack.Pop() ) );
																	stack.PushFrame( new ExecutionStack.ReturnAddress( CurrentPosition.MessageHandler, pc + 1, false ), newHandlerBase );
																	stack.Push( message.ToUValue() );
																	stack.Push( arglist.ToUValue() );
																}
															} else
																stack.PushFrame( new ExecutionStack.ReturnAddress( CurrentPosition.MessageHandler, pc + 1, false ), newHandlerBase );
															CurrentPosition = new InterpreterPosition( newHandlerBase, 0 );
														}
													}
												}
											}
										}
										goto entry;
									}
								case VMILLib.OpCode.ReturnVoid:
								case VMILLib.OpCode.Return: {
										var ret = stack.PopFrame( opcode == VMILLib.OpCode.Return );

										if (ret.Handler.Start != -1)
											CurrentPosition = new InterpreterPosition( ret.Handler.ToWeakHandle(), ret.InstructionOffset );

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
											using (var hV = v.ToHandle()) {
												var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Thread.Send( KnownStrings.is_true_0, hV ).Value;
												if (res > 0)
													pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
												else
													pc++;
											}
										}
										goto entry;
									}
								case VMILLib.OpCode.JumpIfFalse: {
										var v = stack.Pop();
										if (v.IsNull)
											pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
										else {
											using (var hV = v.ToHandle()) {
												var res = v.Type == KnownClasses.System_Integer.Value ? v.Value : Thread.Send( KnownStrings.is_false_0, hV ).Value;
												if (res <= 0)
													pc += (int) (((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF));
												else
													pc++;
											}
										}
										goto entry;
									}
								case VMILLib.OpCode.Throw:
									var excep = stack.Pop();
									using (var excepType = excep.Type.ToHandle())
										if (!excepType.Is( KnownClasses.System_Exception ))
											throw new InvalidCastException();
									var _catch = stack.PopTry();
									ExecutionStack.ReturnAddress? ra = null;
									while (_catch == null && (ra == null || !ra.Value.DoActualReturnHere) && stack.StackPointer > 0) {
										ra = stack.PopFrame( false );
										_catch = stack.PopTry();
									}
									if (ra != null && ra.Value.DoActualReturnHere || stack.StackPointer <= 0) {
										isRethrow = true;
										throw VMException.MakeDotNetException( excep.ToHandle() );
									}

									if (ra != null)
										CurrentPosition = new InterpreterPosition( ra.Value.Handler.ToWeakHandle(), ra.Value.InstructionOffset );
									pc = _catch.Value + 1;
									stack.Push( excep.Type, excep.Value );
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
									throw new InvalidVMProgramException( ("Unknown VMILLib.OpCode encountered: " + opcode).ToVMString().ToHandle() );
							}
						} catch (OutOfMemoryException) {
							throw;
						} catch (VMException ex) {
							if (isRethrow)
								throw;
							var vmex = ex.ToVMException().ToWeakHandle();
							stack.Push( vmex.Class(), vmex );
							opcode = VMILLib.OpCode.Throw;
							ex.Dispose();
							goto entry2;
						}
					}
				}
				throw new InvalidVMProgramException( "Reach end of message handler without returning.".ToVMString().ToHandle() );
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

		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}

			if (finalResult != null) {
				finalResult.Dispose();
				finalResult = null;
			}
			if (messageHandler != null) {
				messageHandler.Dispose();
				messageHandler = null;
			}
		}

		public class Factory : IInterpreterFactory {
			public IInterpreter CreateInstance( InterpreterThread thread, Handle<AppObject> entrypointObject, Handle<MessageHandlerBase> entrypoint, params Handle<AppObject>[] args ) {
				return new Interpreter( thread, entrypointObject, entrypoint, args );
			}

			public IInterpreter CreateInstance( InterpreterThread thread ) {
				return new Interpreter( thread );
			}

			public IInterpreter CreateInstance( InterpreterThread thread, InterpreterPosition position ) {
				return new Interpreter( thread, position );
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

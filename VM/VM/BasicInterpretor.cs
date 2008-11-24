using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class BasicInterpretor : IInterpretor {
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

		BasicInterpretor( Handle<AppObject> entrypointObject, Handle<VMILMessageHandler> entrypoint, params Handle<AppObject>[] args ) {
			stack.Push( entrypointObject.Class().Value, entrypointObject.Value );
			if (args.Length != entrypoint.ArgumentCount())
				throw new VMAppException( "Entrypoint takes " + entrypoint.ArgumentCount() + " arguments, but " + args.Length + " was supplied." );

			foreach (var arg in args)
				stack.Push( arg.Class(), arg.Value );
			stack.PushFrame( new ExecutionStack.ReturnAddress(), entrypoint );
			handler = entrypoint.Value;
		}

		BasicInterpretor() {
		}

		Handle<AppObject> Invoke() {
			var doActualReturnAt = stack.StackPointer;

		entry:
			var receiver = ((AppObject) stack.GetArgument( 0 ).Value).ToHandle();

			for (; pc < handler.InstructionCount(); pc++) {
				if (state == InterpretorState.Stopped)
					return null;
				if (state == InterpretorState.Paused) {
					waitToPause.Set();
					while (state == InterpretorState.Paused)
						waitToResume.WaitOne();
					waitToPause.Set();
				}

				var ins = handler.GetInstruction( pc );
				var opcode = (VMILLib.OpCode) (ins >> 27);
				int operand = (int) (0x07FFFFFF & ins);

				switch (opcode) {
					case VMILLib.OpCode.StoreField: {
							var v = stack.Pop();
							receiver.SetField( operand, v.Type, v.Value );
							break;
						}
					case VMILLib.OpCode.LoadField:
						stack.Push( receiver.GetFieldType( operand ), receiver.GetFieldValue( operand ) );
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
						stack.Push( VirtualMachine.IntegerClass, (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1) );
						break;
					case VMILLib.OpCode.PushLiteralString:
						stack.Push( VirtualMachine.StringClass, VirtualMachine.ConstantPool.GetString( operand ).Value );
						break;
					case VMILLib.OpCode.PushLiteralIntExtend:
						int i = stack.Pop().Value;
						i += (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
						stack.Push( VirtualMachine.IntegerClass, i );
						break;
					case VMILLib.OpCode.Pop:
						stack.Pop();
						break;
					case VMILLib.OpCode.Dup:
						stack.Push( stack.Peek() );
						break;
					case VMILLib.OpCode.NewInstance: {
							var cls = VirtualMachine.ResolveClass( receiver.Class(), ((h.String) stack.Pop().Value).ToHandle() );
							var obj = VMObjects.AppObject.CreateInstance( cls );
							stack.Push( cls, obj );
							break;
						}
					case VMILLib.OpCode.SendMessage: {
							var messageVal = stack.Pop();
							if (messageVal.Type != VirtualMachine.StringClass.Value)
								throw new InvalidOperationException( "Value on top of stack is not a message." );
							var message = (h.String) messageVal.Value;
							var argCount = ParseArgumentCount( message );
							var newReceiver = stack[argCount];
							var newHandlerBase = newReceiver.Type.ToHandle().ResolveMessageHandler( receiver, message );
							if (newHandlerBase == null)
								throw new MessageNotUnderstoodException( message.ToHandle(), ((AppObject) newReceiver.Value).ToHandle() );

							if (newHandlerBase.IsInternal()) {
								var newHandler = newHandlerBase.To<VMObjects.DelegateMessageHandler>();
								var method = SystemCalls.FindMethod( newHandler.ExternalName().ToHandle() );
								if (method == null)
									throw new MessageNotUnderstoodException( message.ToHandle(), ((AppObject) newReceiver.Value).ToHandle() );

								var args = new Handle<AppObject>[argCount];
								argCount.ForEachDescending( k => args[k] = ((AppObject) stack.Pop().Value).ToHandle() );
								stack.Pop();
								var ret = method( this, ((AppObject) newReceiver.Value).ToHandle(), args );
								if (ret != null)
									stack.Push( ret.Class().Value, ret.Value );
								break;
							} else {
								var newHandler = newHandlerBase.To<VMILMessageHandler>();

								stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc + 1 ), newHandler );
								pc = 0;
								handler = newHandler;
								goto entry;
							}
						}
					case VMILLib.OpCode.ReturnVoid:
					case VMILLib.OpCode.Return: {
							var ret = stack.PopFrame( opcode == VMILLib.OpCode.Return );
							handler = ret.Handler;
							pc = ret.InstructionOffset;
							if (stack.StackPointer < doActualReturnAt) {
								if (opcode == VMILLib.OpCode.ReturnVoid)
									return null;
								return ((AppObject) stack.Pop().Value).ToHandle();
							}
							goto entry;
						}
					case VMILLib.OpCode.Jump:
						pc += operand;
						break;
					case VMILLib.OpCode.JumpIfTrue: {
							var v = stack.Pop();
							if ((v.Type == VirtualMachine.IntegerClass.Value || v.Type == VirtualMachine.StringClass.Value) && v.Value != 0)
								pc += operand;
							else
								throw new NotImplementedException( "JumpIfTrue for AppObject" );
							break;
						}
					case VMILLib.OpCode.JumpIfFalse: {
							var v = stack.Pop();
							if ((v.Type == VirtualMachine.IntegerClass.Value || v.Type == VirtualMachine.StringClass.Value) && v.Value == 0)
								pc += operand;
							else
								throw new NotImplementedException( "JumpIfTrue for AppObject" );
							break;
						}
					case VMILLib.OpCode.Throw:
					case VMILLib.OpCode.Try:
					case VMILLib.OpCode.Catch:
					case VMILLib.OpCode.EndTryCatch:
						throw new NotImplementedException( opcode.ToString() );
					default:
						throw new InvalidVMProgramException( "Unknown VMILLib.OpCode encountered: " + opcode );
				}
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

		public Handle<AppObject> Send( Handle<VM.VMObjects.String> message, Handle<AppObject> to, params Handle<AppObject>[] arguments ) {
			var newHandlerBase = to.Class().ResolveMessageHandler( to, message );
			if (newHandlerBase.IsInternal()) {
				var newHandler = newHandlerBase.To<DelegateMessageHandler>();
				var method = SystemCalls.FindMethod( newHandler.ExternalName() );
				if (method == null)
					throw new MessageNotUnderstoodException( message, to );

				return method( this, to, arguments );
			} else {
				var newHandler = newHandlerBase.To<VMILMessageHandler>();
				if (newHandler == null)
					throw new MessageNotUnderstoodException( message, to );

				stack.Push( to.Class(), to );
				arguments.ForEach( a => stack.Push( a.Class(), a ) );

				stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc ), newHandler );
				pc = 0;
				handler = newHandler;
				return Invoke();
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

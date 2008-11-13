using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;
using h = VM.VMObjects;
using System.Threading;

namespace VM {
	class BasicInterpretor : IInterpretor {
		bool isBlocked;
		InterpretorState state;
		public InterpretorState State {
			get { return state; }
		}

		object externalWaitObject = new object();
		Thread thread;
		ExecutionStack stack = new ExecutionStack( 100 );
		VMILMessageHandler handler;
		int pc = 0;
		EventWaitHandle waitToPause = new EventWaitHandle( false, EventResetMode.AutoReset ),
			waitToResume = new EventWaitHandle( false, EventResetMode.AutoReset );

		BasicInterpretor( AppObject entryObject, MessageHandlerBase entryPoint, params AppObject[] args ) {
			stack.Push( entryObject.Class(), entryObject );
			foreach (var arg in args)
				stack.Push( arg.Class(), arg );
			handler = (VMILMessageHandler) entryPoint;
		}

		void Invoke() {
		entry:
			for (; pc < handler.InstructionCount(); pc++) {
				if (state == InterpretorState.Stopped)
					return;
				if (state == InterpretorState.Paused) {
					waitToPause.Set();
					while (state == InterpretorState.Paused)
						waitToResume.WaitOne();
					waitToPause.Set();
				}

				var ins = handler.Instruction( pc );
				var opcode = (VMILLib.OpCode) (ins >> 27);
				int operand = (int) (0x07FFFFFF & ins);
				var receiver = (AppObject) stack.GetArgument( 0 ).Value;

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
						stack.Push( stack.GetLocal( operand + 1 ) );
						break;
					case VMILLib.OpCode.LoadArgument:
						stack.Push( stack.GetArgument( operand + 1 ) );
						break;
					case VMILLib.OpCode.PushLiteralInt:
						stack.Push( VirtualMachine.IntegerClass, VirtualMachine.ConstantPool.GetInteger( operand ) );
						break;
					case VMILLib.OpCode.PushLiteralString:
						stack.Push( VirtualMachine.StringClass, VirtualMachine.ConstantPool.GetString( operand ) );
						break;
					case VMILLib.OpCode.PushLiteralIntInline:
						stack.Push( VirtualMachine.IntegerClass,
							VirtualMachine.ConstantPool.GetInteger( (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1) ) );
						break;
					case VMILLib.OpCode.Pop:
						stack.Pop();
						break;
					case VMILLib.OpCode.Dup:
						stack.Push( stack.Peek() );
						break;
					case VMILLib.OpCode.NewInstance: {
							var cls = VirtualMachine.ResolveClass( (h.String) stack.Pop().Value );
							AppObject obj = VirtualMachine.MemoryManager.Allocate( cls.InstanceSize() );
							stack.Push( cls, obj );
							break;
						}
					case VMILLib.OpCode.SendMessage: {
							var messageVal = stack.Pop();
							if (messageVal.Type != VirtualMachine.StringClass)
								throw new InvalidOperationException( "Value on top of stack is not a message." );
							var message = (h.String) messageVal.Value;
							var newReceiver = stack.Peek();
							var newHandlerBase = newReceiver.Type.GetMessageHandler( receiver, message );
							if (newHandlerBase.IsInternal())
								throw new NotImplementedException( "Calling DelegateMessageHandler" );
							else {
								var newHandler = (VMILMessageHandler) newHandlerBase;
								if (newHandler == 0)
									throw new MessageNotUnderstoodException( message.AsHandle(), ((AppObject) newReceiver.Value).AsHandle() );

								stack.PushFrame( new ExecutionStack.ReturnAddress( handler, pc ), newHandler );
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
							goto entry;
						}
					case VMILLib.OpCode.Jump:
						pc += operand;
						break;
					case VMILLib.OpCode.JumpIfTrue: {
							var v = stack.Pop();
							if ((v.Type == VirtualMachine.IntegerClass || v.Type == VirtualMachine.StringClass) && v.Value != 0)
								pc += operand;
							else
								throw new NotImplementedException( "JumpIfTrue for AppObject" );
							break;
						}
					case VMILLib.OpCode.JumpIfFalse: {
							var v = stack.Pop();
							if ((v.Type == VirtualMachine.IntegerClass || v.Type == VirtualMachine.StringClass) && v.Value == 0)
								pc += operand;
							else
								throw new NotImplementedException( "JumpIfTrue for AppObject" );
							break;
						}
					case VMILLib.OpCode.Throw:
					case VMILLib.OpCode.Try:
					case VMILLib.OpCode.Catch:
					case VMILLib.OpCode.EndTryCatch:
						throw new NotImplementedException( "EndTryCatch" );
					default:
						throw new InvalidVMProgramException( "Unknown VMILLib.OpCode encountered: " + opcode );
				}
			}

			throw new InvalidVMProgramException( "Reach end of message handler without returning." );
		}

		public void Start() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped." );
			lock (externalWaitObject) {
				if (state != InterpretorState.NotStarted)
					throw new InterpretorException( "Interpretor already started." );

				thread = new Thread( Invoke );
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
			public IInterpretor CreateInstance( AppObject entryObject, MessageHandlerBase entryPoint, params AppObject[] args ) {
				return new BasicInterpretor( entryObject, entryPoint, args );
			}
		}
	}
}

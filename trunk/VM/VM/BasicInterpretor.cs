using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.Handles;

namespace VM {
	class BasicInterpretor : IInterpretor {
		List<SV> stack = new List<SV>();
		uint basePointer = 0;

		void Invoke( AppObject receiver, VMILMessageHandler handler ) {
			var inss = handler.Instructions();
			basePointer = (uint) stack.Count - handler.ArgumentCount();

			for (int i = 0; i < handler.LocalCount() - handler.ArgumentCount(); i++)
				Push( new SV() );

			foreach (var ins in inss) {
				var opcode = (VMILLib.OpCode) (ins >> 27);
				var operand = 0x07FFFFFF & ins;

				switch (opcode) {
					case VMILLib.OpCode.StoreField:
						var v = Pop();
						receiver.SetField( operand, v.Class, v.Value );
						break;
					case VMILLib.OpCode.LoadField:
						Push( receiver.GetFieldType( operand ), receiver.GetFieldValue( operand ) );
						break;
					case VMILLib.OpCode.StoreLocal:
						SetLocal( operand, Pop() );
						break;
					case VMILLib.OpCode.LoadLocal:
						Push( GetLocal( operand ) );
						break;
					case VMILLib.OpCode.PushLiteral:
						Push( VirtualMachine.ConstantPool.GetConstantType( operand ), VirtualMachine.ConstantPool.GetValue( operand ) );
						break;
					case VMILLib.OpCode.Pop:
						Pop();
						break;
					case VMILLib.OpCode.Dup:
						Push( Peek() );
						break;
					case VMILLib.OpCode.NewInstance:
						var cls = VirtualMachine.ResolveClass( Pop().Value );
						AppObject obj = VirtualMachine.MemoryManager.Allocate( cls.InstanceSize() );
						Push( cls, obj );
						break;
					case VMILLib.OpCode.SendMessage:
						break;
					case VMILLib.OpCode.ReturnVoid:
						break;
					case VMILLib.OpCode.Return:
						break;
					case VMILLib.OpCode.Jump:
						break;
					case VMILLib.OpCode.JumpIfTrue:
						break;
					case VMILLib.OpCode.JumpIfFalse:
						break;
					case VMILLib.OpCode.Throw:
						break;
					case VMILLib.OpCode.Try:
						break;
					case VMILLib.OpCode.Catch:
						break;
					case VMILLib.OpCode.EndTryCatch:
						break;
					default:
						throw new ArgumentException( "Unknown VMILLib.OpCode encountered: " + opcode );
				}
			}
		}

		void Push( Class cls, uint value ) {
			Push( new SV { Class = cls, Value = value } );
		}

		void Push( SV v ) {
			stack.Add( v );
		}

		SV Pop() {
			var v = stack[stack.Count - 1];
			stack.RemoveAt( stack.Count - 1 );
			return v;
		}

		SV Peek() {
			return stack[stack.Count - 1];
		}

		SV GetLocal( uint i ) {
			return stack[(int) (basePointer + i)];
		}

		void SetLocal( uint i, SV v ) {
			stack[(int) (basePointer + i)] = v;
		}

		struct SV {
			public Class Class;
			public uint Value;
		}
	}
}

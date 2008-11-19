using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	class ExecutionStack {
		const int TYPE_BASE_POINTER = -1;
		const int TYPE_RETURN_HANDLER = -2;
		const int TYPE_RETURN_INSTRUCTION_OFFSET = -3;
		const int TYPE_FRAME_BOUNDARY = -4;

		const int ARGUMENT_OFFSET = 5;
		const int RETURN_ADDRESS_OFFSET = 4;
		const int RETURN_ADDRESS_INSTRUCTION_OFFSET = 3;
		const int OLD_BASE_POINTER_OFFSET = 2;
		const int OLD_FRAME_BOUNDARY_OFFSET = 1;

		int initialSize;
		StackValue[] stack;
		int stackPointer = 0, basePointer, frameBoundary;
		public int BasePointer { get { return basePointer; } }
		public int StackPointer { get { return stackPointer; } }
		public int FrameBoundary { get { return frameBoundary; } }

		public StackValue this[int index] {
			get {
				if (stackPointer - index - 1 < 0)
					throw new ArgumentOutOfRangeException();
				return stack[stackPointer - index - 1];
			}
			set {
				if (stackPointer - index - 1 < 0)
					throw new ArgumentOutOfRangeException();
				stack[stackPointer - index - 1] = value;
			}
		}

		public ExecutionStack( int initialSize ) {
			this.initialSize = initialSize;
			if (initialSize < 0)
				throw new ArgumentOutOfRangeException( "Argument must be greater than zero.", "initialSize" );

			stack = new StackValue[initialSize];
		}

		public void Push( Class type, Word value ) {
			Push( new StackValue( type, value ) );
		}

		public void Push( StackValue v ) {
			if (stackPointer == stack.Length)
				Expand();

			stack[stackPointer++] = v;
		}

		public StackValue Pop() {
			if (stackPointer == 0)
				throw new InvalidOperationException( "Stack is empty." );
			if (stackPointer < stack.Length / 2 && stackPointer > initialSize)
				Shrink();

			return stack[--stackPointer];
		}

		public StackValue Peek() {
			if (stackPointer == 0)
				throw new InvalidOperationException( "Stack is empty." );
			if (stackPointer < stack.Length / 2 && stackPointer > initialSize)
				Shrink();

			return stack[stackPointer];
		}

		public void PushFrame( ReturnAddress returnAddress, VMILMessageHandler callee ) {
			Push( (Class) TYPE_RETURN_HANDLER, returnAddress.Handler );
			Push( (Class) TYPE_RETURN_INSTRUCTION_OFFSET, returnAddress.InstructionOffset );
			Push( (Class) TYPE_BASE_POINTER, basePointer );
			Push( (Class) TYPE_FRAME_BOUNDARY, frameBoundary );
			frameBoundary = stackPointer - ARGUMENT_OFFSET - callee.ArgumentCount;
			basePointer = stackPointer;

			for (var i = 0; i < callee.LocalCount; i++)
				Push( new StackValue() );
		}

		public ReturnAddress PopFrame( bool withReturnValue ) {
			StackValue ret = withReturnValue ? Pop() : new StackValue();

			var retAdr = new ReturnAddress( (VMILMessageHandler) stack[basePointer - RETURN_ADDRESS_OFFSET].Value, stack[basePointer - RETURN_ADDRESS_INSTRUCTION_OFFSET].Value );
			stackPointer = frameBoundary;
			frameBoundary = stack[basePointer - OLD_FRAME_BOUNDARY_OFFSET].Value;
			basePointer = stack[basePointer - OLD_BASE_POINTER_OFFSET].Value;

			if (withReturnValue)
				Push( ret );

			return retAdr;
		}

		public StackValue GetLocal( int index ) {
			if (index + basePointer >= stackPointer)
				throw new ArgumentOutOfRangeException( "Index plus base pointer must be less than the stack pointer.", "index" );

			return stack[basePointer + index];
		}

		public void SetLocal( int index, StackValue value ) {
			index = basePointer + index;
			if (index >= stackPointer)
				throw new ArgumentOutOfRangeException( "Index plus base pointer must be less than the stack pointer.", "index" );

			stack[index] = value;
		}

		public void SetLocal( int index, Class type, Word value ) {
			SetLocal( index, new StackValue( type, value ) );
		}

		public StackValue GetArgument( int index ) {
			index = frameBoundary + index;
			if (index > basePointer - ARGUMENT_OFFSET)
				throw new ArgumentOutOfRangeException( "No such argument.", "index" );

			return stack[index];
		}

		void Shrink() {
			var temp = new StackValue[stack.Length / 2];
			System.Array.Copy( stack, temp, stackPointer );
			stack = temp;
		}

		void Expand() {
			var temp = new StackValue[stack.Length * 2];
			System.Array.Copy( stack, temp, stackPointer );
			stack = temp;
		}

		#region StackValue
		public struct StackValue {
			public readonly Class Type;
			public readonly Word Value;

			public StackValue( Class type, Word value ) {
				this.Type = type;
				this.Value = value;
			}
		}
		#endregion

		#region ReturnAddress
		public struct ReturnAddress {
			public readonly VMILMessageHandler Handler;
			public readonly int InstructionOffset;

			public ReturnAddress( VMILMessageHandler handler, int instructionOffset ) {
				this.Handler = handler;
				this.InstructionOffset = instructionOffset;
			}
		}
		#endregion
	}
}

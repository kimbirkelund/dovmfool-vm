using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	class ExecutionStack : IExecutionStack {
		internal const int TYPE_BASE_POINTER = -1;
		internal const int TYPE_RETURN_HANDLER = -2;
		internal const int TYPE_RETURN_INSTRUCTION_OFFSET = -3;
		internal const int TYPE_FRAME_BOUNDARY = -4;
		internal const int TYPE_TRY = -5;
		internal const int TYPE_ACTUAL_RETURN_HERE = -6;

		internal const int ARGUMENT_OFFSET = 6;
		internal const int RETURN_ADDRESS_OFFSET = 5;
		internal const int RETURN_ADDRESS_INSTRUCTION_OFFSET = 4;
		internal const int ACTUAL_RETURN_HERE_OFFSET = 3;
		internal const int OLD_BASE_POINTER_OFFSET = 2;
		internal const int OLD_FRAME_BOUNDARY_OFFSET = 1;

		protected int initialSize;
		protected UValue[] stack;
		protected int stackPointer = 0, basePointer, frameBoundary;
		public int BasePointer { get { return basePointer; } }
		public int StackPointer { get { return stackPointer; } }
		public int FrameBoundary { get { return frameBoundary; } }
		public int Size { get { return stack.Length; } }

		public UValue this[int index] {
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
				throw new ArgumentOutOfRangeException( "Argument must be greater than zero.".ToVMString().ToHandle(), "initialSize".ToVMString().ToHandle() );

			stack = new UValue[initialSize];
		}

		public ExecutionStack( IExecutionStack stack )
			: this( stack.Size ) {
			this.basePointer = stack.BasePointer;
			this.frameBoundary = stack.FrameBoundary;
			this.stackPointer = stack.StackPointer;
			for (int i = 0; i < stack.StackPointer; i++)
				this[i] = stack[i];
		}

		public void Push( Class type, Word value ) {
			Push( UValue.Ref( type, value ) );
		}

		public virtual void Push( UValue v ) {
			if (stackPointer == stack.Length)
				Expand();

			stack[stackPointer++] = v;
		}

		public virtual UValue Pop() {
			if (stackPointer == 0)
				throw new InvalidOperationException( "Stack is empty." );
			if (stackPointer < stack.Length / 2 && stackPointer > initialSize)
				Shrink();

			return stack[--stackPointer];
		}

		public UValue Peek() {
			if (stackPointer == 0)
				throw new InvalidOperationException( "Stack is empty." );
			if (stackPointer < stack.Length / 2 && stackPointer > initialSize)
				Shrink();

			return stack[stackPointer - 1];
		}

		public virtual void PushFrame( ReturnAddress returnAddress, Handle<MessageHandlerBase> callee ) {
			Push( (Class) TYPE_RETURN_HANDLER, returnAddress.Handler );
			Push( (Class) TYPE_RETURN_INSTRUCTION_OFFSET, returnAddress.InstructionOffset );
			Push( (Class) TYPE_ACTUAL_RETURN_HERE, returnAddress.DoActualReturnHere );
			Push( (Class) TYPE_BASE_POINTER, basePointer );
			Push( (Class) TYPE_FRAME_BOUNDARY, frameBoundary );
			frameBoundary = stackPointer - ARGUMENT_OFFSET - callee.ArgumentCount();
			basePointer = stackPointer;

			if (!callee.IsExternal())
				using (var hVmilCallee = callee.To<VMILMessageHandler>())
					hVmilCallee.LocalCount().ForEach( () => Push( new UValue() ) );
		}

		public virtual ReturnAddress PopFrame( bool withReturnValue ) {
			UValue ret = withReturnValue ? Pop() : new UValue();

			var retAdr = new ReturnAddress( (VMILMessageHandler) stack[basePointer - RETURN_ADDRESS_OFFSET].Value, stack[basePointer - RETURN_ADDRESS_INSTRUCTION_OFFSET].Value, stack[basePointer - ACTUAL_RETURN_HERE_OFFSET].Value );
			stackPointer = frameBoundary;
			frameBoundary = stack[basePointer - OLD_FRAME_BOUNDARY_OFFSET].Value;
			basePointer = stack[basePointer - OLD_BASE_POINTER_OFFSET].Value;

			if (withReturnValue)
				Push( ret );

			return retAdr;
		}

		public virtual UValue GetLocal( int index ) {
			if (index + basePointer >= stackPointer)
				throw new ArgumentOutOfRangeException( "Index plus base pointer must be less than the stack pointer.".ToVMString().ToHandle(), "index".ToVMString().ToHandle() );

			return stack[basePointer + index];
		}

		public virtual void SetLocal( int index, UValue value ) {
			index = basePointer + index;
			if (index >= stackPointer)
				throw new ArgumentOutOfRangeException( "Index plus base pointer must be less than the stack pointer.".ToVMString().ToHandle(), "index".ToVMString().ToHandle() );

			stack[index] = value;
		}

		public void SetLocal( int index, Class type, Word value ) {
			SetLocal( index, UValue.Ref( type, value ) );
		}

		public UValue GetArgument( int index ) {
			index = frameBoundary + index;
			if (index > basePointer - ARGUMENT_OFFSET)
				throw new ArgumentOutOfRangeException( "No such argument.".ToVMString().ToHandle(), "index".ToVMString().ToHandle() );

			return stack[index];
		}

		public void PushTry( int index ) {
			Push( (Class) TYPE_TRY, index );
		}

		public virtual int? PopTry() {
			var i = 0;
			while (stackPointer - i > basePointer && this[i].Type != TYPE_TRY)
				i++;
			if (stackPointer - i <= basePointer)
				return null;
			stackPointer -= i + 1;
			return this[-1].Value;
		}

		void Shrink() {
			var temp = new UValue[stack.Length / 2];
			System.Array.Copy( stack, temp, stackPointer );
			stack = temp;
		}

		void Expand() {
			var temp = new UValue[stack.Length * 2];
			System.Array.Copy( stack, temp, stackPointer );
			stack = temp;
		}

		#region ReturnAddress
		public struct ReturnAddress {
			public readonly MessageHandlerBase Handler;
			public readonly int InstructionOffset;
			public readonly bool DoActualReturnHere;

			public ReturnAddress( MessageHandlerBase handler, int instructionOffset, bool doActualReturnHere ) {
				this.Handler = handler;
				this.InstructionOffset = instructionOffset;
				this.DoActualReturnHere = doActualReturnHere;
			}
		}
		#endregion
	}
}

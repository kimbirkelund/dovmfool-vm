using System;

namespace VM {
	interface IExecutionStack {
		int Size { get; }
		int BasePointer { get; }
		int FrameBoundary { get; }
		UValue GetArgument( int index );
		UValue GetLocal( int index );
		UValue Peek();
		UValue Pop();
		ExecutionStack.ReturnAddress PopFrame( bool withReturnValue );
		int? PopTry();
		void Push( UValue v );
		void Push( VM.VMObjects.Class type, Word value );
		void PushFrame( ExecutionStack.ReturnAddress returnAddress, Handle<VM.VMObjects.MessageHandlerBase> callee );
		void PushTry( int index );
		void SetLocal( int index, VM.VMObjects.Class type, Word value );
		void SetLocal( int index, UValue value );
		int StackPointer { get; }
		UValue this[int index] { get; set; }
	}
}

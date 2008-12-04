using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM.Debugging {
	class DebugExecutionStack : ExecutionStack {
		public event EventHandler<StackPushEventArgs> ValuePushed;
		void OnPushValue( UValue value ) {
			if (ValuePushed != null)
				ValuePushed( this, new StackPushEventArgs( value ) );
		}
		public event EventHandler<StackPopEventArgs> ValuePopped;
		void OnPopValue( int popCount ) {
			if (ValuePopped != null)
				ValuePopped( this, new StackPopEventArgs( popCount ) );
		}
		public event EventHandler<StackChangeEventArgs> StackChanged;
		void OnStackChanged( int position, UValue newValue ) {
			if (StackChanged != null)
				StackChanged( this, new StackChangeEventArgs( position, newValue ) );
		}

		public DebugExecutionStack( int initialSize )
			: base( initialSize ) { }

		public DebugExecutionStack( ExecutionStack stack ) : base( stack ) { }

		public override UValue Pop() {
			var v = base.Pop();
			OnPopValue( 1 );
			return v;
		}

		public override void Push( UValue v ) {
			base.Push( v );
			OnPushValue( v );
		}

		public override void SetLocal( int index, UValue value ) {
			base.SetLocal( index, value );
			OnStackChanged( BasePointer + index, value );
		}

		public override ReturnAddress PopFrame( bool withReturnValue ) {
			var sp = StackPointer;

			UValue ret = withReturnValue ? Pop() : new UValue();

			var retAdr = new ReturnAddress( (VMILMessageHandler) stack[basePointer - RETURN_ADDRESS_OFFSET].Value, stack[basePointer - RETURN_ADDRESS_INSTRUCTION_OFFSET].Value, stack[basePointer - ACTUAL_RETURN_HERE_OFFSET].Value );
			stackPointer = frameBoundary;
			frameBoundary = stack[basePointer - OLD_FRAME_BOUNDARY_OFFSET].Value;
			basePointer = stack[basePointer - OLD_BASE_POINTER_OFFSET].Value;

			OnPopValue( sp - StackPointer );
			if (withReturnValue)
				Push( ret );

			return retAdr;
		}

		public override int? PopTry() {
			var sp = StackPointer;
			var ret = base.PopTry();
			OnPopValue( sp - StackPointer );
			return ret;
		}
	}
}

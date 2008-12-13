using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VM.Debugging.Service.Server {
	static class DebuggerService {
		static Dictionary<InterpreterThread, int> interpToId = new Dictionary<InterpreterThread, int>();
		static Dictionary<int, InterpreterThread> idToInterp = new Dictionary<int, InterpreterThread>();

		public static event EventHandler<NewInterpreterEventArgs> NewInterpreter;
		static void OnNewInterpreter( int id ) {
			if (NewInterpreter != null)
				NewInterpreter( null, new NewInterpreterEventArgs( id ) );
		}
		public static event EventHandler<BreakpointHitEventArgs> BreakpointHit;
		static void OnBreakpointHit( int messageHandler, int position ) {
			if (BreakpointHit != null)
				BreakpointHit( null, new BreakpointHitEventArgs( messageHandler, position ) );
		}
		public static event EventHandler<StackPushEventArgs> StackPushed;
		static void OnValuePushed( StackPushEventArgs args ) {
			if (StackPushed != null)
				StackPushed( null, args );
		}

		public static event EventHandler<StackPopEventArgs> StackPopped;
		static void OnValuePopped( StackPopEventArgs args ) {
			if (StackPopped != null)
				StackPopped( null, args );
		}

		public static event EventHandler<StackChangeEventArgs> StackChanged;
		static void OnStackChanged( StackChangeEventArgs args ) {
			if (StackChanged != null)
				StackChanged( null, args );
		}

		static DebuggerService() {
			VM.VirtualMachine.NewThread += new EventHandler<VM.NewThreadEventArgs>( VirtualMachine_NewInterpreter );
		}

		static void VirtualMachine_NewInterpreter( object sender, VM.NewThreadEventArgs e ) {
			OnNewInterpreter( Get( (InterpreterThread) e.Thread ) );
		}

		static int Add( InterpreterThread interp ) {
			interpToId.Add( interp, interp.Id );
			idToInterp.Add( interp.Id, interp );

			((IDebugInterpreter) interp.Interpreter).StackChanged += new EventHandler<VM.Debugging.StackChangeEventArgs>( interp_StackChanged );
			return interp.Id;
		}

		static void interp_StackChanged( object sender, VM.Debugging.StackChangeEventArgs e ) {
			OnStackChanged( new StackChangeEventArgs( e.Position, e.NewValue.ToValue() ) );
		}

		public static InterpreterThread Get( int id ) {
			if (!idToInterp.ContainsKey( id ))
				throw new System.ArgumentOutOfRangeException( "id" );

			return idToInterp[id];
		}

		public static int Get( InterpreterThread interp ) {
			int id;
			if (!interpToId.TryGetValue( interp, out id ))
				id = Add( interp );
			return id;
		}

		public static void Attach() {
			VirtualMachine.DebuggerAttached();
		}

		internal static void Detach() {
			VirtualMachine.DebuggerDetached();
		}

		public static int[] GetInterpreters() {
			return VirtualMachine.GetInterpreters().Select( i => Get( i ) ).ToArray();
		}

		public static InterpreterPosition Break( int id ) {
			var ip = ((IDebugInterpreter) Get( id ).Interpreter).Break();
			using (var hBase = ip.MessageHandler.To<VMObjects.MessageHandlerBase>())
				return new InterpreterPosition( MessageHandlerReflectionService.Get( hBase ), ip.Position );
		}

		public static InterpreterPosition StepOne( int id ) {
			var ip = ((IDebugInterpreter) Get( id ).Interpreter).StepOne();
			using (var hBase = ip.MessageHandler.To<VMObjects.MessageHandlerBase>())
				return new InterpreterPosition( MessageHandlerReflectionService.Get( hBase ), ip.Position );
		}

		public static void Continue( int id ) {
			((IDebugInterpreter) Get( id ).Interpreter).Continue();
		}
	}

	class NewInterpreterEventArgs : EventArgs {
		public readonly int Id;

		public NewInterpreterEventArgs( int id ) {
			this.Id = id;
		}
	}

	class BreakpointHitEventArgs : EventArgs {
		public readonly int MessageHandlerId;
		public readonly int Position;

		public BreakpointHitEventArgs( int messageHandlerId, int position ) {
			this.MessageHandlerId = messageHandlerId;
			this.Position = position;
		}
	}

	class StackPushEventArgs : EventArgs {
		public readonly Value Object;

		public StackPushEventArgs( Value obj ) {
			this.Object = obj;
		}
	}

	class StackPopEventArgs : EventArgs {
		public readonly int PopCount;

		public StackPopEventArgs( int popCount ) {
			this.PopCount = popCount;
		}
	}

	class StackChangeEventArgs : EventArgs {
		public readonly int Position;
		public readonly Value NewValue;

		public StackChangeEventArgs( int position, Value newValue ) {
			this.Position = position;
			this.NewValue = newValue;
		}
	}
}

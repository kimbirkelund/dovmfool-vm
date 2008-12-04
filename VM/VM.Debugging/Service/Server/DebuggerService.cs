using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace VM.Debugging.Service.Server {
	static class DebuggerService {
		static Dictionary<InterpretorThread, int> interpToId = new Dictionary<InterpretorThread, int>();
		static Dictionary<int, InterpretorThread> idToInterp = new Dictionary<int, InterpretorThread>();

		public static event EventHandler<NewInterpretorEventArgs> NewInterpretor;
		static void OnNewInterpretor( int id ) {
			if (NewInterpretor != null)
				NewInterpretor( null, new NewInterpretorEventArgs( id ) );
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
			VM.VirtualMachine.NewThread += new EventHandler<VM.NewThreadEventArgs>( VirtualMachine_NewInterpretor );
		}

		static void VirtualMachine_NewInterpretor( object sender, VM.NewThreadEventArgs e ) {
			OnNewInterpretor( Get( (InterpretorThread) e.Thread ) );
		}

		static int Add( InterpretorThread interp ) {
			interpToId.Add( interp, interp.Id );
			idToInterp.Add( interp.Id, interp );

			((IDebugInterpretor) interp).StackChanged += new EventHandler<VM.Debugging.StackChangeEventArgs>( interp_StackChanged );
			return interp.Id;
		}

		static void interp_StackChanged( object sender, VM.Debugging.StackChangeEventArgs e ) {
			OnStackChanged( new StackChangeEventArgs( e.Position, e.NewValue.ToValue() ) );
		}

		public static InterpretorThread Get( int id ) {
			if (!idToInterp.ContainsKey( id ))
				throw new System.ArgumentOutOfRangeException( "id" );

			return idToInterp[id];
		}

		public static int Get( InterpretorThread interp ) {
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

		public static int[] GetInterpretors() {
			return VirtualMachine.GetInterpretors().Select( i => Get( i ) ).ToArray();
		}

		public static InterpretorPosition Break( int id ) {
			var ip = ((IDebugInterpretor) Get( id ).Interpretor).Break();
			return new InterpretorPosition( MessageHandlerReflectionService.Get( ip.MessageHandler.To<VMObjects.MessageHandlerBase>() ), ip.Position );
		}

		public static InterpretorPosition StepOne( int id ) {
			var ip = ((IDebugInterpretor) Get( id ).Interpretor).StepOne();
			return new InterpretorPosition( MessageHandlerReflectionService.Get( ip.MessageHandler.To<VMObjects.MessageHandlerBase>() ), ip.Position );
		}

		public static void Continue( int id ) {
			((IDebugInterpretor) Get( id ).Interpretor).Continue();
		}
	}

	class NewInterpretorEventArgs : EventArgs {
		public readonly int Id;

		public NewInterpretorEventArgs( int id ) {
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

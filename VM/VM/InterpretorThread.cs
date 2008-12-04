using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VM.VMObjects;

namespace VM {
	class InterpretorThread {
		public static IInterpretorFactory InterpretorFactory { get; set; }

		public int Id { get; private set; }
		public IInterpretor Interpretor { get; private set; }
		public IExecutionStack Stack { get; private set; }

		bool isBlocked = false;
		InterpretorState state;
		public InterpretorState State {
			get { return state; }
		}

		Thread thread;
		EventWaitHandle waitToPause = new EventWaitHandle( false, EventResetMode.AutoReset ),
			waitToResume = new EventWaitHandle( false, EventResetMode.AutoReset );

		public InterpretorThread( int id, Handle<VMObjects.MessageHandlerBase> messageHandler, Handle<VMObjects.AppObject> obj, params Handle<AppObject>[] arguments ) {
			this.Id = id;
			this.Stack = InterpretorFactory.CreateStack();
			this.Interpretor = InterpretorFactory.CreateInstance( this, obj, messageHandler, arguments );
		}

		public bool PollForReturn() {
			if (state == InterpretorState.Stopped)
				return true;
			if (state == InterpretorState.Paused) {
				waitToPause.Set();
				while (state == InterpretorState.Paused)
					waitToResume.WaitOne();
				waitToPause.Set();
			}
			return false;
		}

		public void Start() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (this) {
				if (state != InterpretorState.NotStarted)
					throw new InterpretorException( "Interpretor already started.".ToVMString() );

				thread = new Thread( () => Interpretor.Run() );
				thread.IsBackground = true;
				state = InterpretorState.Running;
				thread.Start();
			}
		}

		public void Pause() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (this) {
				if (state != InterpretorState.Paused && state != InterpretorState.Running && state != InterpretorState.Blocked)
					throw new InterpretorException( "Interpretor is in neither of the states Paused, Running or Blocked.".ToVMString() );

				var oldState = state;
				state = InterpretorState.Paused;
				if (oldState == InterpretorState.Running)
					waitToPause.WaitOne();
			}
		}

		public void Resume() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (this) {
				state = isBlocked ? InterpretorState.Blocked : InterpretorState.Running;
				if (!isBlocked)
					waitToResume.Set();
			}
		}

		public void Kill() {
			if (state == InterpretorState.Stopped)
				throw new InterpretorException( "Interpretor has been stopped.".ToVMString() );
			lock (this) {
				state = InterpretorState.Stopped;
				if (thread.Join( 5000 ))
					return;
				thread.Abort();
				if (!thread.Join( 5000 ))
					throw new InterpretorFailedToStopException();
				thread = null;
			}
		}

		public Handle<AppObject> Join() {
			if (thread != null && thread.ThreadState != ThreadState.Unstarted)
				thread.Join();

			return Interpretor.FinalResult;
		}

		public void Swap() {
			Pause();
			this.Stack = InterpretorFactory.CreateStack( this.Stack );
			this.Interpretor = InterpretorFactory.CreateInstance( this, this.Interpretor.CurrentPosition );
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Sekhmet.Logging {
    /// <summary>
    /// Represents the mediator for the posting of logging messages.
    /// </summary>
    public class Logger : IDisposable {
        #region Fields & props
        bool isDisposed;

        /// <summary>
        /// Gets the list of handlers attached to the logger.
        /// </summary>
        public IList<ILogHandler> Handlers { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether an encountered exception should be thrown or simply logged.
        /// </summary>
        /// <value><c>true</c> if an exception should be thrown when exception is raised; <c>false</c> causes the exception to simply be logged.</value>
        public bool ThrowOnException { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance has logged errors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has logged errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors { get; private set; }

        bool runInSeperateThread;
        /// <summary>
        /// Gets or sets a value indicating whether message processing should run in a seperate thread.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if message processing should run in a seperate thread; otherwise, <c>false</c>.
        /// </value>
        public bool RunInSeperateThread {
            get { return runInSeperateThread; }
            set {
                if (value && !runInSeperateThread)
                    SetupSpooler();
                else if (!value && runInSeperateThread)
                    Close();
                runInSeperateThread = value;
            }
        }

        EventWaitHandle spoolWaiter;
        EventWaitHandle closeWaiter;
        bool running;
        Thread spooler;
        Queue<LogEntry> messages = new Queue<LogEntry>();

        Dictionary<string, StringBuilder> lines = new Dictionary<string, StringBuilder>();

        Stack<ITask> tasks = new Stack<ITask>();
        /// <summary>
        /// Gets the current task.
        /// </summary>
        /// <value>The current task.</value>
        public ITask CurrentTask { get { return tasks.Count == 0 ? null : tasks.Peek(); } }
        string currentTaskName;
        #endregion

        #region Cons
        /// <summary>
        /// Initialises an instance of <see cref="Logger"/>.
        /// </summary>
        public Logger() {
            Open();
        }
        #endregion

        #region Methods
        /// <summary>
        /// Informs the logger that a new task has been started. 
        /// </summary>
        /// <param name="task">The task.</param>
        /// <remarks>
        /// Note that a new task can be started without stopping the current task, allowing for nested tasks.
        /// </remarks>
        public void BeginTask( ITask task ) {
            AssertDisposed();
            tasks.Push( task );
            currentTaskName = task.Name;
            PostEvent( new TaskEntry( EntryType.BeginTask, "logger", "Beginning phase", task ) );
        }

        /// <summary>
        /// Ends the currently running task.
        /// </summary>
        public void EndTask() {
            AssertDisposed();
            PostEvent( new TaskEntry( EntryType.EndTask, "logger", "Ending phase", CurrentTask ) );
            tasks.Pop();
            currentTaskName = null;
        }

        /// <summary>
        /// Posts an error message.
        /// </summary>
        /// <param name="error">The error.</param>
        public void PostError( Error error ) {
            PostEvent( new ErrorEntry( error ) );
        }

        /// <summary>
        /// Posts an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public void PostError( string message ) {
            PostError( new Error( message ) );
        }

        /// <summary>
        /// Posts an error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="args">The arguments to be used to format the error message.</param>
        public void PostError( string message, params object[] args ) {
            PostError( new Error( string.Format( message, args ) ) );
        }

        /// <summary>
        /// Posts an error message.
        /// </summary>
        /// <param name="exception">The exception representing the error.</param>
        public void PostError( Exception exception ) {
            PostError( new Error( exception ) );
        }

        /// <summary>
        /// Posts a warning.
        /// </summary>
        public void PostWarning( string category, string message, params object[] args ) {
            PostEvent( new LogEntry( EntryType.Warning, category, string.Format( message, args ) ) );
        }

        /// <summary>
        /// Posts a warning.
        /// </summary>
        public void PostWarning( string category, string message ) {
            PostEvent( new LogEntry( EntryType.Warning, category, message ) );
        }

        /// <summary>
        /// Posts the specified message.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The arguments the format <c>message</c> with.</param>
        public void Post( string category, string message, params object[] args ) {
            AssertDisposed();
            StringBuilder sb;
            if (lines.ContainsKey( category ))
                sb = lines[category];
            else {
                sb = new StringBuilder();
                lines.Add( category, sb );
            }

            sb.AppendFormat( message, args );
        }

        /// <summary>
        /// Posts the specified message, appending to an existing line or creating a new line. This method call doesn't propagate to the log handlers.
        /// </summary>
        /// <param name="category">The category of the message.</param>
        /// <param name="message">The message.</param>
        public void Post( string category, object message ) {
            AssertDisposed();
            StringBuilder sb;
            if (lines.ContainsKey( category ))
                sb = lines[category];
            else {
                sb = new StringBuilder();
                lines.Add( category, sb );
            }

            sb.AppendFormat( "{0}", message );
        }

        /// <summary>
        /// Posts the specified message and finishes a line. This method call propagates to log handlers.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <param name="args">The args.</param>
        public void PostLine( string category, string message, params object[] args ) {
            if (lines.ContainsKey( category )) {
                PostEvent( new LogEntry( EntryType.Message, category, lines[category].ToString() + (args.Length == 0 ? message : string.Format( message, args )) ) );
                lines.Remove( category );
            } else
                PostEvent( new LogEntry( EntryType.Message, category, args.Length == 0 ? message : string.Format( message, args ) ) );
        }

        /// <summary>
        /// Posts the specified message and finishes a line. This method call propagates to log handlers.
        /// </summary>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        public void PostLine( string category, object message ) {
            PostLine( category, "{0}", message );
        }

        /// <summary>
        /// Posts an empty message finishing a line. This method call propagates to log handlers.
        /// </summary>
        /// <param name="category">The category.</param>
        public void PostLine( string category ) {
            PostLine( category, "" );
        }

        /// <summary>
        /// Posts the specified entry.
        /// </summary>
        /// <param name="e">The entry.</param>
        public void PostEvent( LogEntry e ) {
            AssertDisposed();
            if (e.Type == EntryType.Error)
                HasErrors = true;

            if (ThrowOnException && e.Type == EntryType.Error && ((ErrorEntry) e).Error.Exception != null)
                throw ((ErrorEntry) e).Error.Exception;

            if (!RunInSeperateThread)
                Handle( e );
            else {
                lock (messages)
                    messages.Enqueue( e );

                spoolWaiter.Set();
            }
        }

        /// <summary>
        /// Opens the logger.
        /// </summary>
        public void Open() {
            AssertDisposed();
            Handlers = new List<ILogHandler>();
            RunInSeperateThread = false;
            HasErrors = false;
            ThrowOnException = false;
        }

        /// <summary>
        /// Closes the logger.
        /// </summary>
        public void Close() {
            AssertDisposed();
            if (RunInSeperateThread) {
                lock (typeof( Logger ))
                    running = false;

                spoolWaiter.Set();
                closeWaiter.WaitOne();
            }
            Handlers = new List<ILogHandler>();
            tasks = new Stack<ITask>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            if (isDisposed)
                return;
            lock (this)
                isDisposed = true;
            if (isDisposed)
                return;

            Close();
        }

        void AssertDisposed() {
            if (isDisposed)
                throw new InvalidOperationException( "Instance has been disposed." );
        }

        void SetupSpooler() {
            spoolWaiter = new EventWaitHandle( false, EventResetMode.AutoReset );
            closeWaiter = new EventWaitHandle( false, EventResetMode.AutoReset );

            spooler = new Thread( new ThreadStart( Spool ) );
            running = true;
            spooler.Start();
            spooler.IsBackground = true;
        }

        void Handle( LogEntry message ) {
            Handlers.ForEach( h => h.Handle( message ) );
        }

        #region Spool
        void Spool() {
            try {
                bool stillRunning;
                lock (typeof( Logger ))
                    stillRunning = running;
                while (stillRunning) {
                    LogEntry m = null;
                    lock (messages) {
                        if (messages.Count > 0)
                            m = messages.Dequeue();
                    }

                    if (m == null)
                        spoolWaiter.WaitOne();
                    else
                        Handle( m );

                    lock (typeof( Logger ))
                        stillRunning = running;
                }
            } finally {
                lock (typeof( Logger )) {
                    while (messages.Count > 0)
                        Handle( messages.Dequeue() );
                }

                closeWaiter.Set();
            }
        }
        #endregion
        #endregion
    }

    /// <summary>
    /// Specifies the different entry types.
    /// </summary>
    public enum EntryType {
        /// <summary>
        /// Specifies that the entry marks the beginning of a task.
        /// </summary>
        BeginTask,
        /// <summary>
        /// Specifies that the entry marks the end of a task.
        /// </summary>
        EndTask,
        /// <summary>
        /// Specifies that the entry is a normal message.
        /// </summary>
        Message,
        /// <summary>
        /// Specifies that the entry indicates an error.
        /// </summary>
        Error,
        /// <summary>
        /// Specifies that the entry indicates a warning.
        /// </summary>
        Warning
    }

    #region Message
    /// <summary>
    /// Represents an entry into the log.
    /// </summary>
    public class LogEntry {
        /// <summary>
        /// Gets the type of this instance.
        /// </summary>
        public readonly EntryType Type;
        /// <summary>
        /// Gets the timestamp of this instance.
        /// </summary>
        public readonly DateTime Timestamp;
        /// <summary>
        /// Gets the message of this instance.
        /// </summary>
        public readonly string Text;
        /// <summary>
        /// Gets the category of this instance.
        /// </summary>
        public readonly string Category;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntry"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="category">The category.</param>
        /// <param name="text">The text.</param>
        public LogEntry( EntryType type, string category, string text ) {
            this.Type = type;
            this.Timestamp = DateTime.Now;
            this.Category = category;
            this.Text = text;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.LogEntry"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.LogEntry"/>.
        /// </returns>
        public override string ToString() {
            return "[" + Timestamp + "]" + Category + ": " + Text;
        }
    }
    #endregion

    #region TaskEntry
    /// <summary>
    /// Specifies that the implementing class can be regarded as a logged task.
    /// </summary>
    public interface ITask {
        /// <summary>
        /// Gets the name of the task.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Specialized log entry used for task related entries.
    /// </summary>
    public sealed class TaskEntry : LogEntry {
        /// <summary>
        /// Gets the task this instance is regarding.
        /// </summary>
        public ITask Task { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskEntry"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="category">The category.</param>
        /// <param name="message">The message.</param>
        /// <param name="task">The task.</param>
        internal TaskEntry( EntryType type, string category, string message, ITask task )
            : base( type, category, message ) {
            this.Task = task;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.TaskEntry"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.TaskEntry"/>.
        /// </returns>
        public override string ToString() {
            return "[" + Timestamp + "]" + Category + ":" + Type + ": " + Text;
        }
    }
    #endregion

    #region ErrorEntry
    /// <summary>
    /// Specialized log entry used to indicate an error.
    /// </summary>
    public class ErrorEntry : LogEntry {
        /// <summary>
        /// Gets the object representing the error.
        /// </summary>
        public readonly Error Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorEntry"/> class.
        /// </summary>
        /// <param name="error">The error.</param>
        public ErrorEntry( Error error )
            : base( EntryType.Error, "error", error.Message ) {
            this.Error = error;
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.ErrorEntry"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:Sekhmet.Logging.ErrorEntry"/>.
        /// </returns>
        public override string ToString() {
            return "[" + Timestamp + "]" + Type + ": " + Text;
        }
    }
    #endregion
}

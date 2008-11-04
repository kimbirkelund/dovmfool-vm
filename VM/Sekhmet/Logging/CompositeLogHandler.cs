using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// Represents a log handler combining a number of other log handlers.
    /// </summary>
	public class CompositeLogHandler : ILogHandler {
        /// <summary>
        /// Gets or sets the handlers.
        /// </summary>
        /// <value>The handlers.</value>
		public IList<ILogHandler> Handlers { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompositeLogHandler"/> class.
        /// </summary>
		public CompositeLogHandler() {
			Handlers = new List<ILogHandler>();
		}

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
		public void Handle( LogEntry message ) {
			foreach (var handler in Handlers)
				handler.Handle( message );
		}
	}
}

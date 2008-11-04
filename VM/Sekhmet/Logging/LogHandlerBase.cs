using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// Common base class for log handlers, to seperate out the filtering of ignored messages.
    /// </summary>
    public abstract class LogHandlerBase : ILogHandler {
        /// <summary>
        /// Gets the ignored categories.
        /// </summary>
        /// <value>The ignore categories.</value>
        public List<string> IgnoreCategories { get; private set; }
        /// <summary>
        /// Gets or sets a value indicating whether only errors should be shown.
        /// </summary>
        /// <value><c>true</c> if only errors are shown; otherwise, <c>false</c>.</value>
        public bool ShowErrorsAndWarningsOnly { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogHandlerBase"/> class.
        /// </summary>
        public LogHandlerBase() {
            IgnoreCategories = new List<string>();
        }

        /// <summary>
        /// Handles the specified message. Filtering is performed and if not filtered out the message is passed to <see cref="M:DoHandle"/>.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Handle( LogEntry message ) {
            if (IgnoreCategories.Contains( message.Category ))
                return;
            if (ShowErrorsAndWarningsOnly && message.Type != EntryType.Error && message.Type != EntryType.Warning)
                return;

            DoHandle( message );
        }

        /// <summary>
        /// Performs the actual processing for a specific log handler.
        /// </summary>
        /// <param name="entry">The message to be processed.</param>
        protected abstract void DoHandle( LogEntry entry );
    }
}

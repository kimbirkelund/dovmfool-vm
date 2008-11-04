using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// Interface for a class that can act as a log handler.
    /// </summary>
	public interface ILogHandler {
        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
		void Handle( LogEntry message );
	}
}

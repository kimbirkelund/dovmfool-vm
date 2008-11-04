using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// Represents the interface for a log reader. Not all <see cref="ILogHandler"/>s must provide a log reader, 
    /// but any that provides the possibility of reading back the log should implement this interface.
    /// </summary>
    public interface ILogReader {
        /// <summary>
        /// Gets the entries currently in the log.
        /// </summary>
        /// <value>The entries.</value>
        IEnumerable<LogEntry> Entries { get; }
    }
}

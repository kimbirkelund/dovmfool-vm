using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sekhmet.Logging {
    /// <summary>
    /// Represents a log handler that can log messages to a specified <see cref="T:TextWriter"/>.
    /// </summary>
    public class PlainTextLogHandler : LogHandlerBase, IDisposable {
        TextWriter writer;

        /// <summary>
        /// Gets or sets a value indicating whether to write the time stamp.
        /// </summary>
        public bool WriteTimeStamp { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to write the category.
        /// </summary>
        public bool WriteCategory { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PlainTextLogHandler"/> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public PlainTextLogHandler( TextWriter writer ) {
            this.writer = writer;
            WriteTimeStamp = WriteCategory = true;
        }

        /// <summary>
        /// Performs the actual processing for a specific log handler.
        /// </summary>
        /// <param name="entry">The message to be processed.</param>
        protected override void DoHandle( LogEntry entry ) {
            switch (entry.Type) {
                case EntryType.BeginTask:
                    var bpe = (TaskEntry) entry;
                    writer.WriteLine( "{0}Beginning task '{1}'", WriteTimeStamp ? "[" + bpe.Timestamp + "] " : "", bpe.Task.Name );
                    break;
                case EntryType.EndTask:
                    var epe = (TaskEntry) entry;
                    writer.WriteLine( "{0}Ending task '{1}'", WriteTimeStamp ? "[" + epe.Timestamp + "] " : "", epe.Task.Name );
                    break;
                case EntryType.Warning:
                case EntryType.Message:
                    writer.WriteLine( "{0}{1}{2}{3}{4}", WriteTimeStamp ? "[" + entry.Timestamp + "]" : "", WriteCategory || WriteTimeStamp ? " " : "", WriteCategory ? entry.Category : "", WriteCategory || WriteTimeStamp ? ": " : "", entry.Text );
                    break;
                case EntryType.Error:
                    var err = (ErrorEntry) entry;
                    writer.WriteLine( "{0}{1}{2}", WriteTimeStamp ? entry.Timestamp + " " : "", err.Error, err.Error.Exception != null ? ": " + err.Error.Exception : "" );
                    break;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose() {
            writer.Dispose();
        }
    }
}

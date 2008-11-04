using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sekhmet.Logging {
    /// <summary>
    /// Specialization of <see cref="PlainTextLogHandler"/> that logs directly to the console. Supports different colors for different message types.
    /// </summary>
    public class ConsoleLogHandler : PlainTextLogHandler {
        /// <summary>
        /// Gets or sets the color to use for BeginTask.
        /// </summary>
        public ConsoleColor BeginTaskColor { get; set; }
        /// <summary>
        /// Gets or sets the end color to use for EndTask.
        /// </summary>
        public ConsoleColor EndTaskColor { get; set; }
        /// <summary>
        /// Gets or sets the color to use for normal messages.
        /// </summary>
        public ConsoleColor MessageColor { get; set; }
        /// <summary>
        /// Gets or sets the color to use for errors.
        /// </summary>
        public ConsoleColor ErrorColor { get; set; }
        /// <summary>
        /// Gets or sets the color of warnings.
        /// </summary>
        /// <value>The color of the warning.</value>
        public ConsoleColor WarningColor { get; set; }

        /// <summary>
        /// Gets or sets the colors to use for specific categories.
        /// </summary>
        /// <value>The category colors.</value>
        public Dictionary<string, ConsoleColor> CategoryColors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLogHandler"/> class.
        /// </summary>
        public ConsoleLogHandler()
            : base( Console.Out ) {
            BeginTaskColor = EndTaskColor = MessageColor = ErrorColor = Console.ForegroundColor;
            CategoryColors = new Dictionary<string, ConsoleColor>();
        }

        /// <summary>
        /// Computes the color to use for a specific entry.
        /// </summary>
        protected ConsoleColor GetColor( LogEntry entry ) {
            switch (entry.Type) {
                case EntryType.BeginTask:
                    return BeginTaskColor;
                case EntryType.EndTask:
                    return EndTaskColor;
                case EntryType.Message:
                    if (!CategoryColors.ContainsKey( entry.Category ))
                        return MessageColor;
                    return CategoryColors[entry.Category];
                case EntryType.Warning: return WarningColor;
                case EntryType.Error: return ErrorColor;
                default:
                    return Console.ForegroundColor;
            }
        }

        /// <summary>
        /// Performs the actual processing for a specific log handler.
        /// </summary>
        /// <param name="entry">The message to be processed.</param>
        protected override void DoHandle( LogEntry entry ) {
            var cc = Console.ForegroundColor;
            Console.ForegroundColor = GetColor( entry );

            base.DoHandle( entry );

            Console.ForegroundColor = cc;
        }
    }
}

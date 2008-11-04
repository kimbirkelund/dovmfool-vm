using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.Editing {
    /// <summary>
    /// Errors collection used during rendering.
    /// </summary>
    public class Errors {
        IList<ParsingError> errorList;
        /// <summary>
        /// Gets the error list.
        /// </summary>
        /// <value>The error list.</value>
        public IEnumerable<ParsingError> ErrorList {
            get { return errorList; }
        }

        bool hasErrors;
        /// <summary>
        /// Gets a value indicating whether this instance has errors.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance has errors; otherwise, <c>false</c>.
        /// </value>
        public bool HasErrors {
            get { return hasErrors; }
        }

        bool throws;
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Errors"/> throws an exception on each error.
        /// </summary>
        public bool Throws {
            get { return throws; }
            set { throws = value; }
        }

        bool warningsAsErrors;
        /// <summary>
        /// Gets or sets a value indicating whether warnings should be treated as errors.
        /// </summary>
        public bool WarningsAsErrors {
            get { return warningsAsErrors; }
            set { warningsAsErrors = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Errors"/> class.
        /// </summary>
        public Errors() {
            errorList = new List<ParsingError>();
        }

        /// <summary>
        /// Adds the specified error.
        /// </summary>
        /// <param name="error">The error.</param>
        public void Add( ParsingError error ) {
            hasErrors = true;
            errorList.Add( error );
            if (throws && (!error.IsWarning || WarningsAsErrors))
                throw new ParsingException( error );
        }
    }
}

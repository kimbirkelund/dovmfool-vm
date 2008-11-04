using System;
using System.Collections.Generic;
using System.Text;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Base class for the common types of command line arguments.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ArgumentBase<T> : IArgument {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        T value;
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public virtual T Value {
            get { return value; }
            protected set {
                this.value = value;
                IsPresent = true;
            }
        }
        /// <summary>
        /// Gets a value indicating whether this instance is present.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is present; otherwise, <c>false</c>.
        /// </value>
        public bool IsPresent { get; protected set; }

        /// <summary>
        /// Gets or sets the position this argument can be specified in on the command line without using its name.
        /// </summary>
        /// <value>The position; or -1 if not a positional argument.</value>
        public int Position { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required. Parsing a set of arguments not containing this argument will fail.
        /// </summary>
        /// <value></value>
        public bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets a textual human-readable description of the purpose of this argument.
        /// </summary>
        /// <value></value>
        public string Description { get; set; }

        /// <summary>
        /// Gets additional information to be output to the user about the usage of this specific argument.
        /// </summary>
        /// <value></value>
        public IDictionary<string, string> UsageInformation { get; private set; }

        /// <summary>
        /// Gets a short description (less than 10 chars preferably) of the value type this argument expects.
        /// </summary>
        /// <value></value>
        public string ShortValueTypeDescription { get; private set; }

        /// <summary>
        /// Gets or sets the argument group.
        /// </summary>
        /// <value>The argument group.</value>
        public int ArgumentGroup { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentBase&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="shortValueTypeDescription">The short value type description.</param>
        /// <param name="name">The name.</param>
        public ArgumentBase( string shortValueTypeDescription, string name ) {
            this.ShortValueTypeDescription = shortValueTypeDescription;
            this.Name = name;
            this.Position = -1;
            this.ArgumentGroup = -1;
            this.UsageInformation = new Dictionary<string, string>();
        }

        /// <summary>
        /// Attempts to set the value of the arguments; on failing the reason is written to <c>failureReason</c>.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="failureReason">The failure reason.</param>
        /// <returns>
        /// A value indicating whether setting the value was succesfully; if <c>false</c>
        /// 	<c>failureReason</c> contains the reason for the faiure.
        /// </returns>
        public abstract bool TrySetValue( string value, out string failureReason );

        #region IArgument Members
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        object IArgument.Value {
            get { return this.Value; }
        }
        #endregion
    }
}

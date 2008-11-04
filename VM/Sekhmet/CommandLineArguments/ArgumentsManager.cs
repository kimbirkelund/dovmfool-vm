using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sekhmet.Collections;
using System.IO;

namespace Sekhmet.CommandLineArguments {
    /// <summary>
    /// Performs management and parsing of command-line arguments.
    /// </summary>
    public class ArgumentsManager {
        /// <summary>
        /// Gets the list of arguments in this manager.
        /// </summary>
        public IList<IArgument> Arguments { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArgumentsManager"/> class.
        /// </summary>
        /// <param name="arguments">The arguments managed by this instance.</param>
        public ArgumentsManager( params IArgument[] arguments ) {
            this.Arguments = new System.Collections.Generic.List<IArgument>( arguments );
        }

        /// <summary>
        /// Adds the specified argument to the list.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument.</typeparam>
        /// <param name="argument">The argument.</param>
        /// <returns>The supplied argument.</returns>
        public TArg AddArgument<TArg>( TArg argument ) where TArg : IArgument {
            Arguments.Add( argument );
            return argument;
        }

        /// <summary>
        /// Parses the specified arguments.
        /// </summary>
        /// <param name="argumentValues">The argument values.</param>
        public void Parse( params string[] argumentValues ) {
            var trie = CreateTrie( Arguments );
            var abbrs = CreateAbbrDict( Arguments );
            var positionalArguments = Arguments.Where( a => a.Position != -1 ).OrderBy( a => a.Position ).ToList();
            var selectedGroup = -1;
            var requiredArgs = Arguments.Where( a => a.IsRequired ).ToList();

            string failureReason = null;

            var positionalArgsCount = 0;
            for (; positionalArgsCount < positionalArguments.Count; positionalArgsCount++) {
                if (positionalArgsCount >= argumentValues.Length || argumentValues[positionalArgsCount].StartsWith( "-" ))
                    break;

                if (!positionalArguments[positionalArgsCount].TrySetValue( argumentValues[positionalArgsCount], out failureReason ))
                    throw new ArgumentMatchException( failureReason );
                if (positionalArguments[positionalArgsCount].IsRequired)
                    requiredArgs.Remove( positionalArguments[positionalArgsCount] );
                if (positionalArguments[positionalArgsCount].ArgumentGroup != -1) {
                    if (selectedGroup == -1)
                        selectedGroup = positionalArguments[positionalArgsCount].ArgumentGroup;
                    else if (selectedGroup != positionalArguments[positionalArgsCount].ArgumentGroup)
                        throw new ArgumentMatchException( "Arguments from different argument groups specified." );
                }
            }

            argumentValues = argumentValues.Skip( positionalArgsCount ).ToArray();

            for (int i = 0; i < argumentValues.Length; i++) {
                if (!argumentValues[i].StartsWith( "-" ))
                    throw new ArgumentMatchException( "Unnamed argument where named argument was expected." );

                var name = argumentValues[i].Substring( 1 );
                var lowerName = name.ToLower();
                string value = null;

                if (string.IsNullOrEmpty( name ))
                    throw new ArgumentMatchException( "Named argument without a name found." );

                IArgument arg;
                var match = trie.Find( lowerName );
                if (match != null)
                    arg = match;
                else {
                    var matches = trie.FindAll( lowerName );
                    if (matches.Count() > 1) {
                        if (name.All( c => char.IsUpper( c ) ) && abbrs.ContainsKey( lowerName ))
                            arg = abbrs[lowerName];
                        else
                            throw new InconclusiveMatchException( name, matches );
                    } else if (matches.Count() == 0) {
                        if (abbrs.ContainsKey( lowerName ))
                            arg = abbrs[lowerName];
                        else
                            throw new ArgumentMatchException( "Unknown argument '" + name + "' found." );
                    } else
                        arg = matches.First();
                }

                if (arg.IsPresent)
                    throw new ArgumentMatchException( "Argument '" + arg.Name + "' occurs more than once." );

                if (argumentValues.Length - 1 != i && !argumentValues[i + 1].StartsWith( "-" ))
                    value = argumentValues[++i];

                if (arg.ArgumentGroup != -1) {
                    if (selectedGroup == -1)
                        selectedGroup = arg.ArgumentGroup;
                    else if (selectedGroup != arg.ArgumentGroup)
                        throw new ArgumentMatchException( "Arguments from different argument groups specified." );
                }

                if (!arg.TrySetValue( value, out failureReason ))
                    throw new ArgumentMatchException( failureReason );

                if (arg.IsRequired)
                    requiredArgs.Remove( arg );
            }

            if (requiredArgs.Where( a => selectedGroup == -1 || a.ArgumentGroup == -1 || a.ArgumentGroup == selectedGroup ).Count() != 0)
                throw new ArgumentMatchException( "The following required arguments was not specified: " + requiredArgs.Select( a => a.Name ).Join( ", " ) + "." );
        }

        static Trie<IArgument> CreateTrie( IEnumerable<IArgument> args ) {
            var trie = new Trie<IArgument>();

            args.ForEach( a => trie.Insert( a.Name.ToLower(), a ) );

            return trie;
        }

        static IDictionary<string, IArgument> CreateAbbrDict( IEnumerable<IArgument> args ) {
            var dict = new Dictionary<string, IArgument>();

            foreach (var arg in args) {
                var abbr = arg.Name.Where( c => char.IsUpper( c ) ).Join( "" ).ToLower();
                if (dict.ContainsKey( abbr ))
                    dict.Remove( abbr );
                else
                    dict.Add( abbr, arg );
            }

            return dict;
        }

        /// <summary>
        /// Prints the syntax.
        /// </summary>
        /// <param name="executable">The executable.</param>
        /// <param name="lineWidth">Width of the line.</param>
        /// <param name="writer">The writer.</param>
        public void PrintSyntax( string executable, int lineWidth, TextWriter writer ) {
            PrintIndented( writer, GetSyntax( executable ), lineWidth, 0, 1 );
        }

        /// <summary>
        /// Prints the usage.
        /// </summary>
        /// <param name="executable">The executable.</param>
        /// <param name="lineWidth">Width of the line.</param>
        /// <param name="writer">The writer.</param>
        public void PrintUsage( string executable, int lineWidth, TextWriter writer ) {
            writer.WriteLine( "SYNTAX:" );
            PrintIndented( writer, GetSyntax( executable ), lineWidth, 4, 5 );

            writer.WriteLine();
            writer.WriteLine( "PARAMETERS:" );

            var positionalArguments = Arguments.Where( a => a.Position != -1 ).OrderBy( a => a.Position ).ToList();
            var args = positionalArguments.Concat( Arguments.OrderBy( a => a.Name ).Except( positionalArguments ) );
            var first = true;
            foreach (var arg in args) {
                if (!first)
                    writer.WriteLine();
                first = false;

                PrintIndented( writer, "-" + arg.Name + (arg.ShortValueTypeDescription != null ? " <" + arg.ShortValueTypeDescription + ">" : ""), lineWidth, 4, 4 );
                if (arg.Description != null)
                    PrintIndented( writer, arg.Description, lineWidth, 8, 9 );

                var maxWidth = new[] { "Required?", "Position?", "Argument group" }.Concat( arg.UsageInformation.Keys ).Select( s => s.Length ).Max() + 2;
                writer.WriteLine();

                WriteProperty( writer, lineWidth, maxWidth, 8, "Required?", arg.IsRequired.ToString() );
                WriteProperty( writer, lineWidth, maxWidth, 8, "Position?", arg.Position != -1 ? arg.Position.ToString() : "named" );
                WriteProperty( writer, lineWidth, maxWidth, 8, "Argument group", arg.ArgumentGroup != -1 ? arg.ArgumentGroup.ToString() : "any" );
                foreach (var e in arg.UsageInformation)
                    WriteProperty( writer, lineWidth, maxWidth, 8, e.Key, e.Value );
            }
        }

        void WriteProperty( TextWriter writer, int lineWidth, int maxWidth, int indentFirst, string property, string value ) {
            var str = property + "".PadLeft( maxWidth - property.Length ) + value;

            PrintIndented( writer, str, lineWidth, indentFirst, indentFirst + maxWidth );
        }

        string GetSyntax( string executable ) {
            var positionalArguments = Arguments.Where( a => a.Position != -1 ).OrderBy( a => a.Position ).ToList();
            var args = positionalArguments.Concat( Arguments.OrderBy( a => a.Name ).Except( positionalArguments ) );
            var sb = new StringBuilder();

            sb.Append( executable + " " );

            foreach (var arg in args) {
                if (!arg.IsRequired)
                    sb.Append( "[" );

                if (arg.Position != -1)
                    sb.Append( "[" );

                sb.Append( "-" + arg.Name );

                if (arg.Position != -1)
                    sb.Append( "]" );

                if (arg.ShortValueTypeDescription != null)
                    sb.Append( " <" + arg.ShortValueTypeDescription + ">" );

                if (!arg.IsRequired)
                    sb.Append( "]" );

                sb.Append( " " );
            }

            return sb.ToString();
        }

        void PrintIndented( TextWriter writer, string str, int lineWidth, int indent ) {
            PrintIndented( writer, str, lineWidth, indent, indent );
        }

        void PrintIndented( TextWriter writer, string str, int lineWidth, int indentFirst, int indentNexts ) {
            if (indentFirst >= lineWidth)
                throw new ArgumentException( "Indentation must be less than the width of the line.", "indentFirst" );
            if (indentNexts >= lineWidth)
                throw new ArgumentException( "Indentation must be less than the width of the line.", "indentNexts" );

            bool first = true;
            while (str.Length != 0) {
                var indent = first ? indentFirst : indentNexts;
                first = false;
                writer.Write( "".PadLeft( indent, ' ' ) );

                var pos = lineWidth - indent >= str.Length ? str.Length : str.LastIndexOf( ' ', lineWidth - indent );
                if (pos == -1)
                    pos = Math.Min( lineWidth - indent, str.Length );
                var subs = str.Substring( 0, pos );
                str = str.Substring( subs.Length ).TrimStart();

                if (subs.Length + indent == lineWidth)
                    writer.Write( subs );
                else
                    writer.WriteLine( subs );
            }
        }
    }
}

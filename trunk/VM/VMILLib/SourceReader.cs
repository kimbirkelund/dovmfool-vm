using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet.Logging;

namespace VMILLib {
	public sealed class SourceReader : IDisposable {
		public Logger Logger { get; set; }

		bool isDisposed;
		Stream input;

		Assembly assembly;
		Dictionary<string, CString> strings = new Dictionary<string, CString>();
		Dictionary<int, CInteger> integers = new Dictionary<int, CInteger>();

		public SourceReader( Stream input ) {
			this.input = input;
		}

		public SourceReader( string input ) : this( new FileStream( input, FileMode.Open ) ) { }

		public Assembly Read() {
			if (assembly == null)
				assembly = ReadProgram();
			return assembly;
		}

		Assembly ReadProgram() {
			var scanner = new Parser.Scanner( input ) { Logger = Logger ?? new Logger() };
			var parser = new Parser.Parser() { scanner = scanner };
			if (!parser.Parse())
				throw new ArgumentException( "Input is not a valid VMIL source file." );

			var classes = new ClassList( parser.Classes.Select( c => ReadClass( c ) ) );
			var stringPool = new CStringPool( strings.Values );
			var integerPool = new CIntegerPool( integers.Values );

			return new Assembly( stringPool, integerPool, classes );
		}

		Class ReadClass( Parser.Class cls ) {
			var visibility = cls.Visibility;
			var name = ReadString( cls.Name );
			var inheritsFrom = ReadNames( cls.InheritsFrom );
			var fields = cls.Fields;
			var defaultHandler = ReadMessageHandler( cls.DefaultHandler );
			var handlers = new MessageHandlerList( cls.Handlers.Select( h => ReadMessageHandler( h ) ) );
			var classes = new ClassList( cls.Classes.Select( c => ReadClass( c ) ) );

			return new Class( visibility, name, inheritsFrom, fields, defaultHandler, handlers, classes );
		}

		MessageHandler ReadMessageHandler( Parser.MessageHandler handler ) {
			if (handler == null)
				return null;

			var visibility = handler.Visibility;
			var name = handler.Name != null ? ReadString( handler.Name + ":" + handler.Arguments.Count ) : null;
			var arguments = handler.Arguments;
			var locals = handler.Locals;
			var instructions = ReadInstructions( handler.Instructions );

			return new MessageHandler( visibility, name, arguments, locals, instructions );
		}

		int VerifyTryCatches( Parser.List<Parser.Instruction> inss, int index ) {
			var foundLabels = new List<string>();
			var unfoundLabel = new List<string>();

			for (int j = index + 1; j < inss.Count; j++) {
				if (inss[j].OpCode == OpCode.Try) {
					j = VerifyTryCatches( inss, j );
					continue;
				}
				if (inss[j].OpCode == OpCode.EndTryCatch) {
					if (unfoundLabel.Count != 0)
						throw new ArgumentException( "Invalid input program: jump to outside of try-block found." );
					return j;
				}

				if (index != -1) {
					if (inss[j].OpCode == OpCode.Jump || inss[j].OpCode == OpCode.JumpIfFalse || inss[j].OpCode == OpCode.JumpIfTrue) {
						if (!foundLabels.Contains( (string) inss[j].Operand ))
							unfoundLabel.Add( (string) inss[j].Operand );
					}

					if (inss[j] is Parser.Label) {
						var l = (Parser.Label) inss[j];
						foundLabels.Add( l.Name );
						unfoundLabel.RemoveAll( s => s == l.Name );
					}
				}
			}

			return -1;
		}

		InstructionList ReadInstructions( Parser.List<Parser.Instruction> list ) {
			VerifyTryCatches( list, -1 );
			return new InstructionList( list.Select( i => ReadInstruction( i ) ) );
		}

		Instruction ReadInstruction( Parser.Instruction ins ) {
			switch (ins.OpCode) {
				case OpCode.StoreField:
				case OpCode.LoadField:
				case OpCode.StoreLocal:
				case OpCode.LoadLocal:
					return new Instruction( ins.OpCode, (string) ins.Operand );
				case OpCode.PushLiteral:
					return new Instruction( OpCode.PushLiteral, ins.Operand is string ? (object) ReadString( (string) ins.Operand ) : ReadInteger( (int) ins.Operand ) );
				case OpCode.Jump:
				case OpCode.JumpIfTrue:
				case OpCode.JumpIfFalse:
					return new Instruction( ins.OpCode, (string) ins.Operand );
				case OpCode.Pop:
				case OpCode.Dup:
				case OpCode.NewInstance:
				case OpCode.SendMessage:
				case OpCode.ReturnVoid:
				case OpCode.Return:
				case OpCode.Throw:
				case OpCode.Try:
				case OpCode.Catch:
				case OpCode.EndTryCatch:
					return new Instruction( ins.OpCode );
				case OpCode.None:
					if (ins is Parser.Label)
						return new Label( ((Parser.Label) ins).Name );
					throw new ArgumentOutOfRangeException( "Invalid OpCode encountered: '" + ins.OpCode + "'." );
				default:
					throw new ArgumentOutOfRangeException( "Invalid OpCode encountered: '" + ins.OpCode + "'." );
			}
		}

		NameList ReadNames( Parser.List<string> list ) {
			return new NameList( list.Select( v => ReadString( v ) ) );
		}

		CString ReadString( string str ) {
			if (!strings.ContainsKey( str ))
				strings.Add( str, new CString( strings.Count, str ) );
			return strings[str];
		}

		CInteger ReadInteger( int i ) {
			if (!integers.ContainsKey( i ))
				integers.Add( i, new CInteger( integers.Count, i ) );
			return integers[i];
		}

		#region IDisposable Members
		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}
			if (input != null) {
				input.Dispose();
				input = null;
			}
		}
		#endregion
	}
}

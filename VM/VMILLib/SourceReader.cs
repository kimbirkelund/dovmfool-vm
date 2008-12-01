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
				return null;

			var classes = new ClassList( parser.Classes.Select( c => ReadClass( c ) ) );

			return new Assembly( classes );
		}

		Class ReadClass( Parser.Class cls ) {
			var visibility = cls.Visibility;
			var name = cls.Name;
			var inheritsFrom = ReadNames( cls.InheritsFrom );
			var fields = cls.Fields;
			var defaultHandler = ReadMessageHandler( cls.DefaultHandler );
			var handlers = new MessageHandlerList( cls.Handlers.Select( h => ReadMessageHandler( h ) ) );
			var classes = new ClassList( cls.Classes.Select( c => ReadClass( c ) ) );

			return new Class( visibility, name, inheritsFrom, fields, defaultHandler, handlers, classes );
		}

		MessageHandlerBase ReadMessageHandler( Parser.MessageHandlerBase handlerBase ) {
			if (handlerBase == null)
				return null;

			var visibility = handlerBase.Visibility;
			var name = handlerBase.Name != null ? handlerBase.Name + ":" + handlerBase.Arguments.Count : null;
			var arguments = handlerBase.Arguments;

			if (handlerBase is Parser.ExternalMessageHandler) {
				var handler = (Parser.ExternalMessageHandler) handlerBase;

				var externalName = handler.ExternalName + ":" + handlerBase.Arguments.Count;

				return new ExternalMessageHandler( visibility, name, externalName, arguments );
			} else {
				var handler = (Parser.VMILMessageHandler) handlerBase;
				var locals = handler.Locals;
				var instructions = ReadInstructions( handler.Instructions );

				return new VMILMessageHandler( visibility, name, arguments, locals, instructions, handler.IsEntrypoint );
			}
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
				case OpCode.LoadArgument:
					return new Instruction( ins.OpCode, (string) ins.Operand );
				case OpCode.PushLiteral:
					return new Instruction( OpCode.PushLiteral, ins.Operand is string ? (object) (string) ins.Operand : (int) ins.Operand );
				case OpCode.Jump:
				case OpCode.JumpIfTrue:
				case OpCode.JumpIfFalse:
					return new Instruction( ins.OpCode, (string) ins.Operand );
				case OpCode.LoadThis:
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
			return new NameList( list );
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

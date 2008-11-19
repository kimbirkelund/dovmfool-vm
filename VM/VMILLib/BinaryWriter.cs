using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet;

namespace VMILLib {
	public sealed class BinaryWriter : IDisposable {
		bool isDisposed;
		Stream output;
		int pos;
		Dictionary<MessageHandlerBase, int> handlers;
		Dictionary<Class, int> classes;

		public BinaryWriter( Stream output ) {
			this.output = output;
		}

		public BinaryWriter( string output ) : this( new FileStream( output, FileMode.Create, FileAccess.Write ) ) { }

		public void Write( Assembly assembly ) {
			handlers = new Dictionary<MessageHandlerBase, int>();
			classes = new Dictionary<Class, int>();

			WriteStrings( assembly.Strings );
			WriteHandlers( assembly.Classes );
			WriteClasses( assembly.Classes );
		}

		void WriteClasses( ClassList classList ) {
			Write( CountClasses( classList ) );

			classList.ForEach( WriteClass );
		}

		int CountClasses( ClassList classList ) {
			return classList.Aggregate( classList.Count, ( count, cls ) => count += CountClasses( cls.Classes ), count => count );
		}

		void WriteClass( Class cls ) {
			foreach (var ccls in cls.Classes)
				WriteClass( ccls );

			classes.Add( cls, classes.Count + 1 );

			Write( cls.InheritsFrom.Count );
			Write( cls.Fields.Count );
			Write( cls.Handlers.Count );
			Write( cls.Classes.Count );
			Write( (cls.Name.Index << 3) | (int) cls.Visibility );

			cls.InheritsFrom.ForEach( s => Write( s.Index ) );

			if (cls.DefaultHandler == null)
				Write( 0 );
			else
				Write( handlers[cls.DefaultHandler] );

			foreach (var handler in cls.Handlers)
				Write( handlers[handler] );

			foreach (var innerCls in cls.Classes)
				Write( classes[innerCls] );
		}

		void WriteHandlers( ClassList classes ) {
			MapHandlers( classes );
			Write( handlers.Count );

			handlers.Keys.ToArray().ForEach( h => WriteHandler( h ) );
		}

		void WriteHandler( MessageHandlerBase handlerBase ) {
			Write( handlerBase.Name == null ? (int) VisibilityModifier.None : (handlerBase.Name.Index << 4) | (handlerBase.IsExternal ? 4 : 0) | (handlerBase.IsEntrypoint ? 8 : 0) | (int) handlerBase.Visibility );
			Write( handlerBase.Arguments.Count );

			if (handlerBase is ExternalMessageHandler) {
				var handler = (ExternalMessageHandler) handlerBase;

				Write( handler.ExternalName.Index );
			} else {
				var handler = (VMILMessageHandler) handlerBase;

				Write( handler.Locals.Count );
				Write( handler.Instructions.Where( i => !(i is Label) ).Count() );
				WriteInstructions( handler.Instructions );
			}
		}

		void WriteInstructions( InstructionList inssList ) {
			var labelMap = new Dictionary<string, int>();
			inssList.ForEach( ( ins, i ) => { if (ins is Label) labelMap.Add( ((Label) ins).Name, i - labelMap.Count ); } );
			var inss = inssList.Where( i => !(i is Label) );

			var trycatchMap = MapTryCatches( inss );
			var trycatchStack = new Stack<TryCatchRecord>();

			int index = 0;
			foreach (var ins in inss) {
				uint eins = (uint) ins.OpCode << 27;
				switch (ins.OpCode) {
					case OpCode.StoreField:
					case OpCode.LoadField:
						eins |= (uint) ins.MessageHandler.Class.Fields.IndexOf( (string) ins.Operand );
						break;
					case OpCode.StoreLocal:
					case OpCode.LoadLocal:
						eins |= (uint) ins.MessageHandler.Locals.IndexOf( (string) ins.Operand );
						break;
					case OpCode.LoadArgument:
						eins |= (uint) ins.MessageHandler.Arguments.IndexOf( (string) ins.Operand ) + 1;
						break;
					case OpCode.LoadThis:
						eins = (uint) OpCode.LoadArgument << 27;
						break;
					case OpCode.PushLiteral:
						if (ins.Operand is CString)
							eins = ((uint) OpCode.PushLiteralString << 27) | (uint) ((CString) ins.Operand).Index;
						else {
							int i = (int) ins.Operand;
							uint ai = i == int.MinValue ? (uint) i : (uint) Math.Abs( i );
							if (ai > 0x03FFFFFF) {
								uint i1 = (ai >> 16) & 0x0000FFFF;
								uint i2 = ai & 0x0000FFFF;
								eins = ((uint) OpCode.PushLiteralInt << 27) | (uint) (i < 0 ? 1 << 26 : 0) | i1;
								Write( eins );
								eins = ((uint) OpCode.PushLiteralIntExtend << 27) | (uint) (i < 0 ? 1 << 26 : 0) | i2;
							} else
								eins = ((uint) OpCode.PushLiteralInt << 27) | (uint) (i < 0 ? 1 << 26 : 0) | (uint) Math.Abs( i );
						}
						break;
					case OpCode.Pop:
					case OpCode.Dup:
					case OpCode.NewInstance:
					case OpCode.SendMessage:
					case OpCode.ReturnVoid:
					case OpCode.Return:
					case OpCode.Throw:
						break;
					case OpCode.Jump:
					case OpCode.JumpIfTrue:
					case OpCode.JumpIfFalse:
						var offset = labelMap[(string) ins.Operand] - index;
						if (offset < 0) {
							offset *= -1;
							eins |= 1 << 27;
						}
						eins |= (uint) offset;
						break;
					case OpCode.Try:
						var tryrecord = trycatchMap[index];
						trycatchStack.Push( tryrecord );
						eins |= (uint) (tryrecord.CatchIndex + 1 - index);
						break;
					case OpCode.Catch:
						eins |= (uint) (trycatchStack.Peek().EndTryCatch + 1 - index);
						break;
					case OpCode.EndTryCatch:
						trycatchStack.Pop();
						break;
					default:
						throw new ArgumentException( "Unexpected opcode : " + ins.OpCode );
				}

				Write( eins );
				index++;
			}
		}

		Dictionary<int, TryCatchRecord> MapTryCatches( IEnumerable<Instruction> inss ) {
			var stack = new Stack<TryCatchRecord>();
			var map = new Dictionary<int, TryCatchRecord>();

			int index = 0;
			foreach (var ins in inss) {
				if (ins.OpCode == OpCode.Try) {
					stack.Push( new TryCatchRecord { TryIndex = index } );
					map.Add( index, stack.Peek() );
				}

				if (ins.OpCode == OpCode.Catch)
					stack.Peek().CatchIndex = index;

				if (ins.OpCode == OpCode.EndTryCatch) {
					stack.Peek().EndTryCatch = index;
					stack.Pop();
				}

				index++;
			}

			return map;
		}

		void MapHandlers( ClassList classes ) {
			foreach (var cls in classes) {
				if (cls.DefaultHandler != null)
					handlers.Add( cls.DefaultHandler, handlers.Count + 1 );
				cls.Handlers.ForEach( h => handlers.Add( h, handlers.Count + 1 ) );
				MapHandlers( cls.Classes );
			}
		}

		void WriteStrings( CStringPool pool ) {
			Write( (uint) pool.Count );

			foreach (var s in pool.Select( s => s.Value )) {
				Write( s.Length );
				var bytes = Encoding.Unicode.GetBytes( s );

				var remainder = bytes.Length % 4;
				if (remainder != 0) {
					var temp = new byte[bytes.Length + (4 - remainder)];
					Array.Copy( bytes, 0, temp, 0, bytes.Length );
					bytes = temp;
				}

				Write( bytes );
			}
		}

		void Write( IEnumerable<uint> arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params uint[] arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( IEnumerable<int> arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params int[] arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params byte[] arr ) {
			if (arr.Length % 4 != 0)
				throw new ArgumentException( "Array length must be divisible with 4." );

			output.Write( arr, 0, arr.Length );
			pos += arr.Length / 4;
		}

		#region IDisposable Members
		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}
			if (output != null) {
				output.Dispose();
				output = null;
			}
		}
		#endregion

		class TryCatchRecord {
			public int TryIndex;
			public int CatchIndex;
			public int EndTryCatch;
		}
	}
}

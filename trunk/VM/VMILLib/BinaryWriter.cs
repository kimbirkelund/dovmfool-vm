using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VMILLib {
	public sealed class BinaryWriter : IDisposable {
		bool isDisposed;
		Stream output;
		int pos;
		Dictionary<MessageHandler, int> handlers;
		Dictionary<Class, int> classes;

		public BinaryWriter( Stream output ) {
			this.output = output;
		}

		public BinaryWriter( string output ) : this( new FileStream( output, FileMode.Create, FileAccess.Write ) ) { }

		public void Write( Assembly assembly ) {
			handlers = new Dictionary<MessageHandler, int>();
			classes = new Dictionary<Class, int>();

			WriteIntegers( assembly.Integers );
			WriteStrings( assembly.Strings );
			WriteHandlers( assembly.Classes );
			WriteClasses( assembly.Classes );
		}

		void WriteClasses( ClassList classList ) {
			MapClasses( classList );
			Write( this.classes.Count );

			var classes = new Queue<Class>();
			this.classes.Keys.ToArray().Where( c => !WriteClass( c ) ).ForEach( c => classes.Enqueue( c ) );

			while (classes.Count > 0)
				if (WriteClass( classes.Peek() ))
					classes.Dequeue();
		}

		bool WriteClass( Class cls ) {
			if (cls.Classes.Any( c => classes[c] == 0 ))
				return false;

			classes[cls] = pos;
			uint size = (uint) (6 + cls.InheritsFrom.Count + cls.Handlers.Count * 2 + cls.Classes.Count * 2);

			Write( (size << 4) | (uint) InternalObjectType.Class );
			Write( (cls.Name.Index << 3) | (int) cls.Visibility );
			Write( 0 );
			Write( (cls.Fields.Count << 18) | ((0x00003FFF & cls.Handlers.Count) << 4) | (cls.InheritsFrom.Count & 0x0000000F) );

			cls.InheritsFrom.ForEach( s => Write( s.Index ) );

			if (cls.DefaultHandler == null)
				Write( 0, 0 );
			else
				Write( (int) VisibilityModifier.Private, handlers[cls.DefaultHandler] );

			foreach (var handler in cls.Handlers)
				Write( (handler.Name.Index << 3) | (int) handler.Visibility, handlers[handler] );

			foreach (var innerCls in cls.Classes)
				Write( (innerCls.Name.Index << 3) | (int) innerCls.Visibility, classes[innerCls] );

			return true;
		}

		void MapClasses( ClassList classes ) {
			foreach (var cls in classes) {
				this.classes.Add( cls, 0 );
				MapClasses( cls.Classes );
			}
		}

		void WriteHandlers( ClassList classes ) {
			MapHandlers( classes );
			Write( handlers.Count );

			handlers.Keys.ToArray().ForEach( h => WriteHandler( h ) );
		}

		void WriteHandler( MessageHandler handler ) {
			handlers[handler] = pos;
			uint size = (uint) handler.Instructions.Where( i => !(i is Label) ).Count() + 3;

			Write( (size << 4) | (uint) InternalObjectType.VMILMessageHandler );
			Write( ((handler.Name == null ? 0 : handler.Name.Index) << 3) | (int) handler.Visibility );
			Write( (handler.Arguments.Count << 16) | ((handler.Arguments.Count + handler.Locals.Count)) );
			WriteInstructions( handler.Instructions );
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
						eins |= (uint) (ins.MessageHandler.Arguments.Contains( (string) ins.Operand ) ? ins.MessageHandler.Arguments.IndexOf( (string) ins.Operand ) : ins.MessageHandler.Arguments.Count + ins.MessageHandler.Locals.IndexOf( (string) ins.Operand ));
						break;
					case OpCode.PushLiteral:
						if (ins.Operand is CString)
							eins = ((uint) OpCode.PushLiteralString << 27) | (uint) ((CString) ins.Operand).Index;
						else {
							var i = ((CInteger) ins.Operand).Value;
							if (Math.Abs( i ) > 0x03FFFFFF)
								eins = ((uint) OpCode.PushLiteralInt << 27) | (uint) ((CInteger) ins.Operand).Index;
							else
								eins = ((uint) OpCode.PushLiteralIntInline << 27) | (uint) (i < 0 ? 1 << 26 : 0) | (uint) Math.Abs( i );
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
					handlers.Add( cls.DefaultHandler, 0 );
				cls.Handlers.ForEach( h => handlers.Add( h, 0 ) );
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

		void WriteIntegers( CIntegerPool pool ) {
			Write( (uint) pool.Count );

			foreach (var i in pool.Select( i => i.Value ))
				Write( i );
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

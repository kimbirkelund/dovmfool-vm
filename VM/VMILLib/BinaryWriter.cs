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
		List<int> integerMap, stringMap;
		Dictionary<MessageHandler, int> handlers;

		public BinaryWriter( Stream output ) {
			this.output = output;
		}

		public BinaryWriter( string output ) : this( new FileStream( output, FileMode.OpenOrCreate, FileAccess.Write ) ) { }

		public void Write( Assembly assembly ) {
			integerMap = new List<int>();
			stringMap = new List<int>();
			handlers = new Dictionary<MessageHandler, int>();

			WriteIntegers( assembly.Integers );
			WriteStrings( assembly.Strings );
		}

		void WriteHandlers( ClassList classes ) {
			MapHandlers( classes );

			Write( handlers.Count );

			WriteHandlers2( classes );
		}

		void WriteHandlers2( ClassList classes ) {
			foreach (var cls in classes) {
				if (cls.DefaultHandler != null)
					WriteHandler( cls.DefaultHandler );
				cls.Handlers.ForEach( h => WriteHandler( h ) );
				WriteHandlers2( cls.Classes );
			}
		}

		void WriteHandler( MessageHandler handler ) {
			handlers[handler] = pos;
			uint size = (uint) handler.Instructions.Count + 2;

			Write( (size << 4) | (uint) InternalObjectType.VMILMessageHandler );
			Write( (stringMap[handler.Name.Index] << 3) | (int) handler.Visibility );
			WriteInstructions( handler.Instructions );
		}

		void WriteInstructions( InstructionList inss ) {
			var labelMap = new Dictionary<string, int>();
			var trycatchMap = MapTryCatches( inss );
			var trycatchStack = new Stack<TryCatchRecord>();

			inss.OfType<Label>().ForEach( ( l, i ) => labelMap.Add( l.Name, i - labelMap.Count ) );

			int index = 0;
			foreach (var ins in inss.Where( i => !(i is Label) )) {
				uint eins = (uint) ins.OpCode << 27;
				switch (ins.OpCode) {
					case OpCode.StoreField:
					case OpCode.LoadField:
					case OpCode.StoreLocal:
					case OpCode.LoadLocal:
						eins |= (uint) ((CString) ins.Operand).Index;
						break;
					case OpCode.PushLiteral:
						eins |= (uint) (ins.Operand is CString ? stringMap[((CString) ins.Operand).Index] : integerMap[((CInteger) ins.Operand).Index]);
						break;
					case OpCode.Pop:
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
						break;
				}

				index++;
			}
		}

		Dictionary<int, TryCatchRecord> MapTryCatches( InstructionList inss ) {
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
				stringMap.Add( pos );
				Write( s.Length );
				var bytes = Encoding.Unicode.GetBytes( s );

				var remainder = bytes.Length % 4;
				if (remainder != 0) {
					var temp = new byte[bytes.Length + (4 - remainder)];
					Array.ConstrainedCopy( bytes, 0, temp, 0, bytes.Length );
					bytes = temp;
				}

				Write( bytes );
			}
		}

		void WriteIntegers( CIntegerPool pool ) {
			Write( (uint) pool.Count );

			foreach (var i in pool.Select( i => i.Value )) {
				integerMap.Add( pos );
				Write( i );
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

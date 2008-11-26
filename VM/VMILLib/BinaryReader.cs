using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet;

namespace VMILLib {
	public sealed class BinaryReader : IDisposable {
		bool isDisposed;
		Stream input;
		int pos;

		List<CString> strings;
		Dictionary<int, MessageHandlerBase> handlers;
		Dictionary<int, Class> classes;

		public BinaryReader( Stream input ) {
			this.input = input;
		}

		public BinaryReader( string inputFile )
			: this( new FileStream( inputFile, FileMode.Open, FileAccess.Read ) ) { }

		public Assembly Read() {
			strings = new List<CString>();
			handlers = new Dictionary<int, MessageHandlerBase>();
			classes = new Dictionary<int, Class>();

			var stringPool = ReadStrings();
			ReadMessageHandlers();
			var classList = ReadClasses();
			return new Assembly( stringPool, classList );
		}

		ClassList ReadClasses() {
			var classCount = ReadInt();

			classCount.ForEach( ReadClass );

			return new ClassList( classes.Values.Where( c => c.ParentClass == null ) );
		}

		void ReadClass() {
			var extendsCount = ReadInt();
			var fieldCount = ReadInt();
			var handlerCount = ReadInt();
			var classCount = ReadInt();
			var clsHeader = ReadUInt();
			var visibility = (VisibilityModifier) (0x00000003 & clsHeader);
			var name = (CString) strings[(int) clsHeader >> 2];

			var extends = new NameList( extendsCount.ForEach( () => (CString) strings[ReadInt()] ) );

			var defHandlerPointer = ReadInt();
			var defHandler = defHandlerPointer != 0 ? this.handlers[defHandlerPointer] : null;

			var fields = fieldCount.ForEach( i => "field_" + i );
			var handlers = new MessageHandlerList( handlerCount.ForEach( () => this.handlers[ReadInt()] ) );
			var classes = new ClassList( classCount.ForEach( () => this.classes[ReadInt()] ) );

			this.classes.Add( this.classes.Count, new Class( visibility, name, extends, fields, defHandler, handlers, classes ) );
		}

		void ReadMessageHandlers() {
			var handlerCount = ReadInt();

			handlerCount.ForEach( ReadMessageHandler );
		}

		void ReadMessageHandler() {
			var handlerHeader = ReadUInt();
			var visibility = (VisibilityModifier) (0x00000003 & handlerHeader);
			var name = visibility == VisibilityModifier.None ? null : strings[(int) handlerHeader >> 4];
			var isExternal = (4 & handlerHeader) != 0;
			var isEntryPoint = (8 & handlerHeader) != 0;

			var argCount = ReadInt();
			var args = argCount.ForEach( i => "argument_" + i );

			if (isExternal) {
				var externalName = strings[ReadInt()];
				this.handlers.Add( this.handlers.Count + 1, new ExternalMessageHandler( visibility, name, externalName, args ) );
			} else {
				var localCount = ReadInt();
				var instructionCount = ReadInt();

				var inss = ReadInstructions( instructionCount, argCount, localCount );

				this.handlers.Add( this.handlers.Count + 1, new VMILMessageHandler( visibility, name, args, (localCount - argCount).ForEach( i => "local_" + i ), inss, isEntryPoint ) );
			}
		}

		InstructionList ReadInstructions( int count, int argumentCount, int localCount ) {
			List<Instruction> inss = new List<Instruction>();
			Dictionary<int, Label> labels = new Dictionary<int, Label>();

			Func<int, Label> getLabel = offset => {
				if (!labels.ContainsKey( offset ))
					labels.Add( offset, new Label( "label" + offset ) );
				return labels[offset];
			};

			for (int i = 0; i < count; i++) {
				var ins = ReadUInt();
				var opcode = (OpCode) (ins >> 27);
				var operand = 0x07FFFFFF & ins;

				object actOperand = null;

				switch (opcode) {
					case OpCode.StoreField:
					case OpCode.LoadField:
						actOperand = "field_" + operand;
						break;
					case OpCode.StoreLocal:
					case OpCode.LoadLocal:
						actOperand = (operand >= argumentCount ? "local_" : "argument_") + operand;
						break;
					case OpCode.LoadArgument:
						if (operand == 0)
							opcode = OpCode.LoadThis;
						else
							actOperand = operand - 1;
						break;
					case OpCode.PushLiteralString:
						opcode = OpCode.PushLiteral;
						actOperand = strings[(int) operand];
						break;
					case OpCode.PushLiteralInt:
						opcode = OpCode.PushLiteral;
						actOperand = (int) (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
						break;
					case OpCode.PushLiteralIntExtend:
						opcode = OpCode.PushLiteral;
						var j1 = (int) inss[--i].Operand;
						var j2 = (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
#pragma warning disable 0675
						var j = (j1 << 16) | j2;
#pragma warning restore 0675
						inss[i] = new Instruction( OpCode.PushLiteral, j );
						continue;
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
						break;
					case OpCode.Jump:
					case OpCode.JumpIfTrue:
					case OpCode.JumpIfFalse:
						actOperand = getLabel( (int) ((((operand & 0x04000000) != 0 ? -1 : 1) * (operand & 0x03FFFFFF)) + i) );
						break;
					default:
						throw new ArgumentException( "Unexpected opcode : " + opcode );
				}

				inss.Add( new Instruction( opcode, actOperand ) );
			}

			labels.Keys.OrderByDescending( i => i ).ForEach( i => inss.Insert( (int) i, labels[i] ) );

			return new InstructionList( inss );
		}

		CStringPool ReadStrings() {
			int stringCount = ReadInt();
			CString[] strings = new CString[stringCount];

			for (int i = 0; i < stringCount; i++) {
				var pos = this.pos;
				var strSize = ReadInt();
				var words = new List<uint>();
				for (int j = 0; j < strSize / 2 + strSize % 2; j++)
					words.Add( ReadUInt() );

				strings[i] = new CString( i, Encoding.Unicode.GetString( words.ToByteStream().Take( strSize * 2 ).ToArray() ) );
				this.strings.Add( strings[i] );
			}

			return new CStringPool( strings );
		}

		byte[] byteArr4 = new byte[4];
		int ReadInt() {
			pos++;
			input.Read( byteArr4, 0, 4 );
			return byteArr4.ToIntStream().First();
		}

		uint ReadUInt() {
			pos++;
			input.Read( byteArr4, 0, 4 );
			return byteArr4.ToUIntStream().First();
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

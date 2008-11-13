﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VMILLib {
	public sealed class BinaryReader : IDisposable {
		bool isDisposed;
		Stream input;
		int pos;

		List<CString> strings;
		List<CInteger> integers;
		Dictionary<int, MessageHandler> handlers;
		Dictionary<int, Class> classes;

		public BinaryReader( Stream input ) {
			this.input = input;
		}

		public BinaryReader( string inputFile )
			: this( new FileStream( inputFile, FileMode.Open, FileAccess.Read ) ) { }

		public Assembly Read() {
			strings = new List<CString>();
			integers = new List<CInteger>();
			handlers = new Dictionary<int, MessageHandler>();
			classes = new Dictionary<int, Class>();

			var intergerPool = ReadIntegers();
			var stringPool = ReadStrings();
			ReadMessageHandlers();
			var classList = ReadClasses();
			return new Assembly( stringPool, intergerPool, classList );
		}

		ClassList ReadClasses() {
			var classCount = ReadInt();

			classCount.ForEach( ReadClass );

			return new ClassList( classes.Values.Where( c => c.ParentClass == null ) );
		}

		void ReadClass() {
			var pos = this.pos;

			var objHeader = ReadUInt();
			if ((objHeader & (uint) InternalObjectType.Class) == 0)
				throw new ArgumentException( "The specified input does constitute a valid VMB file." );

			var wordSize = objHeader >> 4;
			var handlerHeader = ReadUInt();
			ReadUInt(); // parent pointer
			var counts = ReadUInt();
			var fieldCount = (int) (counts >> 18);
			var handlerCount = (int) (0x00003FFF & (counts >> 4));
			var extendsCount = (int) (0x0000000F & counts);
			var classCount = (int) (wordSize - 6 - extendsCount - handlerCount * 2) / 2;
			var visibility = (VisibilityModifier) (0x00000003 & handlerHeader);
			var name = (CString) strings[(int) handlerHeader >> 3];

			var extends = new NameList( extendsCount.ForEach( () => (CString) strings[ReadInt()] ) );

			ReadUInt();
			var defHandlerPointer = ReadInt();
			var defHandler = defHandlerPointer != 0 ? this.handlers[defHandlerPointer] : null;

			var fields = fieldCount.ForEach( i => "field_" + i );
			var handlers = new MessageHandlerList( handlerCount.ForEach( () => { ReadUInt(); return this.handlers[ReadInt()]; } ) );
			var classes = new ClassList( classCount.ForEach( () => { ReadUInt(); return this.classes[ReadInt()]; } ) );

			this.classes.Add( pos, new Class( visibility, name, extends, fields, defHandler, handlers, classes ) );
		}

		void ReadMessageHandlers() {
			var handlerCount = ReadInt();

			handlerCount.ForEach( ReadMessageHandler );
		}

		void ReadMessageHandler() {
			var pos = this.pos;

			var objHeader = ReadUInt();
			if ((objHeader & (uint) InternalObjectType.VMILMessageHandler) == 0)
				throw new ArgumentException( "The specified input does constitute a valid VMB file." );

			var wordSize = objHeader >> 4;
			var handlerHeader = ReadUInt();
			var counts = ReadUInt();
			var argCount = (int) (counts >> 16);
			var localCount = (int) (0x0000FFFF & counts);
			var visibility = (VisibilityModifier) (0x00000003 & handlerHeader);
			var name = visibility == VisibilityModifier.None ? null : (CString) strings[(int) handlerHeader >> 3];
			var inss = ReadInstructions( (int) (wordSize - 3), argCount, localCount );

			this.handlers.Add( pos, new MessageHandler( visibility, name, argCount.ForEach( i => "argument_" + i ), (localCount - argCount).ForEach( i => "local_" + i ), inss ) );
		}

		InstructionList ReadInstructions( int count, int argumentCount, int localCount ) {
			List<Instruction> inss = new List<Instruction>();
			Dictionary<uint, Label> labels = new Dictionary<uint, Label>();

			Func<uint, Label> getLabel = offset => {
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
					case OpCode.PushLiteralString:
						opcode = OpCode.PushLiteral;
						actOperand = strings[(int) operand];
						break;
					case OpCode.PushLiteralInt:
						opcode = OpCode.PushLiteral;
						actOperand = integers[(int) operand];
						break;
					case OpCode.PushLiteralIntInline:
						opcode = OpCode.PushLiteral;
						actOperand = (operand & 0x03FFFFFF) * ((operand & 0x04000000) != 0 ? -1 : 1);
						break;
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
						actOperand = getLabel( (uint) (operand + i) );
						break;
					default:
						throw new ArgumentException( "Unexpected opcode : " + opcode );
				}

				inss.Add( new Instruction( opcode, actOperand ) );
			}

			labels.Keys.OrderByDescending( i => i ).ForEach( i => inss.Insert( (int) i, labels[i] ) );

			return new InstructionList( inss );
		}

		CIntegerPool ReadIntegers() {
			int intCount = ReadInt();
			CInteger[] ints = new CInteger[intCount];

			for (int i = 0; i < intCount; i++) {
				var pos = this.pos;
				ints[i] = new CInteger( i, ReadInt() );
				this.integers.Add( ints[i] );
			}

			return new CIntegerPool( ints );
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
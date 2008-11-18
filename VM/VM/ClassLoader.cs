using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet;
using VMILLib;
using vmo = VM.VMObjects;

namespace VM {
	class ClassLoader {
		Stream input;
		Dictionary<int, int> constantIndexMap = new Dictionary<int, int>();
		Dictionary<int, Handle<VMObjects.VMILMessageHandler>> messageHandlerIndexMap = new Dictionary<int, Handle<VMObjects.VMILMessageHandler>>();
		Dictionary<int, Handle<VMObjects.Class>> classIndexMap = new Dictionary<int, Handle<VMObjects.Class>>();
		Handle<vmo.MessageHandlerBase> entrypoint;

		public ClassLoader( Stream input ) {
			this.input = input;
		}

		public ClassLoader( string fileName ) : this( new FileStream( fileName, FileMode.Open, FileAccess.Read ) ) { }

		/// <summary>
		/// Reads the classes stored in input into the virtual machine.
		/// </summary>
		/// <returns>The entrypoint specified in the input if any.</returns>
		public Handle<vmo.MessageHandlerBase> Read() {
			ReadStrings();
			ReadMessageHandlers();
			ReadClasses();

			return entrypoint;
		}

		void ReadStrings() {
			int stringCount = ReadW();

			for (int i = 0; i < stringCount; i++)
				ReadString( i );
		}

		void ReadString( int index ) {
			int stringLength = ReadW();
			var wordCount = stringLength / 2 + stringLength % 2;

			var str = VirtualMachine.MemoryManager.Allocate<VMObjects.String>( wordCount + vmo.String.LENGTH_OFFSET );
			str[1] = stringLength;
			for (int i = 0; i < wordCount; i++)
				str[2] = ReadW();

			constantIndexMap.Add( index, VirtualMachine.ConstantPool.RegisterString( str.ToHandle() ) );
		}

		void ReadMessageHandlers() {
			int handlerCount = ReadW();

			for (int i = 0; i < handlerCount; i++)
				ReadHandler( i );
		}

		void ReadHandler( int index ) {
			int argCount = ReadW();
			int localCount = ReadW();
			int instructionCount = ReadW();
			var handlerHeader = ReadW();

			var handler = vmo.VMILMessageHandler.New( instructionCount );

			handler[vmo.MessageHandlerBase.HEADER_OFFSET] = handlerHeader != (int) VisibilityModifier.None ? ((constantIndexMap[handlerHeader >> 4]) << 4) | (handlerHeader & 15) : handlerHeader;
			if ((handlerHeader & vmo.MessageHandlerBase.IS_INTERNAL_MASK) != 0)
				entrypoint = ((vmo.MessageHandlerBase) handler).ToHandle();

			for (int i = 0; i < instructionCount; i++) {
				var ins = ReadW();
				if ((OpCode) (ins >> 27) == OpCode.PushLiteralString)
					ins = ((int) OpCode.PushLiteralString << 27) | constantIndexMap[ins & 0x07FFFFFF];
				handler[i + vmo.VMILMessageHandler.INSTRUCTIONS_OFFSET] = ReadW();
			}

			messageHandlerIndexMap.Add( index + 1, handler.ToHandle() );
		}

		void ReadClasses() {
			int classCount = ReadW();

			for (int i = 0; i < classCount; i++)
				ReadClass( i );
		}

		void ReadClass( int index ) {
			int superClassCount = ReadW();
			int fieldCount = ReadW();
			int handlerCount = ReadW();
			int innerClassCount = ReadW();
			var clsHeader = ReadW();

			var cls = VirtualMachine.MemoryManager.Allocate<VMObjects.Class>( vmo.Class.CalculateSize( superClassCount, handlerCount, innerClassCount ) );

			cls[vmo.Class.HEADER_OFFSET] = (constantIndexMap[cls >> 2] << 2) | (clsHeader & 3);

			cls[vmo.Class.COUNTS_OFFSET] = (fieldCount << vmo.Class.COUNTS_FIELDS_RSHIFT) | ((handlerCount << vmo.Class.COUNTS_HANDLERS_RSHIFT) & vmo.Class.COUNTS_HANDLERS_MASK) | (vmo.Class.COUNTS_SUPERCLASSES_MASK & superClassCount);

			superClassCount.ForEach( i => cls[vmo.Class.SUPERCLASSES_OFFSET] = constantIndexMap[ReadW()] );

			var offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount;
			int defaultHandlerIndex = ReadW();
			if (defaultHandlerIndex != 0) {
				var defaultHandler = messageHandlerIndexMap[defaultHandlerIndex];
				cls[offset] = defaultHandler.Value[vmo.MessageHandlerBase.HEADER_OFFSET];
				cls[offset + 1] = defaultHandler;
			}

			offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount + 2;
			handlerCount.ForEach( i => {
				var handler = messageHandlerIndexMap[ReadW()];
				cls[offset + 2 * i] = handler[vmo.MessageHandlerBase.HEADER_OFFSET];
				cls[offset + i + 1] = handler;
				handler[vmo.MessageHandlerBase.CLASS_POINTER_OFFSET] = cls;
			} );

			offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount + 2 + handlerCount * 2;
			innerClassCount.ForEach( i => {
				var innerCls = classIndexMap[ReadW()];
				cls[offset + 2 * i] = innerCls[vmo.Class.HEADER_OFFSET];
				cls[offset + 2 * i + 1] = innerCls;
				innerCls[vmo.Class.PARENT_CLASS_OFFSET] = cls;
			} );
		}

		byte[] byteArr4 = new byte[4];
		Word ReadW() {
			input.Read( byteArr4, 0, 4 );
			return byteArr4.ToUIntStream().First();
		}
	}
}

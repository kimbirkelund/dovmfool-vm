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
		Dictionary<int, Handle<VMObjects.MessageHandlerBase>> messageHandlerIndexMap = new Dictionary<int, Handle<VMObjects.MessageHandlerBase>>();
		Dictionary<int, Handle<VMObjects.Class>> classIndexMap = new Dictionary<int, Handle<VMObjects.Class>>();
		Handle<vmo.VMILMessageHandler> entrypoint;

		public ClassLoader( Stream input ) {
			this.input = input;
		}

		public ClassLoader( string fileName ) : this( new FileStream( fileName, FileMode.Open, FileAccess.Read ) ) { }

		/// <summary>
		/// Reads the classes stored in input into the virtual machine.
		/// </summary>
		/// <returns>The entrypoint specified in the input if any.</returns>
		public Handle<vmo.VMILMessageHandler> Read() {
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

			var str = VMObjects.String.CreateInstance( stringLength );
			str[vmo.String.LENGTH_OFFSET] = stringLength;
			for (int i = 0; i < wordCount; i++)
				str[vmo.String.FIRST_CHAR_OFFSET + i] = ReadW();

			constantIndexMap.Add( index, VirtualMachine.ConstantPool.RegisterString( str.ToHandle() ) );
		}

		void ReadMessageHandlers() {
			int handlerCount = ReadW();

			for (int i = 0; i < handlerCount; i++)
				ReadHandler( i );
		}

		void ReadHandler( int index ) {
			var handlerHeader = ReadW();
			int argCount = ReadW();
			vmo.MessageHandlerBase handlerBase;

			if ((handlerHeader & vmo.MessageHandlerBase.IS_INTERNAL_MASK) != 0) {
				var externalName = constantIndexMap[ReadW()];

				var handler = vmo.DelegateMessageHandler.CreateInstance();
				handlerBase = handler;

				handler[vmo.MessageHandlerBase.HEADER_OFFSET] = handlerHeader != (int) VisibilityModifier.None ? ((constantIndexMap[handlerHeader >> 4]) << 4) | (handlerHeader & 15) : handlerHeader;
				handler[vmo.DelegateMessageHandler.EXTERNAL_NAME_OFFSET] = VirtualMachine.ConstantPool.GetString( externalName );
				handler[vmo.DelegateMessageHandler.ARGUMENT_COUNT_OFFSET] = argCount;
			} else {
				int localCount = ReadW();
				int instructionCount = ReadW();

				var handler = vmo.VMILMessageHandler.CreateInstance( instructionCount );
				handlerBase = handler;

				handler[vmo.MessageHandlerBase.HEADER_OFFSET] = handlerHeader != (int) VisibilityModifier.None ? ((constantIndexMap[handlerHeader >> 4]) << 4) | (handlerHeader & 15) : handlerHeader;
				if ((handlerHeader & vmo.MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0)
					entrypoint = handler.ToHandle();

				for (int i = 0; i < instructionCount; i++) {
					var ins = ReadW();
					if ((OpCode) (ins >> 27) == OpCode.PushLiteralString)
						ins = ((int) OpCode.PushLiteralString << 27) | constantIndexMap[ins & 0x07FFFFFF];
					handler[i + vmo.VMILMessageHandler.INSTRUCTIONS_OFFSET] = ins;
				}
			}

			messageHandlerIndexMap.Add( index + 1, handlerBase.ToHandle() );
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

			var cls = vmo.Class.CreateInstance( superClassCount, handlerCount, innerClassCount );

			cls[vmo.Class.HEADER_OFFSET] = (constantIndexMap[clsHeader >> 2] << 2) | (clsHeader & 3);

			cls[vmo.Class.COUNTS_OFFSET] = (fieldCount << vmo.Class.COUNTS_FIELDS_RSHIFT) | ((handlerCount << vmo.Class.COUNTS_HANDLERS_RSHIFT) & vmo.Class.COUNTS_HANDLERS_MASK) | (vmo.Class.COUNTS_SUPERCLASSES_MASK & superClassCount);

			superClassCount.ForEach( i => cls[vmo.Class.SUPERCLASSES_OFFSET + i] = constantIndexMap[ReadW()] );

			var offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount;
			int defaultHandlerIndex = ReadW();
			if (defaultHandlerIndex != 0) {
				var defaultHandler = messageHandlerIndexMap[defaultHandlerIndex];
				cls[offset] = defaultHandler;
			} else
				cls[offset] = 0;

			offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount + 1;
			handlerCount.ForEach( i => {
				var handler = messageHandlerIndexMap[ReadW()];
				cls[offset + 2 * i] = handler[vmo.MessageHandlerBase.HEADER_OFFSET];
				cls[offset + 2 * i + 1] = handler;
				handler[vmo.MessageHandlerBase.CLASS_POINTER_OFFSET] = cls;
			} );

			offset = vmo.Class.SUPERCLASSES_OFFSET + superClassCount + 1 + handlerCount * 2;
			innerClassCount.ForEach( i => {
				var innerCls = classIndexMap[ReadW()];
				cls[offset + 2 * i] = innerCls[vmo.Class.HEADER_OFFSET];
				cls[offset + 2 * i + 1] = innerCls;
				innerCls[vmo.Class.PARENT_CLASS_OFFSET] = cls;
			} );

			VirtualMachine.RegisterClass( cls.ToHandle() );
		}

		byte[] byteArr4 = new byte[4];
		Word ReadW() {
			input.Read( byteArr4, 0, 4 );
			var w = byteArr4.ToUIntStream().First();

			//#if DEBUG
			//            var frame = new System.Diagnostics.StackTrace().GetFrame( 1 );
			//            System.Diagnostics.Trace.TraceInformation( w.ToString( "X8" ) + ": " + frame.GetMethod().Name );
			//#endif

			return w;
		}
	}
}

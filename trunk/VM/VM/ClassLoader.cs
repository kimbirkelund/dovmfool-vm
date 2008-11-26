using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Sekhmet;
using VMILLib;
using vmo = VM.VMObjects;
using VM.VMObjects;

namespace VM {
	class ClassLoader : IDisposable {
		bool disposed;
		Stream input;
		Dictionary<int, int> constantIndexMap = new Dictionary<int, int>();
		Dictionary<int, Handle<VMObjects.MessageHandlerBase>> messageHandlerIndexMap = new Dictionary<int, Handle<VMObjects.MessageHandlerBase>>();
		Dictionary<int, Handle<VMObjects.Class>> classIndexMap = new Dictionary<int, Handle<VMObjects.Class>>();
		Handle<vmo.VMILMessageHandler> entrypoint;

		public ClassLoader( Stream input ) {
			this.input = input;
		}

		public ClassLoader( string fileName ) {
			if (Path.GetExtension( fileName ) == ".vmil") {
				var memStream = new MemoryStream();
				using (var reader = new SourceReader( fileName )) {
					var writer = new VMILLib.BinaryWriter( memStream );
					writer.Write( reader.Read() );
				}
				memStream.Position = 0;
				input = memStream;
			} else if (Path.GetExtension( fileName ) == ".vmb")
				input = new FileStream( fileName, FileMode.Open, FileAccess.Read );
			else
				throw new ArgumentException( "Specified input file is not in a known format." );
		}

		/// <summary>
		/// Reads the classes stored in input into the virtual machine.
		/// </summary>
		/// <returns>The entrypoint specified in the input if any.</returns>
		public Handle<vmo.VMILMessageHandler> Read() {
			ReadStrings();
			ReadMessageHandlers();
			ReadClasses();
			foreach (var item in classIndexMap.Values) {
				if (item.ParentClass() == null)
					VirtualMachine.RegisterClass( item );

			}
			//classIndexMap.Values.Where( c => c.ParentClass() == null ).ForEach( c => VirtualMachine.RegisterClass( c ) );

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

			constantIndexMap.Add( index, VirtualMachine.ConstantPool.RegisterString( str ) );
		}

		void ReadMessageHandlers() {
			int handlerCount = ReadW();

			for (int i = 0; i < handlerCount; i++)
				ReadHandler( i );
		}

		void ReadHandler( int index ) {
			var handlerHeader = ReadW();
			int argCount = ReadW();
			Handle<vmo.MessageHandlerBase> handlerBase;

			if ((handlerHeader & vmo.MessageHandlerBase.IS_INTERNAL_MASK) != 0) {
				var externalName = constantIndexMap[ReadW()];

				var handler = vmo.DelegateMessageHandler.CreateInstance();
				handlerBase = handler.To<vmo.MessageHandlerBase>();

				handler[vmo.MessageHandlerBase.HEADER_OFFSET] = handlerHeader != (int) VisibilityModifier.None ? ((constantIndexMap[handlerHeader >> 4]) << 4) | (handlerHeader & 15) : handlerHeader;
				handler[vmo.DelegateMessageHandler.EXTERNAL_NAME_OFFSET] = VirtualMachine.ConstantPool.GetString( externalName );
				handler[vmo.DelegateMessageHandler.ARGUMENT_COUNT_OFFSET] = argCount;
			} else {
				int localCount = ReadW();
				int instructionCount = ReadW();

				var handler = vmo.VMILMessageHandler.CreateInstance( instructionCount );
				handlerBase = handler.To<vmo.MessageHandlerBase>();

				handler[vmo.MessageHandlerBase.HEADER_OFFSET] = handlerHeader != (int) VisibilityModifier.None ? ((constantIndexMap[handlerHeader >> 4]) << 4) | (handlerHeader & 15) : handlerHeader;
				if ((handlerHeader & vmo.MessageHandlerBase.IS_ENTRYPOINT_MASK) != 0)
					entrypoint = handler;
				handler[vmo.VMILMessageHandler.COUNTS_OFFSET] = (argCount << vmo.VMILMessageHandler.ARGUMENT_COUNT_RSHIFT) | (localCount & vmo.VMILMessageHandler.LOCAL_COUNT_MASK);

				for (int i = 0; i < instructionCount; i++) {
					var ins = ReadW();
					if ((OpCode) (ins >> 27) == OpCode.PushLiteralString)
						ins = ((int) OpCode.PushLiteralString << 27) | constantIndexMap[ins & 0x07FFFFFF];
					handler[i + vmo.VMILMessageHandler.INSTRUCTIONS_OFFSET] = ins;
				}
			}

			messageHandlerIndexMap.Add( index + 1, handlerBase );
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

			var isObject = cls.Name() == VirtualMachine.ConstantPool.RegisterString( "Object" );

			cls[vmo.Class.COUNTS_OFFSET] = (fieldCount << vmo.Class.COUNTS_FIELDS_RSHIFT) | ((handlerCount << vmo.Class.COUNTS_HANDLERS_RSHIFT) & vmo.Class.COUNTS_HANDLERS_MASK) | (vmo.Class.COUNTS_SUPERCLASSES_MASK & (isObject ? 0 : Math.Max( 1, superClassCount )));
			cls[vmo.Class.LINEARIZATION_OFFSET] = 0;
			cls[vmo.Class.INSTANCE_SIZE_OFFSET] = -1;

			if (superClassCount == 0 && !isObject) {
				cls[vmo.Class.SUPERCLASSES_OFFSET + 1] = VirtualMachine.ConstantPool.RegisterString( "Object" );
				superClassCount++;
			} else
				superClassCount.ForEach( i => { cls[vmo.Class.SUPERCLASSES_OFFSET + i] = constantIndexMap[ReadW()]; } );

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

			classIndexMap.Add( index, cls );
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

		public void Dispose() {
			lock (this) {
				if (disposed)
					return;
			}
			disposed = true;

			if (input != null) {
				input.Dispose();
				input = null;
			}
		}
	}
}

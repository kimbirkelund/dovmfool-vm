using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Handles {
	public class Class : InternalObjectBase {
		public const uint VISIBILITY_MASK = 0x00000003;

		public const uint NAME_MASK = 0xFFFFFFFC;
		public const int NAME_RSHIFT = 2;

		public const uint DEFAULT_HANDLER_MASK = 0xFFFF0000;
		public const int DEFAULT_HANDLER_RSHIFT = 16;

		public const uint FIRST_CLASS_MASK = 0x0000FFFF;

		public new const uint TypeId = 4;

		public VisibilityModifier Visibility { get { return (VisibilityModifier) (this[1] & VISIBILITY_MASK); } }
		public uint InstanceSize { get { return this[2]; } }

		public String Name {
			get {
				var nameIndex = (this[1] & NAME_MASK) >> NAME_RSHIFT;
				return this.VirtualMachine.ConstantPool.GetConstant<String>( nameIndex );
			}
		}

		public MessageHandlerBase DefaultHandler {
			get {
				var defHandOffset = (this[3] & DEFAULT_HANDLER_MASK) >> DEFAULT_HANDLER_RSHIFT;
				var defHandPointer = this[3 + defHandOffset];
				throw new NotImplementedException();
			}
		}

		internal Class( MemoryManagerBase memoryManager, uint start ) : base( memoryManager, start ) { }

		public MessageHandlerBase GetHandler( String messageName ) {
			throw new NotImplementedException();
		}

		public Class GetClass( String className ) {
			throw new NotImplementedException();
		}

		public IEnumerable<MessageHandlerBase> GetHandlers() {
			var firstHandOffset = ((this[3] & DEFAULT_HANDLER_MASK) >> DEFAULT_HANDLER_RSHIFT) + 2;
			var lastHandOffset = this[3] & FIRST_CLASS_MASK;

			for (uint i = 0; i < lastHandOffset; i += 2)
				throw new NotImplementedException();

			throw new NotImplementedException();
		}

		public IEnumerable<Class> GetClasses() {
			var firstClassOffset = this[3] & FIRST_CLASS_MASK + start;
			var lastClassOffset = Size;

			for (uint i = 0; i < lastClassOffset; i += 2)
				throw new NotImplementedException();

			throw new NotImplementedException();
		}
	}
}

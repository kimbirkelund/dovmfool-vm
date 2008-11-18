using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct ObjectBase : IVMObject {
		#region Constants
		public const int OBJECT_HEADER_OFFSET = 0;
		public static readonly Word OBJECT_TYPE_MASK = 0x0000000F;
		public const int OBJECT_SIZE_RSHIFT = 4;
		#endregion

		#region Properties
		public TypeId TypeId { get { return (TypeId) (int) (this[0] & OBJECT_TYPE_MASK); } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}
		#endregion

		#region Casts
		public static implicit operator int( ObjectBase cls ) {
			return cls.start;
		}

		public static implicit operator ObjectBase( int cls ) {
			return new ObjectBase { start = cls };
		}

		public static explicit operator AppObject( ObjectBase obj ) {
			return new AppObject { Start = obj.start };
		}

		public static explicit operator AppObjectSet( ObjectBase obj ) {
			return new AppObjectSet { Start = obj.start };
		}

		public static explicit operator Class( ObjectBase obj ) {
			return new Class{ Start = obj.start };

		}

		public static explicit operator ClassManager( ObjectBase obj ) {
			return new ClassManager{ Start = obj.start };

		}

		public static explicit operator MessageHandlerBase( ObjectBase obj ) {
			return new MessageHandlerBase{ Start = obj.start };

		}

		public static explicit operator VMILMessageHandler( ObjectBase obj ) {
			return new VMILMessageHandler { Start = obj.start };

		}

		public static explicit operator DelegateMessageHandler( ObjectBase obj ) {
			return new DelegateMessageHandler { Start = obj.start };

		}

		public static explicit operator String( ObjectBase obj ) {
			return new String { Start = obj.start };

		}

		public static explicit operator Integer( ObjectBase obj ) {
			return new Integer { Start = obj.start };

		}

		public static implicit operator ObjectBase( AppObject obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( AppObjectSet obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( Class obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( ClassManager obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( MessageHandlerBase obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( VMILMessageHandler obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( DelegateMessageHandler obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( String obj ) {
			return new ObjectBase { start = obj.Start };
		}

		public static implicit operator ObjectBase( Integer obj ) {
			return new ObjectBase { start = obj.Start };
		}
		#endregion
	}
}

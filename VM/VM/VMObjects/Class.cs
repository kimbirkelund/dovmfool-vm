using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct Class:IVMObject {
		public const int CLASS_HEADER_OFFSET = 1;
		public const int CLASS_PARENT_CLASS_OFFSET = 2;
		public const int CLASS_COUNTS_OFFSET = 3;
		public const int CLASS_SUPERCLASSES_OFFSET = 4;

		public static readonly Word VISIBILITY_MASK = 0x00000003;

		public static readonly Word NAME_MASK = 0xFFFFFFFC;
		public const int NAME_RSHIFT = 2;

		public const int COUNTS_FIELDS_RSHIFT = 18;
		public static readonly Word COUNTS_HANDLERS_MASK = 0x0003FFF0;
		public const int COUNTS_HANDLERS_RSHIFT = 4;
		public static readonly Word COUNTS_EXTENDS_MASK = 0x0000000F;

		public const int TypeId = 4;

		int start;
		public int Start {
			get { return start; }
			set { start = value; }
		}

		public static implicit operator int( Class cls ) {
			return cls.start;
		}

		public static implicit operator Class( int cls ) {
			return new Class { start = cls };
		}
	}

	public static class ExtClass {
		public static VisibilityModifier Visibility( this Class cls ) {
			return (VisibilityModifier) (cls.Get( Class.CLASS_HEADER_OFFSET ) & Class.VISIBILITY_MASK);
		}

		public static int FieldCount( this Class cls ) {
			return cls.Get( Class.CLASS_COUNTS_OFFSET ) >> Class.COUNTS_FIELDS_RSHIFT;
		}

		public static int HandlerCount( this Class cls ) {
			return (cls.Get( Class.CLASS_COUNTS_OFFSET ) & Class.COUNTS_HANDLERS_MASK) >> Class.COUNTS_HANDLERS_RSHIFT;
		}

		public static int SuperClassCount( this Class cls ) {
			return cls.Get( Class.CLASS_COUNTS_OFFSET ) & Class.COUNTS_EXTENDS_MASK;
		}

		public static int ClassCount( this Class cls ) {
			return cls.Size() - Class.CLASS_SUPERCLASSES_OFFSET - cls.SuperClassCount() - 1 - cls.HandlerCount();
		}

		public static int InstanceSize( this Class cls ) {
			return cls.FieldCount() + AppObject.FIELDS_OFFSET;
		}

		public static String Name( this Class cls ) {
			return (String) ((cls.Get( Class.CLASS_HEADER_OFFSET ) & Class.NAME_MASK) >> Class.NAME_RSHIFT);
		}

		public static IEnumerable<String> SuperClasses( this Class cls ) {
			for (var i = Class.CLASS_SUPERCLASSES_OFFSET; i < Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount(); i++)
				yield return (String) cls.Get( i );
		}

		public static MessageHandlerBase DefaultHandler( this Class cls ) {
			return (MessageHandlerBase) cls.Get( Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() );
		}

		public static IEnumerable<MessageHandlerBase> MessageHandlers( this Class cls ) {
			var firstHandler = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + 1;
			var handlers = cls.HandlerCount() * 2;
			for (var i = 1; i < handlers; i += 2)
				yield return (MessageHandlerBase) cls.Get( firstHandler + i );
		}

		public static MessageHandlerBase GetMessageHandler( this Class cls, AppObject caller, String messageName ) {
			var firstHandler = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + 1;
			var handlers = cls.HandlerCount() * 2;

			for (var i = 0; i < handlers; i += 2) {
				var header = cls.Get( firstHandler + i );
				var visibility = (VisibilityModifier) (header & MessageHandlerBase.VISIBILITY_MASK);
				var name = (String) (header >> MessageHandlerBase.NAME_RSHIFT);

				if (visibility == VisibilityModifier.Private)
					continue;
				if (messageName.EqualTo( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( cls ))
					continue;

				return (MessageHandlerBase) cls.Get( firstHandler + i + 1 );
			}

			return cls.DefaultHandler();
		}

		public static MessageHandlerBase GetClass( this Class cls, Class referencer, String className ) {
			var firstClass = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + cls.HandlerCount() + 1;
			var classes = cls.ClassCount() * 2;

			for (var i = 0; i < classes; i += 2) {
				var header = cls.Get( firstClass + i );
				var visibility = (VisibilityModifier) (header & Class.VISIBILITY_MASK);
				var name = (String) (header >> Class.NAME_RSHIFT);

				if (visibility == VisibilityModifier.Private)
					continue;
				if (className.EqualTo( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !referencer.Extends( cls ))
					continue;

				return (MessageHandlerBase) cls.Get( firstClass + i + 1 );
			}

			return cls.DefaultHandler();
		}

		public static bool Extends( this Class cls, Class testSuperCls ) {
			foreach (var superClsName in cls.SuperClasses()) {
				var superCls = VirtualMachine.ResolveClass( superClsName );
				if (superCls == testSuperCls)
					return true;
				else
					superCls.Extends( testSuperCls );
			}

			return false;
		}
	}
}

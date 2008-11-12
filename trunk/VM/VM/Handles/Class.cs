using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.Handles {
	public struct Class {
		public const uint CLASS_HEADER_OFFSET = 1;
		public const uint CLASS_PARENT_CLASS_OFFSET = 2;
		public const uint CLASS_COUNTS_OFFSET = 3;
		public const uint CLASS_SUPERCLASSES_OFFSET = 4;

		public const uint VISIBILITY_MASK = 0x00000003;

		public const uint NAME_MASK = 0xFFFFFFFC;
		public const int NAME_RSHIFT = 2;

		public const int COUNTS_FIELDS_RSHIFT = 18;
		public const uint COUNTS_HANDLERS_MASK = 0x0003FFF0;
		public const int COUNTS_HANDLERS_RSHIFT = 4;
		public const uint COUNTS_EXTENDS_MASK = 0x0000000F;

		public const uint TypeId = 4;

		uint start;

		public static implicit operator uint( Class cls ) {
			return cls.start;
		}

		public static implicit operator Class( uint cls ) {
			return new Class { start = cls };
		}
	}

	public static class ExtClass {
		public static VisibilityModifier Visibility( this Class cls ) {
			return (VisibilityModifier) (cls.Get( Class.CLASS_HEADER_OFFSET ) & Class.VISIBILITY_MASK);
		}

		public static uint FieldCount( this Class cls ) {
			return cls.Get( Class.CLASS_COUNTS_OFFSET ) >> Class.COUNTS_FIELDS_RSHIFT;
		}

		public static uint HandlerCount( this Class cls ) {
			return (cls.Get( Class.CLASS_COUNTS_OFFSET ) & Class.COUNTS_HANDLERS_MASK) >> Class.COUNTS_HANDLERS_RSHIFT;
		}

		public static uint SuperClassCount( this Class cls ) {
			return cls.Get( Class.CLASS_COUNTS_OFFSET ) & Class.COUNTS_EXTENDS_MASK;
		}

		public static uint ClassCount( this Class cls ) {
			return cls.Size() - Class.CLASS_SUPERCLASSES_OFFSET - cls.SuperClassCount() - 1 - cls.HandlerCount();
		}

		public static uint InstanceSize( this Class cls ) {
			return cls.FieldCount() + AppObject.FIELDS_OFFSET;
		}

		public static uint Name( this Class cls ) {
			return (cls.Get( Class.CLASS_HEADER_OFFSET ) & Class.NAME_MASK) >> Class.NAME_RSHIFT;
		}

		public static IEnumerable<String> SuperClasses( this Class cls ) {
			for (uint i = Class.CLASS_SUPERCLASSES_OFFSET; i < Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount(); i++)
				yield return cls.Get( i );
		}

		public static uint DefaultHandler( this Class cls ) {
			return cls.Get( Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() );
		}

		public static IEnumerable<uint> MessageHandlers( this Class cls ) {
			var firstHandler = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + 1;
			var handlers = cls.HandlerCount() * 2;
			for (uint i = 1; i < handlers; i += 2)
				yield return cls.Get( firstHandler + i );
		}

		public static uint GetMessageHandler( this Class cls, AppObject caller, String messageName ) {
			var firstHandler = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + 1;
			var handlers = cls.HandlerCount() * 2;

			for (uint i = 0; i < handlers; i += 2) {
				var header = cls.Get( (uint) (firstHandler + i) );
				var visibility = (VisibilityModifier) (header & MessageHandlerBase.VISIBILITY_MASK);
				var name = header >> MessageHandlerBase.NAME_RSHIFT;

				if (visibility == VisibilityModifier.Private)
					continue;
				if (messageName.Compare( name ) != 0)
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( cls ))
					continue;

				return cls.Get( firstHandler + i + 1 );
			}

			return cls.DefaultHandler();
		}

		public static uint GetClass( this Class cls, Class referencer, String className ) {
			var firstClass = Class.CLASS_SUPERCLASSES_OFFSET + cls.SuperClassCount() + cls.HandlerCount() + 1;
			var classes = cls.ClassCount() * 2;

			for (uint i = 0; i < classes; i += 2) {
				var header = cls.Get( firstClass + i );
				var visibility = (VisibilityModifier) (header & Class.VISIBILITY_MASK);
				var name = header >> Class.NAME_RSHIFT;

				if (visibility == VisibilityModifier.Private)
					continue;
				if (className.Compare( name ) != 0)
					continue;
				if (visibility == VisibilityModifier.Protected && !referencer.Extends( cls ))
					continue;

				return cls.Get( firstClass + i + 1 );
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

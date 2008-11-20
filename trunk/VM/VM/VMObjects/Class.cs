﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	public struct Class : IVMObject<Class> {
		#region Constants
		public const int HEADER_OFFSET = 1;
		public const int PARENT_CLASS_OFFSET = 2;
		public const int COUNTS_OFFSET = 3;
		public const int SUPERCLASSES_OFFSET = 4;

		public static readonly Word VISIBILITY_MASK = 0x00000003;

		public const int NAME_RSHIFT = 2;

		public const int COUNTS_FIELDS_RSHIFT = 18;
		public static readonly Word COUNTS_HANDLERS_MASK = 0x0003FFF0;
		public const int COUNTS_HANDLERS_RSHIFT = 4;
		public static readonly Word COUNTS_SUPERCLASSES_MASK = 0x0000000F;
		#endregion

		#region Properties
		public bool IsNull { get { return start == 0; } }
		public TypeId TypeId { get { return VMILLib.TypeId.Class; } }
		public int Size { get { return this[ObjectBase.OBJECT_HEADER_OFFSET] >> ObjectBase.OBJECT_SIZE_RSHIFT; } }

		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}

		int start;
		public int Start {
			get { return start; }
		}

		public VisibilityModifier Visibility { get { return (VisibilityModifier) (this[Class.HEADER_OFFSET] & Class.VISIBILITY_MASK); } }
		public int FieldCount { get { return this[Class.COUNTS_OFFSET] >> Class.COUNTS_FIELDS_RSHIFT; } }
		public int HandlerCount { get { return (this[Class.COUNTS_OFFSET] & Class.COUNTS_HANDLERS_MASK) >> Class.COUNTS_HANDLERS_RSHIFT; } }
		public int SuperClassCount { get { return this[Class.COUNTS_OFFSET] & Class.COUNTS_SUPERCLASSES_MASK; } }
		public int InnerClassCount { get { return (this.Size - Class.SUPERCLASSES_OFFSET - this.SuperClassCount - 1 - this.HandlerCount * 2) / 2; } }
		public int InstanceSize { get { return this.FieldCount + AppObject.FIELDS_OFFSET; } }
		public String Name { get { return VirtualMachine.ConstantPool.GetString( this[Class.HEADER_OFFSET] >> Class.NAME_RSHIFT ); } }
		public MessageHandlerBase DefaultHandler { get { return (MessageHandlerBase) this[Class.SUPERCLASSES_OFFSET + this.SuperClassCount]; } }
		public Class ParentClass { get { return (Class) this[Class.PARENT_CLASS_OFFSET]; } }

		public IEnumerable<String> SuperClasses {
			get {
				for (var i = Class.SUPERCLASSES_OFFSET; i < Class.SUPERCLASSES_OFFSET + this.SuperClassCount; i++)
					yield return VirtualMachine.ConstantPool.GetString( this[i] );
			}
		}

		public IEnumerable<MessageHandlerBase> MessageHandlers {
			get {
				var firstHandler = Class.SUPERCLASSES_OFFSET + this.SuperClassCount + 1;
				var handlers = this.HandlerCount * 2;
				for (var i = 1; i < handlers; i += 2)
					yield return (MessageHandlerBase) this[firstHandler + i];
			}
		}
		#endregion

		#region Cons
		public Class( int start ) {
			this.start = start;
		}

		public Class New( int startPosition ) {
			return new Class( startPosition );
		}
		#endregion

		#region Static methods
		public static Class CreateInstance( int superClassCount, int handlerCount, int innerClassCount ) {
			return VirtualMachine.MemoryManager.Allocate<Class>( SUPERCLASSES_OFFSET - 1 + superClassCount + 1 + 2 * handlerCount + 2 * innerClassCount );
		}
		#endregion

		#region Casts
		public static implicit operator int( Class cls ) {
			return cls.start;
		}

		public static explicit operator Class( int cls ) {
			return new Class { start = cls };
		}
		#endregion

		#region Instance method
		public MessageHandlerBase ResolveMessageHandler( AppObject caller, String messageName ) {
			var firstHandler = Class.SUPERCLASSES_OFFSET + this.SuperClassCount + 1;
			var handlers = this.HandlerCount * 2;

			for (var i = 0; i < handlers; i += 2) {
				var header = this[firstHandler + i];
				var visibility = (VisibilityModifier) (header & MessageHandlerBase.VISIBILITY_MASK);
				var name = VirtualMachine.ConstantPool.GetString( header >> MessageHandlerBase.NAME_RSHIFT ).Value;

				if (visibility == VisibilityModifier.Private && caller.Class != this)
					continue;
				if (!messageName.Equals( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( this ))
					continue;

				return (MessageHandlerBase) this[firstHandler + i + 1];
			}

			return this.DefaultHandler;
		}

		public Class ResolveClass( Class referencer, String className ) {
			var firstClass = Class.SUPERCLASSES_OFFSET + this.SuperClassCount + this.HandlerCount * 2 + 1;
			var classes = this.InnerClassCount * 2;

			for (var i = 0; i < classes; i += 2) {
				var header = this[firstClass + i];
				var visibility = (VisibilityModifier) (header & Class.VISIBILITY_MASK);
				var name = VirtualMachine.ConstantPool.GetString( header >> Class.NAME_RSHIFT );

				if (!referencer.IsNull && visibility == VisibilityModifier.Private && referencer != this)
					continue;
				if (!className.Equals( name ))
					continue;
				if (!referencer.IsNull && visibility == VisibilityModifier.Protected && !referencer.Extends( this ))
					continue;

				return (Class) this[firstClass + i + 1];
			}

			return (Class) 0;
		}

		public bool Extends( Class testSuperCls ) {
			foreach (var superClsName in this.SuperClasses) {
				var superCls = VirtualMachine.ResolveClass( this.ToHandle(), superClsName.ToHandle() ).Value;

				if (superCls == testSuperCls)
					return true;
				else
					superCls.Extends( testSuperCls );
			}

			return false;
		}

		public override string ToString() {
			if (IsNull)
				return "{NULL}";
			return ".class " + Visibility.ToString().ToLower() + " " + Name + (SuperClassCount > 0 ? " extends " + SuperClasses.Join( ", " ) : "");
		}
		#endregion
	}
}

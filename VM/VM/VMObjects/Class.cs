using System;
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
		public const int LINEARIZATION_OFFSET = 4;
		public const int SUPERCLASSES_OFFSET = 5;

		public static readonly Word VISIBILITY_MASK = 0x00000003;

		public const int NAME_RSHIFT = 2;

		public const int COUNTS_FIELDS_RSHIFT = 18;
		public static readonly Word COUNTS_HANDLERS_MASK = 0x0003FFF0;
		public const int COUNTS_HANDLERS_RSHIFT = 4;
		public static readonly Word COUNTS_SUPERCLASSES_MASK = 0x0000000F;
		#endregion

		#region Properties
		int start;
		public int Start { get { return start; } }
		public TypeId TypeIdAtInstancing { get { return TypeId.Class; } }
		#endregion

		#region Cons
		public Class( int start ) {
			this.start = start;
		}

		public Class New( int start ) {
			return new Class( start );
		}
		#endregion

		#region Static methods
		public static Handle<Class> CreateInstance( int superClassCount, int handlerCount, int innerClassCount ) {
			return VirtualMachine.MemoryManager.Allocate<Class>( SUPERCLASSES_OFFSET - 1 + superClassCount + 1 + 2 * handlerCount + 2 * innerClassCount );
		}
		#endregion

		#region Casts
		public static implicit operator int( Class cls ) {
			return cls.start;
		}

		public static explicit operator Class( int cls ) {
			return new Class( cls );
		}
		#endregion

		#region Instance method
		public override string ToString() {
			return ExtClass.ToString( this );
		}
		#endregion
	}

	public static class ExtClass {
		public static VisibilityModifier Visibility( this Handle<Class> obj ) {
			return (VisibilityModifier) (obj[Class.HEADER_OFFSET] & Class.VISIBILITY_MASK);
		}

		public static int FieldCount( this Handle<Class> obj ) {
			return obj[Class.COUNTS_OFFSET] >> Class.COUNTS_FIELDS_RSHIFT;
		}

		public static int HandlerCount( this Handle<Class> obj ) {
			return (obj[Class.COUNTS_OFFSET] & Class.COUNTS_HANDLERS_MASK) >> Class.COUNTS_HANDLERS_RSHIFT;
		}

		public static int SuperClassCount( this Handle<Class> obj ) {
			return obj[Class.COUNTS_OFFSET] & Class.COUNTS_SUPERCLASSES_MASK;
		}

		public static int InnerClassCount( this Handle<Class> obj ) {
			return (obj.Size() - Class.SUPERCLASSES_OFFSET - obj.SuperClassCount() - 1 - obj.HandlerCount() * 2) / 2;
		}

		public static int InstanceSize( this Handle<Class> obj ) {
			return obj.FieldCount() + AppObject.FIELDS_OFFSET;
		}

		public static Handle<String> Name( this Handle<Class> obj ) {
			return VirtualMachine.ConstantPool.GetString( obj[Class.HEADER_OFFSET] >> Class.NAME_RSHIFT );
		}

		public static Handle<MessageHandlerBase> DefaultHandler( this Handle<Class> obj ) {
			return (MessageHandlerBase) obj[Class.SUPERCLASSES_OFFSET + obj.SuperClassCount()];
		}

		public static Handle<Class> ParentClass( this Handle<Class> obj ) {
			return (Class) obj[Class.PARENT_CLASS_OFFSET];
		}

		public static IEnumerable<Handle<String>> SuperClasses( this Handle<Class> obj ) {
			for (var i = Class.SUPERCLASSES_OFFSET; i < Class.SUPERCLASSES_OFFSET + obj.SuperClassCount(); i++)
				yield return VirtualMachine.ConstantPool.GetString( obj[i] );
		}

		public static IEnumerable<Handle<MessageHandlerBase>> MessageHandlers( this Handle<Class> obj ) {
			var firstHandler = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			var handlers = obj.HandlerCount() * 2;
			for (var i = 1; i < handlers; i += 2)
				yield return (MessageHandlerBase) obj[firstHandler + i];
		}

		public static Handle<MessageHandlerBase> ResolveMessageHandler( this Handle<Class> obj, Handle<AppObject> caller, Handle<String> messageName ) {
			var firstHandler = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			var handlers = obj.HandlerCount() * 2;

			for (var i = 0; i < handlers; i += 2) {
				var header = obj[firstHandler + i];
				var visibility = (VisibilityModifier) (header & MessageHandlerBase.VISIBILITY_MASK);
				var name = VirtualMachine.ConstantPool.GetString( header >> MessageHandlerBase.NAME_RSHIFT );

				if (visibility == VisibilityModifier.Private && caller.Class() != obj)
					continue;
				if (!messageName.Equals( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( obj ))
					continue;

				return (MessageHandlerBase) obj[firstHandler + i + 1];
			}

			return obj.DefaultHandler();
		}

		public static Handle<Class> ResolveClass( this Handle<Class> obj, Handle<Class> referencer, Handle<String> className ) {
			var firstClass = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.HandlerCount() * 2 + 1;
			var classes = obj.InnerClassCount() * 2;

			for (var i = 0; i < classes; i += 2) {
				var header = obj[firstClass + i];
				var visibility = (VisibilityModifier) (header & Class.VISIBILITY_MASK);
				var name = VirtualMachine.ConstantPool.GetString( header >> Class.NAME_RSHIFT );

				if (referencer != null && visibility == VisibilityModifier.Private && referencer != obj)
					continue;
				if (!className.Equals( name ))
					continue;
				if (referencer != null && visibility == VisibilityModifier.Protected && !referencer.Extends( obj ))
					continue;

				return (Class) obj[firstClass + i + 1];
			}

			return (Class) 0;
		}

		public static bool Extends( this Handle<Class> obj, Handle<Class> testSuperCls ) {
			foreach (var superClsName in obj.SuperClasses()) {
				var superCls = VirtualMachine.ResolveClass( obj, superClsName );

				if (superCls == testSuperCls)
					return true;
				else
					superCls.Extends( testSuperCls );
			}

			return false;
		}

		public static string ToString( this Handle<Class> obj ) {
			if (obj == null)
				return "{NULL}";
			return ".class " + obj.Visibility().ToString().ToLower() + " " + obj.Name() + (obj.SuperClassCount() > 0 ? " extends " + obj.SuperClasses().Join( ", " ) : "");
		}
	}
}

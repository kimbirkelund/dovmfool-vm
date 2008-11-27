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
		public const int INSTANCE_SIZE_OFFSET = 5;
		public const int SUPERCLASSES_OFFSET = 6;

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

		public static int MessageHandlerCount( this Handle<Class> obj ) {
			return (obj[Class.COUNTS_OFFSET] & Class.COUNTS_HANDLERS_MASK) >> Class.COUNTS_HANDLERS_RSHIFT;
		}

		public static int SuperClassCount( this Handle<Class> obj ) {
			return obj[Class.COUNTS_OFFSET] & Class.COUNTS_SUPERCLASSES_MASK;
		}

		public static int InnerClassCount( this Handle<Class> obj ) {
			return (obj.Size() - Class.SUPERCLASSES_OFFSET - obj.SuperClassCount() - 1 - obj.MessageHandlerCount() * 2) / 2;
		}

		public static int TotalFieldCount( this Handle<Class> obj ) {
			if (obj[Class.INSTANCE_SIZE_OFFSET] == -1) {
				int instanceSize = 0;
				var lin = obj.Linearization();
				for (int i = 0; i < lin.Length(); i++) {
					var superCls = ((Class) lin.Get( i )).ToHandle();
					instanceSize += superCls.FieldCount();
				}
				obj[Class.INSTANCE_SIZE_OFFSET] = instanceSize;
			}
			return obj[Class.INSTANCE_SIZE_OFFSET];
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
			var handlers = obj.MessageHandlerCount() * 2;
			for (var i = 1; i < handlers; i += 2)
				yield return (MessageHandlerBase) obj[firstHandler + i];
		}

		public static Handle<MessageHandlerBase> ResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var dotIndex = messageName.IndexOf( String.Dot );
			if (dotIndex != -1) {
				var clsName = messageName.Substring( 0, dotIndex );
				messageName = messageName.Substring( dotIndex + 1 );

				if (!obj.Is( caller ))
					return null;

				foreach (var superCls in caller.SuperClasses()) {
					if (superCls == clsName)
						return VirtualMachine.ResolveClass( caller, clsName ).InternNoDefaultResolveMessageHandler( caller, messageName );
				}

				return null;
			} else
				return obj.InternNoDefaultResolveMessageHandler( caller, messageName ) ?? obj.DefaultHandler();

		}

		static Handle<MessageHandlerBase> InternNoDefaultResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var lin = obj.Linearization();
			for (int i = 0; i < lin.Length(); i++) {
				var cls = lin.Get<Class>( i );
				var handler = cls.InternResolveMessageHandler( caller, messageName );
				if (handler != null)
					return handler;
			}

			return null;
		}

		static Handle<MessageHandlerBase> InternResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var firstHandler = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			var handlers = obj.MessageHandlerCount() * 2;

			for (var i = 0; i < handlers; i += 2) {
				var header = obj[firstHandler + i];
				var visibility = (VisibilityModifier) (header & MessageHandlerBase.VISIBILITY_MASK);
				var name = VirtualMachine.ConstantPool.GetString( header >> MessageHandlerBase.NAME_RSHIFT );

				if (visibility == VisibilityModifier.Private && caller != obj)
					continue;
				if (!messageName.Equals( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( obj ))
					continue;

				return (MessageHandlerBase) obj[firstHandler + i + 1];
			}

			return null;
		}

		public static IEnumerable<Handle<Class>> InnerClasses( this Handle<Class> obj ) {
			var firstClass = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.MessageHandlerCount() * 2 + 1;
			var classes = obj.InnerClassCount() * 2;
			for (var i = 1; i < classes; i += 2)
				yield return (Class) obj[firstClass + i];
		}

		public static Handle<Class> ResolveInnerClass( this Handle<Class> obj, Handle<Class> referencer, Handle<String> className ) {
			var firstClass = Class.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.MessageHandlerCount() * 2 + 1;
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

		public static bool Is( this Handle<Class> obj, Handle<Class> testCls ) {
			return obj == testCls || obj.Extends( testCls );
		}

		public static bool Extends( this Handle<Class> obj, Handle<Class> testSuperCls ) {
			foreach (var superClsName in obj.SuperClasses()) {
				var superCls = VirtualMachine.ResolveClass( obj, superClsName );

				if (superCls == testSuperCls || superCls.Extends( testSuperCls ))
					return true;
			}

			return false;
		}

		public static string ToString( this Handle<Class> obj ) {
			if (obj == null)
				return "{NULL}";
			return ".class " + obj.Visibility().ToString().ToLower() + " " + obj.Name() + (obj.SuperClassCount() > 0 ? " extends " + obj.SuperClasses().Join( ", " ) : "");
		}

		public static Handle<Array> Linearization( this Handle<Class> obj ) {
			if (obj[Class.LINEARIZATION_OFFSET] == 0)
				obj.Linearize();
			return (Array) obj[Class.LINEARIZATION_OFFSET];
		}

		static void Linearize( this Handle<Class> obj ) {
			Handle<Array> list = null;

			foreach (var superClsName in obj.SuperClasses().Reverse()) {
				var superCls = VirtualMachine.ResolveClass( obj, superClsName );
				if (superCls == null)
					throw new ClassNotFoundException( superClsName );
				if (list == null)
					list = superCls.Linearization();
				else
					list = MergeLinearizations( list, superCls.Linearization() );
			}
			Handle<Array> arr;
			if (list == null)
				arr = Array.CreateInstance( 1 );
			else {
				arr = Array.CreateInstance( list.Length() + 1 );
				Array.Copy( list, 0, arr, 1, list.Length() );
			}
			arr.Set( 0, obj, true );
			obj[Class.LINEARIZATION_OFFSET] = arr;
		}

		#region MergeLinearizations
		static Handle<Array> MergeLinearizations( Handle<Array> l1, Handle<Array> l2 ) {
			if (l1 == null || l1.Length() == 0)
				return l2;
			if (l2 == null || l2.Length() == 0)
				return l1;

			bool allInOther;
			bool noneInOther = true;
			var l1InL2 = new bool[l1.Length()];
			var l2InL1 = new bool[l2.Length()];
			if (l1.Length() < l2.Length())
				goto checkL2;
		checkL1:
			allInOther = true;
			{
				int l = 0;
				for (int k = 0; k < l1.Length(); k++)
					for (int n = l; n < l2.Length(); n++) {
						var check = l1.Get( k ).Equals( l2.Get( n ) );
						allInOther &= check;
						noneInOther &= !check;
						if (check) {
							l1InL2[k] = true;
							l++;
							break;
						}
					}
			}
			if (allInOther)
				return l2;

			if (l2.Length() > l1.Length())
				goto checkingDone;

		checkL2:
			allInOther = true;
			{
				int l = 0;
				for (int k = 0; k < l2.Length(); k++)
					for (int n = l; n < l1.Length(); n++) {
						var check = l2.Get( k ).Equals( l1.Get( n ) );
						allInOther &= check;
						noneInOther &= !check;
						if (check) {
							l2InL1[k] = true;
							l++;
							break;
						}
					}
			}
			if (allInOther)
				return l1;

			if (l1.Length() < l2.Length())
				goto checkL1;

		checkingDone:
			if (noneInOther) {
				var l = Array.CreateInstance( l1.Length() + l2.Length() );
				int j = 0;
				for (int i = 0; i < l1.Length(); i++)
					l.Set( j++, l1.Get( i ), true );
				for (int i = 0; i < l2.Length(); i++)
					l.Set( j++, l2.Get( i ), true );
				return l;
			} else {
				var l = new List<Handle<Class>>();

				// Starting from the end of each list
				int i = 0, j = 0;
				// Until all mixins has been checked
				while (i < l1.Length() && j < l2.Length()) {
					if (l1.Get( i ).Equals( l2.Get( j ) )) { // If we have the same mixin in both lists, simply add it and move on
						l.Add( l1.Get<Class>( i ) );
						i++;
						j++;
					} else if (!l2InL1[j]) // If the mixin m1[i] \notin m2, add m1[i]
						l.Add( l2.Get<Class>( j++ ) );
					else if (!l1InL2[i]) // If the mixin m2[j] \notin m1, add m2[j]
						l.Add( l1.Get<Class>( i++ ) );
					else // If both the mixin m1[i] and m2[j] are in both lists, just add the one from m2 and move on
						l.Add( l2.Get<Class>( j++ ) );
				}

				if (i >= l1.Length())
					for (int j2 = j + 1; j2 < l2.Length(); j2++)
						l.Add( l2.Get<Class>( j2 ) );
				if (j >= l2.Length())
					for (int i2 = i + 1; i2 < l1.Length(); i2++)
						l.Add( l1.Get<Class>( i2 ) );

				return l.ToVMArray();
			}
		}
		#endregion
	}
}

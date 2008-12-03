using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class ClassConsts {
		public const int HEADER_OFFSET = 1;
		public const int PARENT_CLASS_OFFSET = 2;
		public const int COUNTS1_OFFSET = 3;
		public const int COUNTS2_OFFSET = 4;
		public const int LINEARIZATION_OFFSET = 5;
		public const int INSTANCE_SIZE_OFFSET = 6;
		public const int SUPERCLASSES_OFFSET = 7;

		public static readonly Word VISIBILITY_MASK = 0x00000003;

		public const int NAME_RSHIFT = 2;

		public const int COUNTS1_FIELDS_RSHIFT = 16;
		public static readonly Word COUNTS1_HANDLERS_MASK = 0x0000FFFF;
		public const int COUNTS2_INNERCLASSES_RSHIFT = 16;
		public static readonly Word COUNTS2_SUPERCLASSES_MASK = 0x0000FFFF;
	}

	public struct Class : IVMObject<Class> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.System_Reflection_Class; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}
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
		public static Class CreateInstance( int superClassCount, int handlerCount, int innerClassCount ) {
			var cls = VirtualMachine.MemoryManager.Allocate<Class>( ClassConsts.SUPERCLASSES_OFFSET - 1 + superClassCount + 1 + 2 * handlerCount + 2 * innerClassCount );
			cls[ClassConsts.INSTANCE_SIZE_OFFSET] = -1;
			cls[ClassConsts.LINEARIZATION_OFFSET] = 0;
			return cls;
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
			return ExtClass.ToString( this.ToHandle() );
		}
		#endregion

		public bool Equals( Handle<Class> obj1, Handle<Class> obj2 ) {
			return obj1.Start == obj2.Start;
		}
	}

	public static class ExtClass {
		public static VisibilityModifier Visibility( this Handle<Class> obj ) {
			return (VisibilityModifier) (obj[ClassConsts.HEADER_OFFSET] & ClassConsts.VISIBILITY_MASK);
		}

		public static int FieldCount( this Handle<Class> obj ) {
			return obj[ClassConsts.COUNTS1_OFFSET] >> ClassConsts.COUNTS1_FIELDS_RSHIFT;
		}

		public static int MessageHandlerCount( this Handle<Class> obj ) {
			return obj[ClassConsts.COUNTS1_OFFSET] & ClassConsts.COUNTS1_HANDLERS_MASK;
		}

		public static int SuperClassCount( this Handle<Class> obj ) {
			return obj[ClassConsts.COUNTS2_OFFSET] & ClassConsts.COUNTS2_SUPERCLASSES_MASK;
		}

		public static int InnerClassCount( this Handle<Class> obj ) {
			return obj[ClassConsts.COUNTS2_OFFSET] >> ClassConsts.COUNTS2_INNERCLASSES_RSHIFT;
		}

		public static int TotalFieldCount( this Handle<Class> obj ) {
			if (obj[ClassConsts.INSTANCE_SIZE_OFFSET] == -1) {
				int instanceSize = 0;
				var lin = obj.Linearization().ToHandle();
				for (int i = 0; i < lin.Length(); i++) {
					var superCls = lin.Get<Class>( i );
					instanceSize += superCls.FieldCount();
				}
				obj[ClassConsts.INSTANCE_SIZE_OFFSET] = instanceSize;
			}
			return obj[ClassConsts.INSTANCE_SIZE_OFFSET];
		}

		public static String Name( this Handle<Class> obj ) {
			return String.GetString( obj[ClassConsts.HEADER_OFFSET] >> ClassConsts.NAME_RSHIFT );
		}

		public static MessageHandlerBase DefaultHandler( this Handle<Class> obj ) {
			return ((MessageHandlerBase) obj[ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount()]);
		}

		public static Class ParentClass( this Handle<Class> obj ) {
			return (Class) obj[ClassConsts.PARENT_CLASS_OFFSET];
		}

		public static IEnumerable<String> SuperClasses( this Handle<Class> obj ) {
			for (var i = ClassConsts.SUPERCLASSES_OFFSET; i < ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount(); i++)
				yield return (String) obj[i];
		}

		public static IEnumerable<MessageHandlerBase> MessageHandlers( this Handle<Class> obj ) {
			var firstHandler = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			var handlers = obj.MessageHandlerCount() * 2;
			for (var i = 1; i < handlers; i += 2)
				yield return (MessageHandlerBase) obj[firstHandler + i];
		}

		public static MessageHandlerBase ResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var dotIndex = messageName.LastIndexOf( String.Dot );
			if (dotIndex != -1) {
				var clsName = messageName.Substring( 0, dotIndex ).ToHandle();
				messageName = messageName.Substring( dotIndex + 1 ).ToHandle();
				var cls = VirtualMachine.ResolveClass( obj, clsName ).ToHandle();

				if (!obj.Is( caller ))
					return (MessageHandlerBase) 0;

				foreach (var superCls in caller.Linearization().ToHandle().GetEnumerator<Class>()) {
					if (superCls == cls)
						return cls.InternNoDefaultResolveMessageHandler( caller, messageName );
				}

				return (MessageHandlerBase) 0;
			} else {
				var hand = obj.InternNoDefaultResolveMessageHandler( caller, messageName );
				return hand.IsNull() ? obj.DefaultHandler() : hand;
			}
		}

		static MessageHandlerBase InternNoDefaultResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var lin = obj.Linearization().ToHandle();
			for (int i = 0; i < lin.Length(); i++) {
				var cls = lin.Get<Class>( i );
				var handler = cls.InternResolveMessageHandler( caller, messageName );
				if (!handler.IsNull())
					return handler;
			}

			return (MessageHandlerBase) 0;
		}

		static MessageHandlerBase InternResolveMessageHandler( this Handle<Class> obj, Handle<Class> caller, Handle<String> messageName ) {
			var firstHandler = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			var handlers = obj.MessageHandlerCount() * 2;

			for (var i = 0; i < handlers; i += 2) {
				var header = obj[firstHandler + i];
				var visibility = (VisibilityModifier) (header & MessageHandlerBaseConsts.VISIBILITY_MASK);
				var name = String.GetString( header >> MessageHandlerBaseConsts.NAME_RSHIFT );

				if (visibility == VisibilityModifier.Private && caller != obj)
					continue;
				if (!messageName.Equals( name ))
					continue;
				if (visibility == VisibilityModifier.Protected && !caller.Extends( obj ))
					continue;

				return (MessageHandlerBase) obj[firstHandler + i + 1];
			}

			return (MessageHandlerBase) 0;
		}

		public static IEnumerable<Class> InnerClasses( this Handle<Class> obj ) {
			var firstClass = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.MessageHandlerCount() * 2 + 1;
			var classes = obj.InnerClassCount() * 2;
			for (var i = 1; i < classes; i += 2)
				yield return (Class) obj[firstClass + i];
		}

		public static Class ResolveInnerClass( this Handle<Class> obj, Handle<Class> referencer, Handle<String> className ) {
			var firstClass = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.MessageHandlerCount() * 2 + 1;
			var classes = obj.InnerClassCount() * 2;

			for (var i = 0; i < classes; i += 2) {
				var header = obj[firstClass + i];
				var visibility = (VisibilityModifier) (header & ClassConsts.VISIBILITY_MASK);
				var name = String.GetString( header >> ClassConsts.NAME_RSHIFT );

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
				var superCls = VirtualMachine.ResolveClass( obj, superClsName.ToHandle() ).ToHandle();

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

		public static Array Linearization( this Handle<Class> obj ) {
			if (obj[ClassConsts.LINEARIZATION_OFFSET] == -1)
				obj.Linearize();
			return (Array) obj[ClassConsts.LINEARIZATION_OFFSET];
		}

		static void Linearize( this Handle<Class> obj ) {
			Handle<Array> list = null;

			foreach (var superClsName in obj.SuperClasses().Reverse().Select( s => s.ToHandle() )) {
				var superCls = VirtualMachine.ResolveClass( obj, superClsName ).ToHandle();
				if (superCls == null)
					throw new ClassNotFoundException( superClsName );
				if (list == null)
					list = superCls.Linearization().ToHandle();
				else
					list = MergeLinearizations( list, superCls.Linearization().ToHandle() );
			}
			Handle<Array> arr;
			if (list == null)
				arr = Array.CreateInstance( 1 ).ToHandle();
			else {
				arr = Array.CreateInstance( list.Length() + 1 ).ToHandle();
				Array.Copy( list, 0, arr, 1, list.Length() );
			}
			arr.Set( 0, obj );
			obj[ClassConsts.LINEARIZATION_OFFSET] = arr;
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
				var l = Array.CreateInstance( l1.Length() + l2.Length() ).ToHandle();
				int j = 0;
				for (int i = 0; i < l1.Length(); i++)
					l.Set( j++, l1.Get<Class>( i ) );
				for (int i = 0; i < l2.Length(); i++)
					l.Set( j++, l2.Get<Class>( i ) );
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

				return l.ToVMArray().ToHandle();
			}
		}
		#endregion

		public static void InitInstance( this Handle<Class> obj, VisibilityModifier visibility, Handle<String> name, Handle<Class> parentClass, IList<Handle<String>> superClasses, int fieldCount, Handle<MessageHandlerBase> defaultHandler, IList<Handle<MessageHandlerBase>> messageHandlers, IList<Handle<Class>> innerClasses ) {
			if (name == null)
				throw new InvalidVMProgramException( "Class with no name specified.".ToVMString() );
			if (fieldCount > 0x0000FFFF)
				throw new InvalidVMProgramException( "Class with more than 65535 fields specified".ToVMString() );
			if (messageHandlers.Count > 0x0000FFFF)
				throw new InvalidVMProgramException( "Class with more than 65535 message handlers specified".ToVMString() );
			if (superClasses.Count > 0x0000FFFF)
				throw new InvalidVMProgramException( "Class with more than 65535 super classes specified".ToVMString() );
			if (innerClasses.Count > 0x0000FFFF)
				throw new InvalidVMProgramException( "Class with more than 65535 inner classes specified".ToVMString() );

			obj[ClassConsts.HEADER_OFFSET] = (name.GetInternIndex() << ClassConsts.NAME_RSHIFT) | (int) visibility;
			obj[ClassConsts.PARENT_CLASS_OFFSET] = parentClass;
			obj[ClassConsts.INSTANCE_SIZE_OFFSET] = -1;
			obj[ClassConsts.LINEARIZATION_OFFSET] = -1;
			obj[ClassConsts.COUNTS1_OFFSET] = (fieldCount << ClassConsts.COUNTS1_FIELDS_RSHIFT) | messageHandlers.Count;
			obj[ClassConsts.COUNTS2_OFFSET] = (innerClasses.Count << ClassConsts.COUNTS2_INNERCLASSES_RSHIFT) | superClasses.Count;

			superClasses.ForEach( ( c, i ) => obj[ClassConsts.SUPERCLASSES_OFFSET + i] = c );

			obj[ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount()] = defaultHandler;

			var firstHandler = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + 1;
			messageHandlers.ForEach( ( h, i ) => {
				obj[firstHandler + i * 2] = h[MessageHandlerBaseConsts.HEADER_OFFSET];
				obj[firstHandler + i * 2 + 1] = h;
			} );

			var firstClass = ClassConsts.SUPERCLASSES_OFFSET + obj.SuperClassCount() + obj.MessageHandlerCount() * 2 + 1;
			innerClasses.ForEach( ( c, i ) => {
				obj[firstClass + i * 2] = c[ClassConsts.HEADER_OFFSET];
				obj[firstClass + i * 2 + 1] = c;
			} );
		}
	}
}

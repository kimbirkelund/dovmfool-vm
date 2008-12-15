using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VMILLib;

namespace VM.VMObjects {
	internal static class AppObjectConsts {
		public const int FIELDS_OFFSET_OFFSET = 0;
		public const int FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET = 1;
	}

	public struct AppObject : IVMObject<AppObject> {
		#region Properties
		int start;
		public int Start { get { return start; } }
		public Handle<Class> VMClass { get { return KnownClasses.Object; } }
		public Word this[int index] {
			get { return VirtualMachine.MemoryManager[Start + index]; }
			set { VirtualMachine.MemoryManager[Start + index] = value; }
		}
		#endregion

		#region Cons
		public AppObject( int start ) {
			this.start = start;
		}

		public AppObject New( int startPosition ) {
			return new AppObject( startPosition );
		}
		#endregion

		#region Casts
		public static implicit operator int( AppObject cls ) {
			return cls.start;
		}

		public static explicit operator AppObject( int cls ) {
			return new AppObject { start = cls };
		}
		#endregion

		#region Static methods
		public static AppObject CreateInstance( Handle<Class> cls ) {
			using (var linearization = cls.Linearization().ToHandle()) {
				var obj = VirtualMachine.MemoryManager.Allocate<AppObject>( cls.TotalFieldCount() * 2 + linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET );
				MarkSweepCompactMemoryManager.DisableAddressVerification();
				obj[ObjectBase.CLASS_POINTER_OFFSET] = cls;
				MarkSweepCompactMemoryManager.EnableAddressVerification();
				obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] = linearization.Length() + AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET;

				int fieldOffset = 0;
				for (int i = 0; i < linearization.Length(); i++) {
					obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i] = fieldOffset;
					using (var hElem = linearization.Get<Class>( i ).ToWeakHandle())
						fieldOffset += hElem.FieldCount();
				}
				return obj;
			}
		}

		public static AppObject CreateInstance( Handle<String> clsName ) {
			using (var cls = VirtualMachine.ResolveClass( null, clsName ).ToHandle()) {
				if (cls != null)
					throw new ClassNotFoundException( clsName );

				return CreateInstance( cls );
			}
		}

		public static AppObject CreateInstance( string clsName ) {
			using (var hClsName = clsName.ToVMString().ToHandle())
				return CreateInstance( hClsName );
		}

		internal static int[] GetReferences( int adr ) {
			var fieldCount = ((AppObject) adr).ToWeakHandle().Class().ToWeakHandle().TotalFieldCount() * 2;
			var firstField = VirtualMachine.MemoryManager[adr + AppObjectConsts.FIELDS_OFFSET_OFFSET];

			List<int> refs = new List<int>();
			for (int i = 0; i < fieldCount; i += 2) {
				int cls = VirtualMachine.MemoryManager[adr + firstField + i];
				if (cls < 0)
					cls = KnownClasses.Resolve( cls ).Start;
				if (cls != 0) {
					if (cls > 0)
						refs.Add( cls );
					if (cls != KnownClasses.System_Integer.Start) {
						var val = VirtualMachine.MemoryManager[adr + firstField + i + 1];
						if (val != 0)
							refs.Add( val );
					}
				}
			}
			if (refs.Any( i => i < 0 ))
				Console.WriteLine();
			return refs.ToArray();
		}
		#endregion

		#region Instance methods
		public override string ToString() {
			using (var hThis = this.ToHandle())
				return ExtAppObject.ToString( hThis );
		}

		public bool Equals( Handle<AppObject> obj1, Handle<AppObject> obj2 ) {
			return obj1.Start == obj2.Start;
		}
		#endregion
	}

	public static class ExtAppObject {
		public static bool Extends( this Handle<AppObject> obj, Handle<Class> cls ) {
			using (var hObjClass = obj.Class().ToHandle())
				return hObjClass.Extends( cls );
		}

		internal static UValue GetField( this Handle<AppObject> obj, int index ) {
			using (var hObjClass = obj.Class().ToHandle()) {
				if (index < 0 || hObjClass.TotalFieldCount() <= index)
					throw new ArgumentOutOfRangeException( "Index must be less than the number of fields.".ToVMString().ToHandle() );
			}

			var type = (Class) obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2];
			var value = obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1];
			if (type == KnownClasses.System_Integer.Start)
				return UValue.Int( value );
			return UValue.Ref( type, value );
		}

		public static void SetField<T>( this Handle<AppObject> obj, int index, Handle<T> value ) where T : struct, IVMObject<T> {
			if (value == null) {
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = 0;
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = 0;
			} else {
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = value.Class().Start;
				obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = value.Start;
			}
		}

		internal static void SetField( this Handle<AppObject> obj, int index, UValue value ) {
			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2] = value.Type;
			obj[obj[AppObjectConsts.FIELDS_OFFSET_OFFSET] + index * 2 + 1] = value.Value;
		}

		public static int GetFieldOffset( this Handle<AppObject> obj, Handle<Class> superClass ) {
			if (superClass == null)
				new ArgumentNullException( "superClass" );

			using (var hObjClass = obj.Class().ToHandle())
			using (var lin = hObjClass.Linearization().ToHandle())
				for (int i = 0; i < lin.Length(); i++)
					if (lin.Get<Class>( i ) == superClass.Start)
						return obj[AppObjectConsts.FIRST_SUPERCLASS_FIELDS_OFFSET_OFFSET + i];

			throw new ClassNotFoundException( "Specified class is not in the inheritance hierachy.".ToVMString().ToHandle(), superClass.Name().ToHandle() );
		}

		public static string ToString( this Handle<AppObject> obj ) {
			if (obj.IsNull())
				return "{NULL}";
			return "Instance of " + obj.Class();
		}

		public static Handle<AppObject> Send( this Handle<AppObject> obj, Handle<String> message, params Handle<AppObject>[] arguments ) {
			using (var cls = obj.Class().ToHandle())
			using (var handler = cls.ResolveMessageHandler( null, message ).ToHandle()) {
				if (handler == null)
					throw new MessageNotUnderstoodException( message );

				var interp = VirtualMachine.Fork( handler, obj, arguments );
				interp.Start();
				return interp.Join();
			}
		}
	}
}

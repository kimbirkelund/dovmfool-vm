using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	struct UValue {
		public readonly Word Value;
		public readonly Class Type;
		public bool IsReference { get { return Type > 0 && Type.Start != KnownClasses.System_Integer.Start; } }
		public bool IsVoid { get { return Type.Start == KnownClasses.Void.Start; } }
		public bool IsNull { get { return Type == 0 && Value == 0; } }

		UValue( Class type, Word value ) {
			this.Type = type;
			this.Value = value;
		}

		public static UValue Void() { return new UValue( KnownClasses.Void, 0 ); }
		public static UValue Null() { return new UValue( (Class) 0, 0 ); }
		public static UValue Int( int i ) { return new UValue( KnownClasses.System_Integer.Value, i ); }
		public static UValue Ref( Class type, Word value ) {
			if (type == KnownClasses.System_Integer.Start)
				return Int( value );
			return new UValue( type, value );
		}

		public static implicit operator UValue( int value ) {
			return Int( value );
		}

		#region Equals
		public bool Equals( UValue value ) {
			return this.Value == value.Value && this.Type == value.Type && this.IsReference == value.IsReference && this.IsVoid == value.IsVoid;
		}

		public override bool Equals( object value ) {
			if (value == null)
				return false;
			return this == (UValue) value;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==( UValue value1, UValue value2 ) {
			if (object.ReferenceEquals( value1, null ) && object.ReferenceEquals( value2, null ))
				return true;
			if (object.ReferenceEquals( value1, null ) || object.ReferenceEquals( value2, null ))
				return false;
			return value1.Equals( value2 );
		}

		public static bool operator !=( UValue value1, UValue value2 ) {
			return !(value1 == value2);
		}
		#endregion

		public override string ToString() {
			if (IsNull)
				return "NULL";
			if (Type.Start < 0) {
				switch (Type.Start) {
					case -1: return "BASE POINTER: " + Value;
					case -2: return "RETURN HANDLER: " + ((MessageHandlerBase) Value);
					case -3: return "OLD PC: " + Value;
					case -4: return "FRAME BOUNDARY: " + Value;
					case -5: return "TRY MARKER: " + Value;
					case -6: return "RETURN HERE: " + Value;
					default:
						throw new ArgumentException( ("Invalid stack value type: " + Type.Start).ToVMString().ToHandle() );
				}
			}
			if (Type == KnownClasses.System_Integer.Start)
				return Value.ToString();
			if (Type == KnownClasses.System_String.Start)
				return "\"" + ((VM.VMObjects.String) Value).ToString() + "\"";
			return "[" + Type + "]" + ((AppObject) Value).ToString();
		}
	}

	static class ExtUValue {
		public static UValue ToUValue<T>( this Handle<T> h ) where T : struct, IVMObject<T> {
			if (h is IntHandle)
				return UValue.Int( h.Start );
			if (h == null)
				return UValue.Null();
			return UValue.Ref( h.Class(), h.Start );
		}

		public static Handle<AppObject> ToHandle( this UValue h ) {
			if (h.IsVoid)
				throw new System.ArgumentException( "Void valued argument can not be converted to handle." );
			if (h.IsReference)
				return ((AppObject) h.Value).ToHandle();
			return new IntHandle( h.Value );
		}

		public static Handle<T> ToHandle<T>( this UValue h ) where T : struct, IVMObject<T> {
			if (h.IsVoid)
				throw new System.ArgumentException( "Void valued argument can not be converted to handle." );
			if (!h.IsReference)
				throw new System.ArgumentException( "Integer valued argument can not be converted to specific handle." );
			return new T().New( h.Value ).ToHandle();
		}

		public static Handle<AppObject> ToWeakHandle( this UValue h ) {
			if (h.IsVoid)
				throw new System.ArgumentException( "Void valued argument can not be converted to handle." );
			if (h.IsReference)
				return ((AppObject) h.Value).ToWeakHandle();
			return new IntHandle( h.Value );
		}

		public static Handle<T> ToWeakHandle<T>( this UValue h ) where T : struct, IVMObject<T> {
			if (h.IsVoid)
				throw new System.ArgumentException( "Void valued argument can not be converted to handle." );
			if (!h.IsReference)
				throw new System.ArgumentException( "Integer valued argument can not be converted to specific handle." );
			return new T().New( h.Value ).ToWeakHandle();
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public struct Word {
		public static readonly Word Null = 0;

		uint v;

		public static implicit operator int( Word w ) { return (int) w.v; }
		public static implicit operator uint( Word w ) { return w.v; }
		public static implicit operator byte( Word w ) { return (byte) w.v; }
		public static implicit operator char( Word w ) { return (char) w.v; }
		public static implicit operator Word( int v ) { return new Word { v = (uint) v }; }
		public static implicit operator Word( uint v ) { return new Word { v = v }; }
		public static implicit operator Word( byte v ) { return new Word { v = (uint) v }; }
		public static implicit operator Word( char v ) { return new Word { v = (uint) v }; }

		public static Word operator >>( Word w, int c ) { return (w.v >> c); }
		public static Word operator <<( Word w, int c ) { return (w.v >> c); }
		public static Word operator &( Word w, int o ) { return (w.v & (uint) o); }
		public static Word operator |( Word w, int o ) { return (w.v | (uint) o); }
		public static Word operator ^( Word w, int o ) { return (w.v ^ (uint) o); }
		public static Word operator &( Word w, uint o ) { return (w.v & o); }
		public static Word operator |( Word w, uint o ) { return (w.v | o); }
		public static Word operator ^( Word w, uint o ) { return (w.v ^ o); }
		public static Word operator &( Word w, byte o ) { return (w.v & (uint) o); }
		public static Word operator |( Word w, byte o ) { return (w.v | (uint) o); }
		public static Word operator ^( Word w, byte o ) { return (w.v ^ (uint) o); }

		#region Equals
		public bool Equals( Word value ) {
			return v == value.v;
		}

		public override bool Equals( object value ) {
			if (!(value is Word))
				return false;
			return this == (Word) value;
		}

		public override int GetHashCode() {
			return base.GetHashCode();
		}

		public static bool operator ==( Word value1, Word value2 ) {
			return value1.Equals( value2 );
		}

		public static bool operator !=( Word value1, Word value2 ) {
			return !(value1 == value2);
		}
		#endregion

		#region Other conversions
		public static explicit operator VMILLib.OpCode( Word w ) { return (VMILLib.OpCode) w.v; }
		public static explicit operator VMILLib.VisibilityModifier( Word w ) { return (VMILLib.VisibilityModifier) w.v; }

		public static explicit operator ObjectBase( Word w ) { return new ObjectBase( (int) w.v ); }
		public static explicit operator AppObject( Word w ) { return new AppObject( (int) w.v ); }
		public static explicit operator AppObjectSet( Word w ) { return new AppObjectSet( (int) w.v ); }
		public static explicit operator Class( Word w ) { return new Class( (int) w.v ); }
		public static explicit operator ClassManager( Word w ) { return new ClassManager( (int) w.v ); }
		public static explicit operator MessageHandlerBase( Word w ) { return new MessageHandlerBase( (int) w.v ); }
		public static explicit operator VMILMessageHandler( Word w ) { return new VMILMessageHandler( (int) w.v ); }
		public static explicit operator DelegateMessageHandler( Word w ) { return new DelegateMessageHandler( (int) w.v ); }
		public static explicit operator VMObjects.String( Word w ) { return new VMObjects.String( (int) w.v ); }
		public static explicit operator Integer( Word w ) { return new Integer( (int) w.v ); }
		public static explicit operator List( Word w ) { return new List( (int) w.v ); }
		public static explicit operator VMObjects.Array( Word w ) { return new VMObjects.Array( (int) w.v ); }

		public static implicit operator Word( ObjectBase w ) { return w.Start; }
		public static implicit operator Word( AppObject w ) { return w.Start; }
		public static implicit operator Word( AppObjectSet w ) { return w.Start; }
		public static implicit operator Word( Class w ) { return w.Start; }
		public static implicit operator Word( ClassManager w ) { return w.Start; }
		public static implicit operator Word( MessageHandlerBase w ) { return w.Start; }
		public static implicit operator Word( VMILMessageHandler w ) { return w.Start; }
		public static implicit operator Word( DelegateMessageHandler w ) { return w.Start; }
		public static implicit operator Word( VMObjects.String w ) { return w.Start; }
		public static implicit operator Word( Integer w ) { return w.Start; }
		public static implicit operator Word( List w ) { return w.Start; }
		public static implicit operator Word( VM.VMObjects.Array w ) { return w.Start; }
		#endregion

		public override string ToString() {
			return v.ToString();
		}
	}
}

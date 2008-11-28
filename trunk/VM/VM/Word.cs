using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public struct Word {
		public static readonly Word Null = 0;

		uint v;

		public Word( uint v ) { this.v = v; }
		public Word( int v ) { this.v = (uint) v; }
		public Word( byte v ) { this.v = v; }
		public Word( char v ) { this.v = v; }
		public Word( bool v ) { this.v = v ? 1u : 0u; }

		public static implicit operator int( Word w ) { return (int) w.v; }
		public static implicit operator uint( Word w ) { return w.v; }
		public static implicit operator byte( Word w ) { return (byte) w.v; }
		public static implicit operator char( Word w ) { return (char) w.v; }
		public static implicit operator bool( Word w ) { return Convert.ToBoolean( w.v ); }
		public static implicit operator Word( int v ) { return new Word( v ); }
		public static implicit operator Word( uint v ) { return new Word( v ); }
		public static implicit operator Word( byte v ) { return new Word( v ); }
		public static implicit operator Word( char v ) { return new Word( v ); }
		public static implicit operator Word( bool v ) { return new Word( v ); }

		public static Word operator >>( Word w, int c ) { return new Word( w.v >> c ); }
		public static Word operator <<( Word w, int c ) { return new Word( w.v << c ); }
		public static Word operator &( Word w1, Word w2 ) { return new Word( w1.v & w2.v ); }
		public static Word operator |( Word w1, Word w2 ) { return new Word( w1.v | w2.v ); }
		public static Word operator ^( Word w1, Word w2 ) { return new Word( w1.v ^ w2.v ); }

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
		public static explicit operator MessageHandlerBase( Word w ) { return new MessageHandlerBase( (int) w.v ); }
		public static explicit operator VMILMessageHandler( Word w ) { return new VMILMessageHandler( (int) w.v ); }
		public static explicit operator DelegateMessageHandler( Word w ) { return new DelegateMessageHandler( (int) w.v ); }
		public static explicit operator VMObjects.String( Word w ) { return new VMObjects.String( (int) w.v ); }
		public static explicit operator Integer( Word w ) { return new Integer( (int) w.v ); }
		public static explicit operator VMObjects.Array( Word w ) { return new VMObjects.Array( (int) w.v ); }

		public static implicit operator Word( ObjectBase w ) { return new Word( w.Start ); }
		public static implicit operator Word( AppObject w ) { return new Word( w.Start ); }
		public static implicit operator Word( AppObjectSet w ) { return new Word( w.Start ); }
		public static implicit operator Word( Class w ) { return new Word( w.Start ); }
		public static implicit operator Word( MessageHandlerBase w ) { return new Word( w.Start ); }
		public static implicit operator Word( VMILMessageHandler w ) { return new Word( w.Start ); }
		public static implicit operator Word( DelegateMessageHandler w ) { return new Word( w.Start ); }
		public static implicit operator Word( VMObjects.String w ) { return new Word( w.Start ); }
		public static implicit operator Word( Integer w ) { return new Word( w.Value ); }
		public static implicit operator Word( VM.VMObjects.Array w ) { return new Word( w.Start ); }
		#endregion

		public override string ToString() {
			return v.ToString();
		}
	}
}

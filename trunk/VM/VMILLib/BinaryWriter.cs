using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace VMILLib {
	public sealed class BinaryWriter : IDisposable {
		bool isDisposed;
		Stream output;
		uint pos;
		List<uint> constantMap;
		uint stringConstantOffset;

		public BinaryWriter( Stream output ) {
			this.output = output;
		}

		public BinaryWriter( string output ) : this( new FileStream( output, FileMode.OpenOrCreate, FileAccess.Write ) ) { }

		public void Write( Assembly assembly ) {
			constantMap = new List<uint>();

			WriteIntegers( assembly.Integers );
			WriteStrings( assembly.Strings );
		}

		void WriteStrings( CStringPool pool ) {
			Write( (uint) pool.Count );

			foreach (var s in pool.Select( s => s.Value )) {
				constantMap.Add( pos );
				Write( s.Length );
				var bytes = Encoding.Unicode.GetBytes( s );

				var remainder = bytes.Length % 4;
				if (remainder != 0) {
					var temp = new byte[bytes.Length + (4 - remainder)];
					Array.ConstrainedCopy( bytes, 0, temp, 0, bytes.Length );
					bytes = temp;
				}

				Write( bytes );
			}
		}

		void WriteIntegers( CIntegerPool pool ) {
			Write( (uint) pool.Count );

			foreach (var i in pool.Select( i => i.Value )) {
				constantMap.Add( pos );
				Write( i );
			}
			stringConstantOffset = pos;
		}

		void Write( IEnumerable<uint> arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params uint[] arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( IEnumerable<int> arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params int[] arr ) {
			Write( arr.ToByteStream().ToArray() );
		}

		void Write( params byte[] arr ) {
			if (arr.Length % 4 != 0)
				throw new ArgumentException( "Array length must be divisible with 4." );

			output.Write( arr, 0, arr.Length );
			pos += (uint) arr.Length / 4;
		}

		#region IDisposable Members
		public void Dispose() {
			lock (this) {
				if (isDisposed)
					return;
				isDisposed = true;
			}
			if (output != null) {
				output.Dispose();
				output = null;
			}
		}
		#endregion
	}
}

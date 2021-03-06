﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using VM.VMObjects;

namespace VM {
	delegate UValue SystemCall( UValue receiver, UValue[] arguments );

	internal static partial class SystemCalls {
		static SCCls root;
		static Dictionary<Handle<VMObjects.String>, SystemCall> cache = new Dictionary<Handle<VM.VMObjects.String>, SystemCall>();

		static SystemCalls() {
			root = new SCCls( typeof( SystemCalls ) );
		}

		public static SystemCall FindMethod( Handle<VMObjects.String> name ) {
			if (cache.ContainsKey( name ))
				return cache[name];

			var method = root.FindMethod( name );
			if (method == null)
				throw new UnknownExternalCallException( name );
			lock (typeof( SystemCalls )) {
				if (!cache.ContainsKey( name ))
					cache.Add( name, method );
			}
			return method;
		}

		#region SCCls
		class SCCls {
			Type type;
			Dictionary<Handle<VMObjects.String>, SCCls> classes;
			Dictionary<Handle<VMObjects.String>, SystemCall> methods;
			bool initialized = false;

			public SCCls( Type type ) {
				this.type = type;
			}

			public SystemCall FindMethod( Handle<VMObjects.String> name ) {
				if (!initialized)
					Init();

				var dotIndex = name.IndexOf( KnownStrings.Dot );
				if (dotIndex == -1)
					dotIndex = name.Length();

				using (var curName = name.Substring( 0, dotIndex ).ToHandle()) {
					if (dotIndex == name.Length()) {
						if (methods.ContainsKey( curName ))
							return methods[curName];
					} else if (classes.ContainsKey( curName ))
						using (var restName = name.Substring( dotIndex + 1 ).ToHandle())
							return classes[curName].FindMethod( restName );
				}
				return null;
			}

			void Init() {
				lock (this) {
					if (initialized)
						return;

					classes = new Dictionary<Handle<VM.VMObjects.String>, SCCls>();
					(from t in type.GetNestedTypes( BindingFlags.Public | BindingFlags.NonPublic )
					 from a in t.GetCustomAttributes( typeof( SystemCallClassAttribute ), false ).OfType<SystemCallClassAttribute>()
					 where a != null
					 select new { Type = t, Attribute = a }).ForEach( p => {
						 using (var hPName = p.Attribute.Name.ToVMString().Intern().ToHandle())
							 classes.Add( hPName, new SCCls( p.Type ) );
					 } );

					methods = new Dictionary<Handle<VM.VMObjects.String>, SystemCall>();
					(from m in type.GetMethods( BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static )
					 from a in m.GetCustomAttributes( typeof( SystemCallMethodAttribute ), false ).OfType<SystemCallMethodAttribute>()
					 where a != null
					 select new { Method = m, Attribute = a }).ForEach( p => {
						 using (var hPName = p.Attribute.Name.ToVMString().Intern().ToHandle())
							 methods.Add( hPName, (SystemCall) Delegate.CreateDelegate( typeof( SystemCall ), p.Method ) );
					 } );
					initialized = true;
				}
			}
		}
		#endregion
	}

	#region Attributes
	[global::System.AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
	sealed class SystemCallClassAttribute : Attribute {
		public readonly string Name;

		public SystemCallClassAttribute( string name ) {
			this.Name = name;
		}
	}

	[global::System.AttributeUsage( AttributeTargets.Method, Inherited = false, AllowMultiple = false )]
	sealed class SystemCallMethodAttribute : Attribute {
		public readonly string Name;

		public SystemCallMethodAttribute( string name ) {
			this.Name = name;
		}
	}
	#endregion
}

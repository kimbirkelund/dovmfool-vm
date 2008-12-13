using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	static partial class SystemCalls {
		partial class System {
			[SystemCallClass( "Reflection" )]
			public static partial class Reflection {
				#region MessageHandler
				[SystemCallClass( "MessageHandler" )]
				public static partial class MessageHandler {
					[SystemCallMethod( "class:0" )]
					public static UValue Class( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
							return UValue.Ref( KnownClasses.System_Reflection_Class, handler.Class() );
					}

					[SystemCallMethod( "name:0" )]
					public static UValue Name( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
							return UValue.Ref( KnownClasses.System_String, handler.Name() );
					}

					[SystemCallMethod( "argument-count:0" )]
					public static UValue ArgumentCount( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
							return handler.ArgumentCount();
					}

					[SystemCallMethod( "visibility:0" )]
					public static UValue Visibility( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
						using (var vis = AppObject.CreateInstance( KnownClasses.System_Reflection_Visibility ).ToHandle()) {
							vis.SetField( 0, (int) handler.Visibility() );
							return vis.ToUValue();
						}
					}

					[SystemCallMethod( "is-external:0" )]
					public static UValue IsExternal( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
							return handler.IsExternal() ? 1 : 0;
					}

					[SystemCallMethod( "is-default:0" )]
					public static UValue IsDefault( UValue receiver, UValue[] arguments ) {
						using (var handler = ((MessageHandlerBase) receiver.Value).ToHandle())
							return handler.Visibility() == VMILLib.VisibilityModifier.None ? 1 : 0;
					}

					[SystemCallMethod( "equals:1" )]
					public static UValue Equals( UValue receiver, UValue[] arguments ) {
						using (var handler1 = ((MessageHandlerBase) receiver.Value).ToHandle())
						using (var handler2 = ((MessageHandlerBase) receiver.Value).ToHandle()) {
							if (handler2.Class() != KnownClasses.System_Reflection_MessageHandler.Start)
								return 0;

							return handler1 == handler2 ? 1 : 0;
						}
					}
				}
				#endregion

				#region Class
				[SystemCallClass( "Class" )]
				public static partial class Class {
					[SystemCallMethod( "name:0" )]
					public static UValue Name( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
							return UValue.Ref( KnownClasses.System_String, cls.Name() );
					}

					[SystemCallMethod( "visibility:0" )]
					public static UValue Visibility( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var vis = AppObject.CreateInstance( KnownClasses.System_Reflection_Visibility ).ToHandle()) {
							vis.SetField( 0, (int) cls.Visibility() );
							return vis.ToUValue();
						}
					}

					[SystemCallMethod( "default-message-handler:0" )]
					public static UValue DefaultMessageHandler( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle()) {
							var def = cls.DefaultHandler();
							if (def.IsNull())
								return UValue.Null();

							return UValue.Ref( KnownClasses.System_Reflection_MessageHandler, def );
						}
					}

					[SystemCallMethod( "parent-class:0" )]
					public static UValue ParentClass( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
							return UValue.Ref( KnownClasses.System_Reflection_Class, cls.ParentClass() );
					}

					[SystemCallMethod( "super-class-names:0" )]
					public static UValue SuperClassNames( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() ).ToHandle()) {
							cls.SuperClasses().ForEach( ( c, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_String.Value, c ) ) );

							return UValue.Ref( KnownClasses.System_Array, arr );
						}
					}

					[SystemCallMethod( "super-classes:0" )]
					public static UValue SuperClasses( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() ).ToHandle()) {
							cls.SuperClasses().ForEach( ( c, i ) => {
								using (var hC = c.ToHandle())
									arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_Class, VirtualMachine.ResolveClass( null, hC ) ) );
							} );

							return UValue.Ref( KnownClasses.System_Array, arr );
						}
					}

					[SystemCallMethod( "message-handlers:0" )]
					public static UValue MessageHandlers( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var arr = VMObjects.Array.CreateInstance( cls.MessageHandlerCount() ).ToHandle()) {
							cls.MessageHandlers().ForEach( ( h, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_MessageHandler.Value, h ) ) );

							return UValue.Ref( KnownClasses.System_Array, arr );
						}
					}

					[SystemCallMethod( "inner-classes:0" )]
					public static UValue InnerClasses( UValue receiver, UValue[] arguments ) {
						using (var cls = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var arr = VMObjects.Array.CreateInstance( cls.InnerClassCount() ).ToHandle()) {
							cls.InnerClasses().ForEach( ( c, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_Class.Value, c ) ) );

							return UValue.Ref( KnownClasses.System_Array, arr );
						}
					}

					[SystemCallMethod( "equals:1" )]
					public static UValue Equals( UValue receiver, UValue[] arguments ) {
						using (var cls1 = ((VMObjects.Class) receiver.Value).ToHandle())
						using (var cls2 = ((VMObjects.Class) arguments[0].Value).ToHandle()) {
							if (cls2.Class() != KnownClasses.System_Reflection_Class.Value)
								return 0;

							return cls1 == cls2 ? 1 : 0;
						}
					}
				}
				#endregion

				#region Reflector
				[SystemCallClass( "Reflector" )]
				public static partial class Reflector {
					[SystemCallMethod( "classes:0" )]
					public static UValue Classes( UValue receiver, UValue[] arguments ) {
						var classes = VirtualMachine.Classes;

						using (var arr = VMObjects.Array.CreateInstance( classes.Count() ).ToHandle()) {
							classes.ForEach( ( c, i ) => arr.Set( i, c ) );

							return UValue.Ref( KnownClasses.System_Array, arr );
						}
					}

					[SystemCallMethod( "find-class:1" )]
					public static UValue FindClass( UValue receiver, UValue[] arguments ) {
						using (var arg = arguments[0].ToHandle()) {
							if (arg.Class() != KnownClasses.System_String.Value)
								throw new ArgumentException( "Argument must be of type System.String.".ToVMString().ToHandle(), "className".ToVMString().ToHandle() );

							using (var hArg = arg.To<VMObjects.String>()) {
								var cls = VirtualMachine.ResolveClass( null, hArg );
								return UValue.Ref( KnownClasses.System_Reflection_Class, cls );
							}
						}
					}
				}
				#endregion
			}
		}
	}
}
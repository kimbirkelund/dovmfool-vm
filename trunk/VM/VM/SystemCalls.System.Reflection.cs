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
				static Handle<VMObjects.String> visibilityInitStr = "initialize:1".ToVMString().Intern();

				#region MessageHandler
				[SystemCallClass( "MessageHandler" )]
				public static partial class MessageHandler {
					[SystemCallMethod( "class:0" )]
					public static UValue Class( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						return UValue.Ref( KnownClasses.System_Reflection_Class, handler.Class() );
					}

					[SystemCallMethod( "name:0" )]
					public static UValue Name( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						return UValue.Ref( KnownClasses.System_String, handler.Name() );
					}

					[SystemCallMethod( "argument-count:0" )]
					public static UValue ArgumentCount( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						return handler.ArgumentCount();
					}

					[SystemCallMethod( "visibility:0" )]
					public static UValue Visibility( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						var vis = AppObject.CreateInstance( KnownClasses.System_Reflection_Visibility ).ToHandle();
						vis.SetField( 0, (int) handler.Visibility() );
						return vis.ToUValue();
					}

					[SystemCallMethod( "is-external:0" )]
					public static UValue IsExternal( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						return handler.IsExternal() ? 1 : 0;
					}

					[SystemCallMethod( "is-default:0" )]
					public static UValue IsDefault( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.Value).ToHandle();

						return handler.Visibility() == VMILLib.VisibilityModifier.None ? 1 : 0;
					}

					[SystemCallMethod( "equals:1" )]
					public static UValue Equals( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var handler1 = ((MessageHandlerBase) receiver.Value).ToHandle();
						var handler2 = ((MessageHandlerBase) receiver.Value).ToHandle();
						if (handler2.Class() != KnownClasses.System_Reflection_Message_Handler.Start)
							return 0;

						return handler1 == handler2 ? 1 : 0;
					}
				}
				#endregion

				#region Class
				[SystemCallClass( "Class" )]
				public static partial class Class {
					[SystemCallMethod( "name:0" )]
					public static UValue Name( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						return UValue.Ref( KnownClasses.System_String, cls.Name() );
					}

					[SystemCallMethod( "visibility:0" )]
					public static UValue Visibility( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var vis = AppObject.CreateInstance( KnownClasses.System_Reflection_Visibility ).ToHandle();
						vis.SetField( 0, (int) cls.Visibility() );
						return vis.ToUValue();
					}

					[SystemCallMethod( "default-message-handler:0" )]
					public static UValue DefaultMessageHandler( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var def = cls.DefaultHandler();
						if (def.IsNull())
							return UValue.Null();

						return UValue.Ref( KnownClasses.System_Reflection_Message_Handler, def );
					}

					[SystemCallMethod( "parent-class:0" )]
					public static UValue ParentClass( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						return UValue.Ref( KnownClasses.System_Reflection_Class, cls.ParentClass() );
					}

					[SystemCallMethod( "super-class-names:0" )]
					public static UValue SuperClassNames( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() ).ToHandle();

						cls.SuperClasses().ForEach( ( c, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_String.Value, c ) ) );

						return UValue.Ref( KnownClasses.System_Array, arr );
					}

					[SystemCallMethod( "super-classes:0" )]
					public static UValue SuperClasses( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() ).ToHandle();

						cls.SuperClasses().ForEach( ( c, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_Class, VirtualMachine.ResolveClass( null, c.ToHandle() ) ) ) );

						return UValue.Ref( KnownClasses.System_Array, arr );
					}

					[SystemCallMethod( "message-handlers:0" )]
					public static UValue MessageHandlers( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.MessageHandlerCount() ).ToHandle();

						cls.MessageHandlers().ForEach( ( h, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_Message_Handler.Value, h ) ) );

						return UValue.Ref( KnownClasses.System_Array, arr );
					}

					[SystemCallMethod( "inner-classes:0" )]
					public static UValue InnerClasses( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls = ((VMObjects.Class) receiver.Value).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.InnerClassCount() ).ToHandle();

						cls.InnerClasses().ForEach( ( c, i ) => arr.Set( i, UValue.Ref( KnownClasses.System_Reflection_Class.Value, c ) ) );

						return UValue.Ref( KnownClasses.System_Array, arr );
					}

					[SystemCallMethod( "equals:1" )]
					public static UValue Equals( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var cls1 = ((VMObjects.Class) receiver.Value).ToHandle();
						var cls2 = ((VMObjects.Class) arguments[0].Value).ToHandle();
						if (cls2.Class() != KnownClasses.System_Reflection_Class.Value)
							return 0;

						return cls1 == cls2 ? 1 : 0;
					}
				}
				#endregion

				#region Reflector
				[SystemCallClass( "Reflector" )]
				public static partial class Reflector {
					[SystemCallMethod( "classes:0" )]
					public static UValue Classes( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var classes = VirtualMachine.Classes;

						var arr = VMObjects.Array.CreateInstance( classes.Count() ).ToHandle();
						classes.ForEach( ( c, i ) => arr.Set( i, c ) );

						return UValue.Ref( KnownClasses.System_Array, arr );
					}

					[SystemCallMethod( "find-class:1" )]
					public static UValue FindClass( IInterpretor interpretor, UValue receiver, UValue[] arguments ) {
						var arg = arguments[0].ToHandle();
						if (arg.Class() != KnownClasses.System_String.Value)
							throw new ArgumentException( "Argument must be of type System.String.".ToVMString(), "className".ToVMString() );

						var cls = VirtualMachine.ResolveClass( null, arg.To<VMObjects.String>() );
						return UValue.Ref( KnownClasses.System_Reflection_Class, cls );
					}
				}
				#endregion
			}
		}
	}
}
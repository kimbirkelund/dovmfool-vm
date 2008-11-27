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
				static Handle<VMObjects.Class> visibilityCls = VirtualMachine.ResolveClass( null, "System.Reflection.Visibility".ToVMString() );
				static Handle<VMObjects.String> visibilityInitStr = "initialize:1".ToVMString().Intern();
				static Handle<VMObjects.Class> messageHandlerCls = VirtualMachine.ResolveClass( null, "System.Reflection.MessageHandler".ToVMString() );
				static Handle<VMObjects.Class> classCls = VirtualMachine.ResolveClass( null, "System.Reflection.Class".ToVMString() );

				#region MessageHandler
				[SystemCallClass( "MessageHandler" )]
				public static partial class MessageHandler {
					[SystemCallMethod( "class:0" )]
					public static Handle<AppObject> Class( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();

						var cls = AppObject.CreateInstance( classCls );
						cls.SetField( 0, VirtualMachine.InternalObjectClass, cls.Start );
						return cls;
					}

					[SystemCallMethod( "name:0" )]
					public static Handle<VMObjects.AppObject> Name( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();

						return handler.Name().To<VMObjects.AppObject>();
					}

					[SystemCallMethod( "argument-count:0" )]
					public static Handle<VMObjects.AppObject> ArgumentCount( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();

						return new IntHandle( handler.ArgumentCount() );
					}

					[SystemCallMethod( "visibility:0" )]
					public static Handle<VMObjects.AppObject> Visibility( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();
						var vis = AppObject.CreateInstance( visibilityCls );
						vis.SetField( 0, new IntHandle( (int) handler.Visibility() ) );
						return vis;
					}

					[SystemCallMethod( "is-external:0" )]
					public static Handle<VMObjects.AppObject> IsExternal( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();
						return new IntHandle( handler.IsExternal() ? 1 : 0 );
					}

					[SystemCallMethod( "is-default:0" )]
					public static Handle<VMObjects.AppObject> IsDefault( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();
						return new IntHandle( handler.Visibility() == VMILLib.VisibilityModifier.None ? 1 : 0 );
					}

					[SystemCallMethod( "equals:1" )]
					public static Handle<AppObject> Equals( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var handler1 = ((MessageHandlerBase) receiver.GetFieldValue( 0 )).ToHandle();

						if (arguments[0].Class() != messageHandlerCls)
							return new IntHandle( 0 );

						var handler2 = ((MessageHandlerBase) arguments[0].GetFieldValue( 0 )).ToHandle();

						return new IntHandle( handler1 == handler2 ? 1 : 0 );
					}
				}
				#endregion

				#region Class
				[SystemCallClass( "Class" )]
				public static partial class Class {
					[SystemCallMethod( "name:0" )]
					public static Handle<AppObject> Name( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();
						return cls.Name().To<VMObjects.AppObject>();
					}

					[SystemCallMethod( "visibility:0" )]
					public static Handle<AppObject> Visibility( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();
						var vis = AppObject.CreateInstance( visibilityCls );
						vis.SetField( 0, new IntHandle( (int) cls.Visibility() ) );
						return vis;
					}

					[SystemCallMethod( "default-message-handler:0" )]
					public static Handle<AppObject> DefaultMessageHandler( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();
						var def = cls.DefaultHandler();
						if (def == null)
							return new IntHandle( 0 );

						var mirror = AppObject.CreateInstance( messageHandlerCls );
						mirror.SetField( 0, VirtualMachine.InternalObjectClass, def.Start );
						return mirror;
					}

					[SystemCallMethod( "parent-class:0" )]
					public static Handle<AppObject> ParentClass( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						var parentCls = AppObject.CreateInstance( classCls );
						parentCls.SetField( 0, VirtualMachine.InternalObjectClass, parentCls.Start );
						return parentCls;
					}

					[SystemCallMethod( "super-class-names:0" )]
					public static Handle<AppObject> SuperClassNames( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() );

						cls.SuperClasses().ForEach( ( c, i ) => {
							arr.Set( i, c.Start, true );
						} );

						return arr.To<AppObject>();
					}

					[SystemCallMethod( "super-classes:0" )]
					public static Handle<AppObject> SuperClasses( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.SuperClassCount() );

						cls.SuperClasses().ForEach( ( c, i ) => {
							var mirror = AppObject.CreateInstance( classCls );
							mirror.SetField( 0, VirtualMachine.InternalObjectClass, VirtualMachine.ResolveClass( null, c ).Start );
							arr.Set( i, mirror, true );
						} );

						return arr.To<AppObject>();
					}

					[SystemCallMethod( "message-handlers:0" )]
					public static Handle<AppObject> MessageHandlers( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.MessageHandlerCount() );

						cls.MessageHandlers().ForEach( ( h, i ) => {
							var mirror = AppObject.CreateInstance( messageHandlerCls );
							mirror.SetField( 0, VirtualMachine.InternalObjectClass, h.Start );
							arr.Set( i, mirror, true );
						} );

						return arr.To<AppObject>();
					}

					[SystemCallMethod( "inner-classes:0" )]
					public static Handle<AppObject> InnerClasses( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						var arr = VMObjects.Array.CreateInstance( cls.InnerClassCount() );

						cls.InnerClasses().ForEach( ( c, i ) => {
							var mirror = AppObject.CreateInstance( classCls );
							mirror.SetField( 0, VirtualMachine.InternalObjectClass, c.Start );
							arr.Set( i, mirror, true );
						} );

						return arr.To<AppObject>();
					}

					[SystemCallMethod( "equals:1" )]
					public static Handle<AppObject> Equals( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var cls1 = ((VMObjects.Class) receiver.GetFieldValue( 0 )).ToHandle();

						if (arguments[0].Class() != classCls)
							return new IntHandle( 0 );

						var cls2 = ((VMObjects.Class) arguments[0].GetFieldValue( 0 )).ToHandle();

						return new IntHandle( cls1 == cls2 ? 1 : 0 );
					}
				}
				#endregion

				#region Reflector
				[SystemCallClass( "Reflector" )]
				public static partial class Reflector {
					[SystemCallMethod( "classes:0" )]
					public static Handle<AppObject> Classes( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						var classes = VirtualMachine.Classes();

						var arr = VMObjects.Array.CreateInstance( classes.Count() );
						classes.ForEach( ( c, i ) => {
							var mirror = AppObject.CreateInstance( classCls );
							mirror.SetField( 0, VirtualMachine.InternalObjectClass, c.Start );
							arr.Set( i, mirror, true );
						} );

						return arr.To<AppObject>();
					}

					[SystemCallMethod( "find-class:1" )]
					public static Handle<AppObject> FindClass( IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments ) {
						if (arguments[0].Class() != VirtualMachine.StringClass)
							throw new ArgumentException( "Argument must be of type System.String.", "className" );

						var cls = VirtualMachine.ResolveClass( null, arguments[0].To<VMObjects.String>() );
						var mirror = AppObject.CreateInstance( classCls );
						mirror.SetField( 0, VirtualMachine.InternalObjectClass, cls.Start );
						return mirror;
					}
				}
				#endregion
			}
		}
	}
}
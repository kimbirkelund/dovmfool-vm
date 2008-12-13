using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM.Debugging.Service.Server {
	static class ClassReflectionService {
		static int nextId = 1;
		static Dictionary<int, Handle<VMObjects.Class>> idToCls = new Dictionary<int, Handle<VM.VMObjects.Class>>();
		static Dictionary<Handle<VMObjects.Class>, int> clsToId = new Dictionary<Handle<VM.VMObjects.Class>, int>();

		static int Add( Handle<VMObjects.Class> cls ) {
			var id = nextId++;
			idToCls.Add( id, cls );
			clsToId.Add( cls, id );
			return id;
		}

		public static Handle<VMObjects.Class> Get( int id ) {
			Handle<VMObjects.Class> cls;
			if (!idToCls.TryGetValue( id, out cls ))
				throw new System.ArgumentOutOfRangeException( "id" );
			return cls;
		}

		public static int Get( Handle<VMObjects.Class> cls ) {
			int id;
			if (!clsToId.TryGetValue( cls, out id ))
				id = Add( cls );
			return id;
		}

		public static int ResolveClass( string name ) {
			var cls = VirtualMachine.ResolveClass( null, name.ToVMString().ToWeakHandle() ).ToWeakHandle();
			return Get( cls );
		}

		public static string Filename( int id ) {
			return Get( id ).Filename().ToString();
		}

		public static string Name( int id ) {
			return Get( id ).Name().ToString();
		}

		public static VMILLib.VisibilityModifier Visibility( int id ) {
			return Get( id ).Visibility();
		}

		public static string Fullname( int id ) {
			return Get( id ).Fullname().ToString();
		}

		public static int SuperClassCount( int id ) {
			return Get( id ).SuperClassCount();
		}

		public static string[] SuperClassNames( int id ) {
			return Get( id ).SuperClasses().Select( s => s.ToString() ).ToArray();
		}

		public static int[] SuperClasses( int id ) {
			return Get( id ).SuperClasses().Select( s => Get( VirtualMachine.ResolveClass( null, s.ToWeakHandle() ).ToWeakHandle() ) ).ToArray();
		}

		public static int? DefaultMessageHandler( int id ) {
			var cls = Get( id );
			var defh = cls.DefaultHandler();
			return defh.IsNull() ? (int?) null : MessageHandlerReflectionService.Get( defh.ToWeakHandle() );
		}

		public static int MessageHandlerCount( int id ) {
			return Get( id ).MessageHandlerCount();
		}

		public static int[] MessageHandlers( int id ) {
			return Get( id ).MessageHandlers().Select( h => MessageHandlerReflectionService.Get( h.ToWeakHandle() ) ).ToArray();
		}

		public static int InnerClassCount( int id ) {
			return Get( id ).InnerClassCount();
		}

		public static int[] InnerClasses( int id ) {
			return Get( id ).InnerClasses().Select( c => Get( c.ToWeakHandle() ) ).ToArray();
		}

		public static int? ParentClass( int id ) {
			var cls = Get( id );
			return cls.IsNull() ? (int?) null : Get( cls.ParentClass().ToWeakHandle() );
		}

		public static int FieldCount( int id ) {
			return Get( id ).FieldCount();
		}

		internal static int TotalFieldCount( int id ) {
			return Get( id ).TotalFieldCount();
		}
	}
}

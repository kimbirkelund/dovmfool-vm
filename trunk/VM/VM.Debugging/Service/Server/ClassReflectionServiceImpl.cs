using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.Debugging.Service.Server {
	class ClassReflectionServiceImpl : IClassReflectionService {
		public int ResolveClass( string name ) {
			return ClassReflectionService.ResolveClass( name );
		}

		public string Filename( int id ) {
			return ClassReflectionService.Filename( id );
		}

		public string Name( int id ) {
			return ClassReflectionService.Name( id );
		}

		public VMILLib.VisibilityModifier Visibility( int id ) {
			return ClassReflectionService.Visibility( id );
		}

		public string Fullname( int id ) {
			return ClassReflectionService.Fullname( id );
		}

		public int SuperClassCount( int id ) {
			return ClassReflectionService.SuperClassCount( id );
		}

		public string[] SuperClassNames( int id ) {
			return ClassReflectionService.SuperClassNames( id );
		}

		public int[] SuperClasses( int id ) {
			return ClassReflectionService.SuperClasses( id );
		}

		public int? DefaultMessageHandler( int id ) {
			return ClassReflectionService.DefaultMessageHandler( id );
		}

		public int MessageHandlerCount( int id ) {
			return ClassReflectionService.MessageHandlerCount( id );
		}

		public int[] MessageHandlers( int id ) {
			return ClassReflectionService.MessageHandlers( id );
		}

		public int InnerClassCount( int id ) {
			return ClassReflectionService.InnerClassCount( id );
		}

		public int[] InnerClasses( int id ) {
			return ClassReflectionService.InnerClasses( id );
		}

		public int? ParentClass( int id ) {
			return ClassReflectionService.ParentClass( id );
		}

		public int FieldCount( int id ) {
			return ClassReflectionService.FieldCount( id );
		}

		public int TotalFieldCount( int id ) {
			return ClassReflectionService.TotalFieldCount( id );
		}
	}
}

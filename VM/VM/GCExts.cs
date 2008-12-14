using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM.GC {
	public static class Exts {
		static readonly Word BLANK = 0xFFFFFFFC;
		static readonly Word BLACK = 1;
		static readonly Word WHITE = 0;
		static readonly Word GREY = 2;

		public static bool IsBlack( this int i ) { return (VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] & 3) == BLACK; }
		public static bool IsWhite( this int i ) { return (VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] & 3) == WHITE; }
		public static bool IsGrey( this int i ) { return (VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] & 3) == GREY; }

		public static void SetBlack( this int i ) { i.Set( BLACK ); }
		public static void SetWhite( this int i ) { i.Set( WHITE ); }
		public static void SetGrey( this int i ) { i.Set( GREY ); }

		static void Set( this int i, int color ) {
			MarkSweepCompactMemoryManager.DisableAddressVerification();
			var oldObjSize = VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] >> VMObjects.ObjectBase.SIZE_RSHIFT;
			VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] = (VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] & BLANK) | color;
			var newObjSize = VirtualMachine.MemoryManager[i + VMObjects.ObjectBase.SIZE_OFFSET] >> VMObjects.ObjectBase.SIZE_RSHIFT;
			Sekhmet.Assert.AreEqual( oldObjSize, newObjSize );
			MarkSweepCompactMemoryManager.EnableAddressVerification();
		}

		public static IEnumerable<int> GetReferences( this int adr ) {
			var cls = VirtualMachine.MemoryManager[adr + VMObjects.ObjectBase.CLASS_POINTER_OFFSET];
			if (cls < 0)
				cls = KnownClasses.Resolve( cls ).Start;
			if (cls > 0)
				yield return cls;

			if (cls == KnownClasses.System_Reflection_Class.Start)
				foreach (var item in VMObjects.Class.GetReferences( adr ))
					yield return item;
			else if (cls == KnownClasses.System_Reflection_MessageHandler.Start) {
				foreach (var item in VMObjects.MessageHandlerBase.GetReferences( adr ))
					yield return item;
			} else if (cls == KnownClasses.System_Array.Start)
				foreach (var item in VMObjects.Array.GetReferences( adr ))
					yield return item;
			else if (cls == KnownClasses.System_String.Start)
				yield break;
			else
				foreach (var item in VMObjects.AppObject.GetReferences( adr ))
					yield return item;
		}
	}
}
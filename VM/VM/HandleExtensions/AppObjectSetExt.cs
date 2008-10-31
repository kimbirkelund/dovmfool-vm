using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public static class AppObjectSetExt {
		public static uint GetSize( this Handle<AppObjectSet> h ) {
			return h[1];
		}
	}
}

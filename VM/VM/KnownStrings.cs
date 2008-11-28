﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public static class KnownStrings {
		public static readonly Handle<VM.VMObjects.String> initialize_0 = "initialize:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> System_Reflection_MessageHandler = "System.Reflection.MessageHandler".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> Object = "Object".ToVMString().Intern();
	}
}

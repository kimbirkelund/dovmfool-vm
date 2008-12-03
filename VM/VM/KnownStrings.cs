using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	public static class KnownStrings {
		public static readonly Handle<VM.VMObjects.String> initialize_0 = "initialize:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> initialize_1 = "initialize:1".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> initialize_2 = "initialize:2".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> initialize_3 = "initialize:3".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> System_Reflection_MessageHandler = "System.Reflection.MessageHandler".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> Object = "Object".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> is_true_0 = "is-true:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> is_false_0 = "is-false:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> run_0 = "run:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> to_string_0 = "to-string:0".ToVMString().Intern();
		public static readonly Handle<VM.VMObjects.String> message_0 = "message:0".ToVMString().Intern();
	}
}

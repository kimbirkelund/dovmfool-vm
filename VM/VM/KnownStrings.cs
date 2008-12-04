using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM {
	static class KnownStrings {
		public static Handle<VM.VMObjects.String> initialize_0 { get; private set; }
		public static Handle<VM.VMObjects.String> initialize_1 { get; private set; }
		public static Handle<VM.VMObjects.String> initialize_2 { get; private set; }
		public static Handle<VM.VMObjects.String> initialize_3 { get; private set; }
		public static Handle<VM.VMObjects.String> System_Reflection_MessageHandler { get; private set; }
		public static Handle<VM.VMObjects.String> Object { get; private set; }
		public static Handle<VM.VMObjects.String> is_true_0 { get; private set; }
		public static Handle<VM.VMObjects.String> is_false_0 { get; private set; }
		public static Handle<VM.VMObjects.String> run_0 { get; private set; }
		public static Handle<VM.VMObjects.String> to_string_0 { get; private set; }
        public static Handle<VM.VMObjects.String> message_0 { get; private set; }
        public static Handle<VM.VMObjects.String> equals_1 { get; private set; }

		public static void Initialize() {
			initialize_0 = "initialize:0".ToVMString().Intern();
			initialize_1 = "initialize:1".ToVMString().Intern();
			initialize_2 = "initialize:2".ToVMString().Intern();
			initialize_3 = "initialize:3".ToVMString().Intern();
			System_Reflection_MessageHandler = "System.Reflection.MessageHandler".ToVMString().Intern();
			Object = "Object".ToVMString().Intern();
			is_true_0 = "is-true:0".ToVMString().Intern();
			is_false_0 = "is-false:0".ToVMString().Intern();
			run_0 = "run:0".ToVMString().Intern();
			to_string_0 = "to-string:0".ToVMString().Intern();
			message_0 = "message:0".ToVMString().Intern();
            equals_1 = "equals:1".ToVMString().Intern();
		}
	}
}

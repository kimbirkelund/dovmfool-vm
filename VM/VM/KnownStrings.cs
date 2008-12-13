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
		public static Handle<VM.VMObjects.String> external_call_0 { get; private set; }
		public static Handle<VM.VMObjects.String> Dot { get; private set; }
		public static Handle<VM.VMObjects.String> Empty { get; private set; }

		public static void Initialize() {
			initialize_0 = "initialize:0".ToVMString().Intern().ToHandle();
			initialize_1 = "initialize:1".ToVMString().Intern().ToHandle();
			initialize_2 = "initialize:2".ToVMString().Intern().ToHandle();
			initialize_3 = "initialize:3".ToVMString().Intern().ToHandle();
			System_Reflection_MessageHandler = "System.Reflection.MessageHandler".ToVMString().Intern().ToHandle();
			Object = "Object".ToVMString().Intern().ToHandle();
			is_true_0 = "is-true:0".ToVMString().Intern().ToHandle();
			is_false_0 = "is-false:0".ToVMString().Intern().ToHandle();
			run_0 = "run:0".ToVMString().Intern().ToHandle();
			to_string_0 = "to-string:0".ToVMString().Intern().ToHandle();
			message_0 = "message:0".ToVMString().Intern().ToHandle();
			equals_1 = "equals:1".ToVMString().Intern().ToHandle();
			external_call_0 = "external-call:0".ToVMString().Intern().ToHandle();
			Dot = ".".ToVMString().Intern().ToHandle();
			Empty = "".ToVMString().Intern().ToHandle();
		}

		public static void Dispose() {
			initialize_0.Dispose();
			initialize_1.Dispose();
			initialize_2.Dispose();
			initialize_3.Dispose();
			System_Reflection_MessageHandler.Dispose();
			Object.Dispose();
			is_true_0.Dispose();
			is_false_0.Dispose();
			run_0.Dispose();
			to_string_0.Dispose();
			message_0.Dispose();
			equals_1.Dispose();
			external_call_0.Dispose();
			Dot.Dispose();
			Empty.Dispose();
		}
	}
}

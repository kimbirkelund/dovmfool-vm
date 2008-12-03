using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VM.VMObjects;

namespace VM
{
    public static class KnownStrings
    {
        public static readonly Handle<VM.VMObjects.String> initialize_0;
        public static readonly Handle<VM.VMObjects.String> initialize_1;
        public static readonly Handle<VM.VMObjects.String> initialize_2;
        public static readonly Handle<VM.VMObjects.String> initialize_3;
        public static readonly Handle<VM.VMObjects.String> System_Reflection_MessageHandler;
        public static readonly Handle<VM.VMObjects.String> Object;
        public static readonly Handle<VM.VMObjects.String> is_true_0;
        public static readonly Handle<VM.VMObjects.String> is_false_0;
        public static readonly Handle<VM.VMObjects.String> run_0;
        public static readonly Handle<VM.VMObjects.String> to_string_0;
        public static readonly Handle<VM.VMObjects.String> message_0;

        static KnownStrings()
        {
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
        }
    }
}

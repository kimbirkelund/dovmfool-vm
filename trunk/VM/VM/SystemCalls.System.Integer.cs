using System.Linq;
using System.Text;

namespace VM
{
    partial class SystemCalls
    {
        partial class System
        {
            [SystemCallClass("Integer")]
            class Integer
            {
                [SystemCallMethod("to-string:0")]
                public static Handle<VMObjects.AppObject> ToString(IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments)
                {
                       return VM.VirtualMachine.ConstantPool.RegisterString(((IntHandle)receiver).Value.ToString()).To<VMObjects.AppObject>();
                }

                [SystemCallMethod("subtract:1")]
                public static Handle<VMObjects.AppObject> subtract(IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments)
                {
                    var other = ((IntHandle)arguments[0]).Value;
                    var value = ((IntHandle)receiver).Value - other;

                    return new IntHandle(value);
                }

                [SystemCallMethod("add:1")]
                public static Handle<VMObjects.AppObject> add(IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments)
                {
                    var other = ((IntHandle)arguments[0]).Value;
                    var value = ((IntHandle)receiver).Value + other;

                    return new IntHandle(value);
                }

                [SystemCallMethod("multiply:1")]
                public static Handle<VMObjects.AppObject> multiply(IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments)
                {
                    var other = ((IntHandle)arguments[0]).Value;
                    var value = ((IntHandle)receiver).Value * other;

                    return new IntHandle(value);
                }

                [SystemCallMethod("divide:1")]
                public static Handle<VMObjects.AppObject> divide(IInterpretor interpretor, Handle<VMObjects.AppObject> receiver, Handle<VMObjects.AppObject>[] arguments)
                {
                    var other = ((IntHandle)arguments[0]).Value;
                    var value = ((IntHandle)receiver).Value / other;

                    return new IntHandle(value);
                }
            }
        }
    }
}
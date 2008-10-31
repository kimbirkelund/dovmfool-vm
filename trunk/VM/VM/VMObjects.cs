using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VM {
	public class VMObject { protected VMObject() { } }


	public class InternalObject : VMObject { public const uint TypeId = Class.TypeId; protected InternalObject() { } }

	public class Class : InternalObject { public new const uint TypeId = 4; protected Class() { } }

	public class ClassManager : InternalObject { public new const uint TypeId = 5; protected ClassManager() { } }

	public class MessageHandler : InternalObject { public new const uint TypeId = 6; protected MessageHandler() { } }

	public class DelegateMessageHandler : InternalObject { public new const uint TypeId = 7; protected DelegateMessageHandler() { } }

	public class MessageHandlerWrapper : InternalObject { public new const uint TypeId = 8; protected MessageHandlerWrapper() { } }

	public class ConstantPool : InternalObject { public new const uint TypeId = 9; protected ConstantPool() { } }


	public class AppObject : VMObject { public const uint TypeId = 0; protected AppObject() { } }

	public class AppObjectSet : VMObject { public const uint TypeId = 1; protected AppObjectSet() { } }

	public class String : VMObject { public const uint TypeId = 2; protected String() { } }

	public class Integer : VMObject { public const uint TypeId = 3; protected Integer() { } }
}

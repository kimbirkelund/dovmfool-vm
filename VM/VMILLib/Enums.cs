using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public enum OpCode {
		None = -1,
		StoreField = 0,
		LoadField = 1,
		StoreLocal = 2,
		LoadLocal = 3,
		LoadArgument = 4,
		PushLiteral = -2,
		PushLiteralString = 5,
		PushLiteralIntInline = 6,
		PushLiteralInt = 7,
		Pop = 8,
		Dup = 9,
		NewInstance = 10,
		SendMessage = 11,
		ReturnVoid = 12,
		Return = 13,
		Jump = 14,
		JumpIfTrue = 15,
		JumpIfFalse = 16,
		Throw = 17,
		Try = 18,
		Catch = 19,
		EndTryCatch = 20
	}

	public enum VisibilityModifier {
		Public = 0,
		Protected = 1,
		Private = 2,
		None = 3
	}

	enum InternalObjectType : uint {
		Class = 4,
		DelegateMessageHandler = 5,
		VMILMessageHandler = 6
	}
}

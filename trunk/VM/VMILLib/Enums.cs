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
		PushLiteral = 4,
		Pop = 5,
		NewInstance = 6,
		SendMessage = 7,
		ReturnVoid = 8,
		Return = 9,
		Jump = 10,
		JumpIfTrue = 11,
		JumpIfFalse = 12,
		Throw = 13,
		Try = 14,
		Catch = 15,
		EndTryCatch = 16
	}

	public enum VisibilityModifier {
		Public = 0,
		Protected = 1,
		Private = 2
	}

	enum InternalObjectType : uint {
		Class = 4,
		DelegateMessageHandler = 5,
		VMILMessageHandler = 6
	}
}

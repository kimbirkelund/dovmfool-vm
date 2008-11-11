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
		Dup = 6,
		NewInstance = 7,
		SendMessage = 8,
		ReturnVoid = 9,
		Return = 10,
		Jump = 11,
		JumpIfTrue = 12,
		JumpIfFalse = 13,
		Throw = 14,
		Try = 15,
		Catch = 16,
		EndTryCatch = 17
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

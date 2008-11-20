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
		LoadThis = -3,
		PushLiteral = -2,
		PushLiteralString = 5,
		PushLiteralInt = 6,
		PushLiteralIntExtend = 7,
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

	public enum TypeId : int {
		Null = 0,
		AppObject = 1,
		AppObjectSet = 2,
		String = 3,
		Integer = 4,
		Char = 5,
		Class = 6,
		DelegateMessageHandler = 7,
		VMILMessageHandler = 8,
		ClassManager = 9,
		List = 10,
		Array = 11
	}
}

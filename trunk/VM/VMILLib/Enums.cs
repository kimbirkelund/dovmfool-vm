using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public enum OpCode {
		None = 0,
		StoreField,
		LoadField,
		StoreLocal,
		LoadLocal,
		PushLiteral,
		Pop,
		NewInstance,
		ReturnVoid,
		Return,
		Jump,
		JumpIfTrue,
		JumpIfFalse,
		Throw,
		Try,
		Catch,
		EndTryCatch
	}

	public enum VisibilityModifier {
		Public = 0,
		Protected = 1,
		Private = 2
	}
}

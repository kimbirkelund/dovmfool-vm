using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILAssembler {
	enum Tokens {
		Class,
		Extends,
		Fields,
		Handler,
		Default,
		Locals,

		StoreField,
		LoadField,
		StoreLocal,
		LoadLocal,
		PushLiteral,
		Pop,
		NewInstance,
		Return,
		Jump,
		JumpIfTrue,
		JumpIfFalse,
		Throw,
		Try,
		Catch,


	}
}

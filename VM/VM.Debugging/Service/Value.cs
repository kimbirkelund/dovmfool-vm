using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using VM.VMObjects;

namespace VM.Debugging.Service {
	[DataContract]
	class Value {
		[DataMember]
		public ValueType Type { get; set; }
		[DataMember]
		public int Class { get; set; }
		[DataMember]
		public int Data { get; set; }
	}

	enum ValueType {
		Object,
		Integer,
		ReturnAddress,
		ReturnPC,
		OldBasePointer,
		OldFrameBoundary,
		TryMarker,
		DoActualReturnHereMarker
	}

	static class ExtValue {
		public static Value ToValue( this UValue val ) {
			if (val.Type.Start < 0) {
				switch (val.Type.Start) {
					case ExecutionStack.TYPE_ACTUAL_RETURN_HERE:
						return new Value { Type = ValueType.DoActualReturnHereMarker, Data = val.Value };
					case ExecutionStack.TYPE_BASE_POINTER:
						return new Value { Type = ValueType.OldBasePointer, Data = val.Value };
					case ExecutionStack.TYPE_FRAME_BOUNDARY:
						return new Value { Type = ValueType.OldFrameBoundary, Data = val.Value };
					case ExecutionStack.TYPE_RETURN_HANDLER:
						return new Value { Type = ValueType.ReturnAddress, Data = val.Value };
					case ExecutionStack.TYPE_RETURN_INSTRUCTION_OFFSET:
						return new Value { Type = ValueType.ReturnPC, Data = val.Value };
					case ExecutionStack.TYPE_TRY:
						return new Value { Type = ValueType.TryMarker, Data = val.Value };
					default:
						throw new System.ArgumentException( "Argument has unknown type.", "val" );
				}
			}
			if (val.IsNull)
				return null;
			if (val.IsVoid)
				throw new System.ArgumentException( "Argument can not be a void value.", "val" );
			if (!val.IsReference)
				return new Value { Type = ValueType.Integer, Data = val.Value };

			var h = val.ToHandle();
			return new Value {
				Type = ValueType.OldBasePointer,
				Class = VM.Debugging.Service.Server.ClassReflectionService.Get( h.Class().ToHandle() ),
				Data = VM.Debugging.Service.Server.ObjectReflectionService.Get( h )
			};
		}
	}
}

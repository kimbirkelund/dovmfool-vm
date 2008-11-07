using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VMILLib {
	public class InstructionList : IEnumerable<Instruction> {
		List<Instruction> instructions;

		public Instruction this[int index] {
			get { return instructions[index]; }
		}

		public readonly int Count;

		public InstructionList( IEnumerable<Instruction> instructions ) {
			this.instructions = instructions.ToList();
			this.Count = this.instructions.Count;
		}

		public IEnumerator<Instruction> GetEnumerator() {
			return instructions.GetEnumerator();
		}
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { return GetEnumerator(); }
	}
}

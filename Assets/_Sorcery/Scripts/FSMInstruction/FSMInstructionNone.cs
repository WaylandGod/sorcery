using UnityEngine;
using System.Collections;

namespace Sorcery 
{
	public class FSMInstructionNone : FSMInstruction {
		public override Instruction instruction {
			get {
				return Instruction.NONE;
			}
		}
	}
}
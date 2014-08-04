using UnityEngine;
using System.Collections;

namespace Sorcery 
{
	public class FSMInstructionMove : FSMInstruction {
		public override Instruction instruction {
			get {
				return Instruction.MOVE;
			}
		}
	}
}

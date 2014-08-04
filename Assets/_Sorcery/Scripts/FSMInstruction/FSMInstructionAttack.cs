using UnityEngine;
using System.Collections;

namespace Sorcery
{
	public class FSMInstructionAttack : FSMInstruction
	{
		public override Instruction instruction 
		{
			get 
			{
				return Instruction.ATTACK;
			}
		}
	}
}

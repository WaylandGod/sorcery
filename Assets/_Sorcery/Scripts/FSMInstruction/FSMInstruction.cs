using UnityEngine;
using System.Collections;

namespace Sorcery
{
	public enum Instruction
	{
		INVALID = -1,

		NONE,
		MOVE,
		ATTACK
	}

	public abstract class FSMInstruction
	{
		public virtual Instruction instruction
		{
			get 
			{
				return Instruction.INVALID;
			}
		}
	}
}

using UnityEngine;
using System.Collections;
using NodeCanvas;
using NodeCanvas.Variables;

namespace Sorcery
{
	[Category("_✫ FSMCondition")]
	public class FSMCondition : ConditionTask
	{
		public BBVar m_value = new BBVar{blackboardOnly = true};
		public Instruction m_instruction = Instruction.INVALID;

		protected override bool OnCheck ()
		{
			if(m_value.value == null)
				return false;

			FSMInstruction instruction = m_value.value as FSMInstruction;
			return instruction.instruction == m_instruction;
		}

		protected override string info {
			get {
				return m_instruction.ToString();
			}
		}
	}
}

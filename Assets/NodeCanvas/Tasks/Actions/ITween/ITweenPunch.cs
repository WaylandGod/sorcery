using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenPunch : ITweenActions {

		public enum PunchType
		{
			PunchPosition,
			PunchRotation,
			PunchScale
		}

		public PunchType punchType = PunchType.PunchPosition;
		public BBVector amount;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + punchType.ToString() + amount;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"amount", amount.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (punchType == PunchType.PunchPosition)
				iTween.PunchPosition(agent.gameObject, hash);
			else if (punchType == PunchType.PunchRotation)
				iTween.PunchRotation(agent.gameObject, hash);
			else
				iTween.PunchScale(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
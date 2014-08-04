using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenMove : ITweenActions {

		public enum MoveType
		{
			MoveTo,
			MoveFrom
		}

		public MoveType moveType = MoveType.MoveTo;
		public BBVector targetPosition;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + moveType.ToString() + targetPosition;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"position", targetPosition.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (moveType == MoveType.MoveTo)
				iTween.MoveTo(agent.gameObject, hash);
			else
				iTween.MoveFrom(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
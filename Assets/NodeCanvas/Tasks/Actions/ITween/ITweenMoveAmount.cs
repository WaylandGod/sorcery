using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenMoveAmount : ITweenActions {

		public enum MoveType
		{
			MoveBy,
			MoveAdd
		}

		public MoveType moveType = MoveType.MoveBy;
		public BBVector amount;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + moveType.ToString() + amount;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"amount", amount.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (moveType == MoveType.MoveBy)
				iTween.MoveBy(agent.gameObject, hash);
			else
				iTween.MoveAdd(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
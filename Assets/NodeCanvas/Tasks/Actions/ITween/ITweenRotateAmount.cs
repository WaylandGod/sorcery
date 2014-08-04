using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenRotateAmount : ITweenActions {

		public enum RotateType
		{
			RotateBy,
			RotateAdd
		}

		public RotateType rotateType = RotateType.RotateBy;
		public BBVector amount;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + rotateType.ToString() + amount;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"amount", amount.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (rotateType == RotateType.RotateBy)
				iTween.RotateBy(agent.gameObject, hash);
			else
				iTween.RotateAdd(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
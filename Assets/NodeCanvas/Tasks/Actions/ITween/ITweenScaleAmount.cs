using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenScaleAmount : ITweenActions {

		public enum ScaleType
		{
			ScaleBy,
			ScaleAdd
		}

		public ScaleType rotateType = ScaleType.ScaleBy;
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

			if (rotateType == ScaleType.ScaleBy)
				iTween.ScaleBy(agent.gameObject, hash);
			else
				iTween.ScaleAdd(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
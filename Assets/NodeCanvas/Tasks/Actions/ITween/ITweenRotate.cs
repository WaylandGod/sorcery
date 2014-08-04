using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenRotate : ITweenActions {

		public enum RotateType
		{
			RotateTo,
			RotateFrom
		}

		public RotateType rotateType = RotateType.RotateTo;
		public BBVector targetRotation;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + rotateType.ToString() + targetRotation;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"rotation", targetRotation.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (rotateType == RotateType.RotateTo)
				iTween.RotateTo(agent.gameObject, hash);
			else
				iTween.RotateFrom(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
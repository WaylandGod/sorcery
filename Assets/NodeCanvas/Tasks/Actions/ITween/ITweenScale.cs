using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenScale : ITweenActions {

		public enum ScaleType
		{
			ScaleTo,
			ScaleFrom
		}

		public ScaleType scaleType = ScaleType.ScaleTo;
		public BBVector targetScale;

		private Hashtable hash;

		protected override string info{
			get {return "Tween " + scaleType.ToString() + targetScale;}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"scale", targetScale.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (scaleType == ScaleType.ScaleTo)
				iTween.ScaleTo(agent.gameObject, hash);
			else
				iTween.ScaleFrom(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	public class ITweenLook : ITweenActions {

		public enum LookType
		{
			LookTo,
			LookFrom
		}

		public LookType lookType = LookType.LookTo;
		public BBVector targetPosition;

		private Hashtable hash;

		protected override string info{
			get {return "ITween " + lookType.ToString();}
		}

		protected override void OnExecute(){

			hash = iTween.Hash(
				"looktarget", targetPosition.value,
				"name", id.value,
				"delay", delay.value,
				"time", time.value,
				"easetype", easeType
			);

			if (lookType == LookType.LookTo)
				iTween.LookTo(agent.gameObject, hash);
			else
				iTween.LookFrom(agent.gameObject, hash);

			base.OnExecute();
		}
	}
}
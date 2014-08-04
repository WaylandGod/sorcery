﻿using UnityEngine;
using NodeCanvas.Variables;

namespace NodeCanvas.StateMachines{

	[Category("My States")]
	[Icon("log")] //Icon must be in a Resources folder
	public class SampleState : FSMState {

		public BBFloat timeout;
		private float timer;

		//When the FSM itself starts
		protected override void Init(){
			Debug.Log("Init");
		}

		//When the status is entered. Not when it is resumed though
		protected override void Enter(){
			Debug.Log("Enter");
		}

		//As long as the status is active
		protected override void Stay(){
			timer += Time.deltaTime;
			if (timer >= timeout.value)
				Finish();
		}

		//When the status was active and another status entered thus this exits. Also when the whole FSM stops.
		protected override void Exit(){
			Debug.Log("Exit");
			timer = 0;
		}

		//When the status was active and FSM paused
		protected override void Pause(){
			Debug.Log("Pause");
		}
	}
}
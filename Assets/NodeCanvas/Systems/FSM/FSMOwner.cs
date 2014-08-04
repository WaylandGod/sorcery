using UnityEngine;
using System.Collections.Generic;

namespace NodeCanvas.StateMachines{

	///This class essentially a front-end to executing a FSM (FSM)
	[AddComponentMenu("NodeCanvas/FSM Owner")]
	public class FSMOwner : GraphOwner {

		public FSM FSM;

		public override Graph behaviour{
			get{return FSM;}
			set {FSM = (FSM)value;}
		}

		public override System.Type graphType{
			get {return typeof(FSM);}
		}

		///The current status name
		public string currentStateName{
			get {return FSM != null? FSM.currentStateName : null;}
		}

		///The last status name
		public string lastStateName{
			get {return FSM != null? FSM.lastStateName : null;}
		}

		///Enter an FSM Status by it's name
		public FSMState TriggerState(string stateName){

			if (FSM != null)
				return FSM.TriggerState(stateName);
			return null;
		}

		///Get all status names, excluding non-named states
		public List<string> GetStateNames(){
			if (FSM != null)
				return FSM.GetStateNames();
			return null;
		}
	}
}
using UnityEngine;
using System.Collections;

namespace NodeCanvas.StateMachines{

	[AddComponentMenu("")]
	[Name("Any State")]
	[Description("The Transitions of this node will constantly be checked. If any becomes true, the target connected State will Enter regardless of the current State. This node can have no incomming transitions.")]
	public class FSMAnyState : FSMState{

		public override string nodeName{
			get {return string.Format("<color=#b3ff7f>{0}</color>", base.nodeName.ToUpper());}
		}

		public override int maxInConnections{
			get {return 0;}
		}

		public override int maxOutConnections{
			get{return -1;}
		}

		public override bool allowAsPrime{
			get {return false;}
		}

		public void UpdateAnyState(){

			if (outConnections.Count == 0)
				return;

			status = Status.Running;

			for (int i = 0; i < outConnections.Count; i++){

				var connection = outConnections[i] as FSMConnection;
				if (connection.condition == null)
					continue;

				if (connection.CheckCondition(graphAgent, graphBlackboard)){
					FSM.EnterState(connection.targetNode as FSMState);
					return;
				}
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeInspectorGUI(){

			base.OnNodeInspectorGUI();

			var emptyFound = false;
			foreach(FSMConnection connection in outConnections){
				if (connection.condition == null)
					emptyFound = true;
			}

			if (emptyFound)
				UnityEditor.EditorGUILayout.HelpBox("This is not a state and as such it never finish and no OnFinish transitions are ever called. Add a condition in all transitions of this node", UnityEditor.MessageType.Warning);
		}

		#endif
	}
}
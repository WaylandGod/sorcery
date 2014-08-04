using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Set")]
	[Category("Decorators")]
	[Description("Set another Agent for the rest of the Tree dynamicaly from this point and on. All nodes under this will be executed for the new agent")]
	[Icon("Set")]
	public class BTSetter : BTDecorator{

		public BBGameObject agentToSet= new BBGameObject();

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (!decoratedConnection)
				return Status.Resting;

			if (agentToSet.value != null)
				agent = agentToSet.value.transform;

			return decoratedConnection.Execute(agent, blackboard);
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			if (!string.IsNullOrEmpty(agentToSet.dataName) || agentToSet.value != null)
				GUILayout.Label("Agent " + agentToSet);
			else
				GUILayout.Label("Identity Agent");
		}

		#endif
	}
}
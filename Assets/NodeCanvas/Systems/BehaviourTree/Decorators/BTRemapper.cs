#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Remap")]
	[Category("Decorators")]
	[Description("Remap the child node's status to another status. Used to either invert the child's return status or to always return a specific status.")]
	[Icon("Remap")]
	public class BTRemapper : BTDecorator{

		public enum RemapStates
		{
			Failure  = 0,
			Success  = 1,
			Inactive = 3
		}

		public RemapStates successRemap = RemapStates.Success;
		public RemapStates failureRemap = RemapStates.Failure;

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (!decoratedConnection)
				return Status.Resting;
			
			status = decoratedConnection.Execute(agent, blackboard);
			
			if (status == Status.Success){

				if (successRemap == RemapStates.Inactive)
					decoratedConnection.ResetConnection();

				return (Status)successRemap;

			} else if (status == Status.Failure){

				if (successRemap == RemapStates.Inactive)
					decoratedConnection.ResetConnection();

				return (Status)failureRemap;
			}

			return status;
		}

		/////////////////////////////////////////
		/////////GUI AND EDITOR STUFF////////////
		/////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeGUI(){

			if ((int)successRemap != (int)Status.Success)
				GUILayout.Label("Success = " + successRemap);

			if ((int)failureRemap != (int)Status.Failure)
				GUILayout.Label("Failure = " + failureRemap);

			GUILayout.Label("", GUILayout.Height(1));
		}

		#endif
	}
}
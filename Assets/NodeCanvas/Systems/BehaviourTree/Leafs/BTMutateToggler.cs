using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.BehaviourTrees{

	[Category("Mutators (beta)")]
	[Name("Toggler")]
	[Description("Enable, Disable or Toggle one or more nodes with provided tag. In practise their incomming connections are disabled\nBeta Feature!")]
	public class BTMutateToggler : BTNodeBase {

		public enum Mode
		{
			Enable,
			Disable,
			Toggle
		}

		public Mode mode = Mode.Toggle;
		public string targetNodeTag;
		
		private List<Node> targetNodes;

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			targetNodes = graph.GetNodesWithTag<Node>(targetNodeTag);

			if (targetNodes.Count != 0){

				if (mode == Mode.Enable){
					foreach (Node node in targetNodes)
						node.inConnections[0].isDisabled = false;
				}

				if (mode == Mode.Disable){
					foreach (Node node in targetNodes)
						node.inConnections[0].isDisabled = true;
				}

				if (mode == Mode.Toggle){
					foreach (Node node in targetNodes)
						node.inConnections[0].isDisabled = !node.inConnections[0].isDisabled;
				}

				return Status.Success;
			}

			return Status.Failure;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			
			GUILayout.Label(string.Format("{0} '{1}'", mode.ToString(), targetNodeTag));
		}

		protected override void OnNodeInspectorGUI(){

			targetNodeTag = EditorUtils.StringPopup("Node Tag", targetNodeTag, graph.GetAllTagedNodes<Node>().Select(n => n.tagName).ToList());
			mode = (Mode)UnityEditor.EditorGUILayout.EnumPopup("Mode", mode);
		}
		
		#endif
	}
}
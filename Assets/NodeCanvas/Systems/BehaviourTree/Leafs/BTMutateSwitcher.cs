using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.BehaviourTrees{

	[Category("Mutators (beta)")]
	[Name("Switcher")]
	[Description("Switch the root node of the behaviour tree to a new one defined by tag\nBeta Feature!")]
	public class BTMutateSwitcher : BTNodeBase {

		public string targetNodeTag;
		private Node targetNode;

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			targetNode = graph.GetNodeWithTag<Node>(targetNodeTag);

			if (targetNode != null ){
				if (graph.primeNode != targetNode)
					graph.primeNode = targetNode;
				return Status.Success;
			}

			return Status.Failure;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			
			GUILayout.Label("Switch To '" + targetNodeTag + "'");
		}

		protected override void OnNodeInspectorGUI(){

			targetNodeTag = EditorUtils.StringPopup("Node Tag", targetNodeTag, graph.GetAllTagedNodes<Node>().Select(n => n.tagName).ToList());
		}

		#endif
	}
}
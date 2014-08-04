using UnityEngine;
using System.Collections;

namespace NodeCanvas.DialogueTrees{

	[AddComponentMenu("")]
	[Name("End")]
	[Description("End the dialogue in Success or Failure.\nNote: A Dialogue will anyway End in Succcess if it has reached a node with no out connections.")]
	public class DLGEndNode : DLGNodeBase {

		public enum EndState{
			Failure = 0,
			Success = 1
		}

		public EndState endState = EndState.Success;

		public override string nodeName{
			get {return "END";}
		}

		public override int maxOutConnections{
			get {return 0;}
		}

		protected override Status OnExecute(){

			DLGTree.currentNode = this;
			status = (Status)endState;
			DLGTree.StopGraph();
			return status;
		}


		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){
			GUILayout.Label("<b>" + endState + "</b>");
		}

		protected override void OnNodeInspectorGUI(){
			DrawDefaultInspector();
		}

		#endif
	}
}
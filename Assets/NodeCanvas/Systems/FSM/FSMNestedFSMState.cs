#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

namespace NodeCanvas.StateMachines{

	[AddComponentMenu("")]
	[Name("FSM")]
	[Category("Nested")]
	[Description("Execute a nested FSM OnEnter and Stop that FSM OnExit. This state is Finished when the nested FSM is finished as well")]
	public class FSMNestedFSMState : FSMState, INestedNode{

		[SerializeField]
		private FSM _nestedFSM;
		private bool instanceChecked;

		private FSM nestedFSM{
			get {return _nestedFSM;}
			set {_nestedFSM = value;}
		}

		[HideInInspector]
		public Graph nestedGraph{
			get {return nestedFSM;}
			set {nestedFSM = (FSM)value;}
		}

		protected override void Enter(){

			if (!nestedFSM){
				Finish(false);
				return;
			}

			CheckInstance();
			nestedFSM.StartGraph(graphAgent, graphBlackboard, Finish);
		}

		protected override void Exit(){
			if (nestedFSM && nestedFSM.isRunning)
				nestedFSM.StopGraph();
		}

		protected override void Pause(){
			if (nestedFSM)
				nestedFSM.PauseGraph();
		}

		private void CheckInstance(){

			if (!instanceChecked && nestedFSM != null && nestedFSM.transform.parent != graph.transform){
				nestedFSM = (FSM)Instantiate(nestedFSM, transform.position, transform.rotation);
				nestedFSM.transform.parent = graph.transform;
				instanceChecked = true;
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			base.OnNodeGUI();
			if (nestedFSM){

				GUILayout.Label("Nested FSM");

			} else {
				if (GUILayout.Button("CREATE NEW"))
					nestedFSM = (FSM)Graph.CreateNested(this, typeof(FSM), "Nested FSM");
			}
		}

		protected override void OnNodeInspectorGUI(){

			base.OnNodeInspectorGUI();
			nestedFSM = EditorGUILayout.ObjectField("FSM", nestedFSM, typeof(FSM), true) as FSM;
			if (nestedFSM == this.FSM){
				Debug.LogWarning("Nested FSM can't be itself!");
				nestedFSM = null;
			}

			if (nestedFSM != null)
				nestedFSM.graphName = nodeName;
		}
		
		#endif
	}
}
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
namespace NodeCanvas.DialogueTrees{

	[AddComponentMenu("")]
	[Name("Action")]
	[Description("Execute an Action Task for the Dialogue Actor selected. The Blackboard will be taken from the selected Actor.")]
	public class DLGActionNode : DLGNodeBase, ITaskAssignable{

		[SerializeField]
		private ActionTask _action;

		[HideInInspector]
		public Task task{
			get {return action;}
			set {action = (ActionTask)value;}
		}

		private ActionTask action{
			get {return _action;}
			set
			{
				_action = value;
				if (_action != null)
					_action.SetOwnerSystem(this);
			}
		}

		public override string nodeName{
			get{return base.nodeName + " " + finalActorName;}
		}

		protected override Status OnExecute(){

			if (!finalActor){
				DLGTree.StopGraph();
				return Error("Actor not found");
			}

			if (!action){
				OnActionEnd(true);
				return Status.Success;
			}

			DLGTree.currentNode = this;

			status = Status.Running;
			action.ExecuteAction(finalActor, finalBlackboard, OnActionEnd);
			return status;
		}

		private void OnActionEnd(System.ValueType success){

			if ( (bool)success ){
				Continue();
				return;
			}

			status = Status.Failure;
			DLGTree.StopGraph();
		}

		protected override void OnReset(){
			if (action)
				action.EndAction(false);
		}

		public override void OnGraphPaused(){
			if (action)
				action.PauseAction();
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			base.OnNodeGUI();
			GUILayout.Label(action? action.taskInfo : "No Action");
		}

		protected override void OnNodeInspectorGUI(){
			
			base.OnNodeInspectorGUI();

			if (action == null){
				EditorUtils.TaskSelectionButton(gameObject, typeof(ActionTask), delegate(Task a){action = (ActionTask)a;} );
			} else {
				EditorUtils.TaskTitlebar(action);
			}
		}

		#endif
	}
}
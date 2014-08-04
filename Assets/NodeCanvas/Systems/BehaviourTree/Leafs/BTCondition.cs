using UnityEngine;
using System.Collections;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Condition")]
	[Description("Check a condition and return Success or Failure")]
	[Icon("Condition")]
	public class BTCondition : BTNodeBase, ITaskAssignable{

		[SerializeField]
		private ConditionTask _condition;

		[SerializeField]
		private BTCondition _referencedNode;

		public Task task{
			get {return condition;}
			set {condition = (ConditionTask)value;}
		}
/*
		public override System.Type outConnectionType{
			get {return typeof(Connection);}
		}

		public override int maxOutConnections{
			get {return 1;}
		}
*/
		private ConditionTask condition{
			get
			{
				if (referencedNode != null)
					return referencedNode.condition;
				return _condition;
			}
			set
			{
				_condition = value;
				if (_condition != null)
					_condition.SetOwnerSystem(graph);
			}
		}

		public BTCondition referencedNode{
			get {return _referencedNode;}
			private set {_referencedNode = value;}
		}

		public override string nodeName{
			get{return base.nodeName.ToUpper();}
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (condition){
				if (outConnections.Count == 0)
					return condition.CheckCondition(agent, blackboard)? Status.Success: Status.Failure;
				if (condition.CheckCondition(agent, blackboard))
					return outConnections[0].Execute(agent, blackboard);
				outConnections[0].ResetConnection();
			}

			return Status.Failure;
		}

		/////////////////////////////////////////
		/////////GUI AND EDITOR STUFF////////////
		/////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeGUI(){
			
	        Rect markRect = new Rect(nodeRect.width - 15, 5, 15, 15);
	        if (referencedNode != null)
	        	GUI.Label(markRect, "<b>R</b>");

			if (condition == null) GUILayout.Label("No Condition");
			else GUILayout.Label(condition.taskInfo);
		}

		protected override void OnNodeInspectorGUI(){

			if (referencedNode != null){
				if (GUILayout.Button("Select Reference"))
					Graph.currentSelection = referencedNode;

				if (GUILayout.Button("Break Reference"))
					BreakReference();

				if (condition != null){
					GUILayout.Label("<b>" + condition.taskName + "</b>");
					condition.ShowInspectorGUI();
				}
				return;
			}

			if (!condition){
				EditorUtils.TaskSelectionButton(gameObject, typeof(ConditionTask), delegate(Task c){condition = (ConditionTask)c;});
			} else {
				EditorUtils.TaskTitlebar(condition);
			}
		}

		protected override void OnContextMenu(UnityEditor.GenericMenu menu){
			menu.AddItem (new GUIContent ("Duplicate (Reference)"), false, DuplicateReference);
		}
		
		private void DuplicateReference(){
			var newNode = graph.AddNewNode(typeof(BTCondition)) as BTCondition;
			newNode.nodeRect.center = this.nodeRect.center + new Vector2(50, 50);
			newNode.referencedNode = referencedNode != null? referencedNode : this;
		}

		public void BreakReference(){

			if (referencedNode == null)
				return;

			if (referencedNode.condition != null)
				condition = (ConditionTask)referencedNode.condition.CopyTo(this.gameObject);

			referencedNode = null;
		}

		#endif
	}
}
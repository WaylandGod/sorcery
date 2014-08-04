#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

namespace NodeCanvas{

	[AddComponentMenu("")]
	///A connection that holds a Condition Task
	public class ConditionalConnection : Connection, ITaskAssignable{

		[SerializeField]
		private ConditionTask _condition;

		public Task task{
			get {return condition;}
			set {condition = (ConditionTask)value;}
		}

		public ConditionTask condition{
			get {return _condition;}
			set
			{
				_condition = value;
				if (_condition != null)
					_condition.SetOwnerSystem(graph);
			}
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (!condition || condition.CheckCondition(agent, blackboard))
				return targetNode.Execute(agent, blackboard);

			targetNode.ResetNode();
			return Status.Failure;
		}

		public bool CheckCondition(){
			return CheckCondition(graphAgent, graphBlackboard);
		}

		public bool CheckCondition(Component agent){
			return CheckCondition(agent, graphBlackboard);
		}

		///To be used if and when want to just check the connection without execution, since OnExecute this is called as well to determine return status.
		virtual public bool CheckCondition(Component agent, Blackboard blackboard){

			if ( !isDisabled && (!condition || condition.CheckCondition(agent, blackboard) ) )
				return true;

			connectionStatus = Status.Failure;
			return false;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		[SerializeField]
		private bool _showConditionsGUI;

		protected override void OnConnectionGUI(){

			var e = Event.current;

			var textToShow= condition? condition.taskInfo : "No Condition";
			textToShow = _showConditionsGUI? textToShow : (condition? "-||-" : "---");

			var finalSize= new GUIStyle("Box").CalcSize(new GUIContent(textToShow));
			areaRect.width = finalSize.x;
			areaRect.height = finalSize.y;

			if (e.button == 1 && e.type == EventType.MouseDown && areaRect.Contains(e.mousePosition)){
				_showConditionsGUI = !_showConditionsGUI;
				e.Use();
			}

			var alpha = (Graph.currentSelection != this && condition == null)? 0.1f : 0.8f;
			GUI.color = new Color(1f,1f,1f,alpha);
			GUI.Box(areaRect, textToShow);

			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
		}

		protected override void OnConnectionInspectorGUI(){

			if (!condition){
				EditorUtils.TaskSelectionButton(gameObject, typeof(ConditionTask), delegate(Task c){condition = (ConditionTask)c;});
			} else {
				EditorUtils.TaskTitlebar(condition);
			}
		}

		#endif
	}
}
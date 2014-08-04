using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Interrupt")]
	[Category("Decorators")]
	[Description("Interrupt the child node and return Failure if the condition is or becomes true while running. Otherwise return whatever the child returns")]
	[Icon("Interruptor")]
	public class BTInterruptor : BTDecorator, ITaskAssignable{

		[SerializeField]
		private ConditionTask _condition;

		public Task task{
			get {return condition;}
			set {condition = (ConditionTask)value;}
		}

		private ConditionTask condition{
			get {return _condition;}
			set
			{
				_condition = value;
				if (_condition != null)
					_condition.SetOwnerSystem(graph);
			}
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (!decoratedConnection)
				return Status.Resting;

			if (!condition || condition.CheckCondition(agent, blackboard) == false)
				return decoratedConnection.Execute(agent, blackboard);

			if (decoratedConnection.connectionStatus == Status.Running)
				decoratedConnection.ResetConnection();
			
			return Status.Failure;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			GUILayout.Label(condition? condition.taskInfo : "No Condition");
		}

		protected override void OnNodeInspectorGUI(){

			if (condition == null){
				EditorUtils.TaskSelectionButton(gameObject, typeof(ConditionTask), delegate(Task c){condition = (ConditionTask)c;});
			} else {
				EditorUtils.TaskTitlebar(condition);
			}
		}

		#endif
	}
}
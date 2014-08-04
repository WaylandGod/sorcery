using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace NodeCanvas.Actions{

	[Category("✫ Script Control")]
	[Description("Calls a function that has signature of 'public Status NAME()'. Return Status.Success, Failure or Running within that function")]
	[AgentType(typeof(Transform))]
	public class ImplementedAction : ActionTask {

		[RequiredField]
		public string methodName;
		[RequiredField]
		public string scriptName;

		private Component script;
		private MethodInfo method;
		private Status status = Status.Resting;

		protected override string info{
			get {return string.Format("({0}.{1})", agentInfo, methodName);}
		}

		protected override string OnInit(){
			script = agent.GetComponent(scriptName);
			if (script == null)
				return "Can't find script";
			method = script.GetType().GetMethod(methodName);
			if (method == null)
				return "Method not found";
			return null;
		}

		protected override void OnExecute(){
			Forward();
		}

		protected override void OnUpdate(){
			Forward();
		}

		void Forward(){

			status = (Status)method.Invoke(script, null);

			if (status == Status.Success){
				EndAction(true);
				return;
			}

			if (status == Status.Failure){
				EndAction(false);
				return;
			}
		}

		protected override void OnStop(){
			status = Status.Resting;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnTaskInspectorGUI(){

			if (agent == null){
				UnityEditor.EditorGUILayout.HelpBox("This Action needs the Agent to be known. Currently the Agent is unknown", UnityEditor.MessageType.Error);
				return;
			}

			if (agent.GetComponent(scriptName) == null){
				scriptName = null;
				methodName = null;
			}

			if (GUILayout.Button("Select Action Method")){
				EditorUtils.ShowMethodSelectionMenu(agent.gameObject, new List<System.Type>{typeof(Status)}, null, delegate(MethodInfo method){
					scriptName = method.ReflectedType.Name;
					methodName = method.Name;
					if (Application.isPlaying)
						OnInit();
				});
			}

			if (!string.IsNullOrEmpty(methodName)){
				UnityEditor.EditorGUILayout.LabelField("Selected Action Method:", methodName);
			}
		}
		
		#endif
	}
}
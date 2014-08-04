using UnityEngine;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	[Category("✫ Script Control")]
	[AgentType(typeof(Transform))]
	[Description("SendMessage to the agent, optionaly with any blackboard variable value as an argument")]
	public class SendUnityMessage : ActionTask{

		[RequiredField]
		public BBString methodName;
		public BBVar variableArgument;

		protected override string info{
			get {return "Message " + methodName;}
		}

		protected override void OnExecute(){

			if (variableArgument.isNull){
				agent.SendMessage(methodName.value);
			} else {
				agent.SendMessage(methodName.value, variableArgument.value);
			}

			EndAction();
		}
	}
}
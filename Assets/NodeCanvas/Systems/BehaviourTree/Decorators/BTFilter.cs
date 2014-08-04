using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Filter")]
	[Category("Decorators")]
	[Description("Filters the access of it's child node either a specific number of times, or every specific amount of time. By default the node is 'Treated as Inactive' to it's parent when child is Filtered. Unchecking this option will instead return Failure when Filtered.")]
	[Icon("Lock")]
	public class BTFilter : BTDecorator {

		public enum LimitMode{
			LimitNumberOfTimes,
			CoolDown
		}

		public LimitMode limitMode = LimitMode.CoolDown;

		public BBInt maxCount = new BBInt{value = 1};
		private int executedCount;

		public BBFloat coolDownTime = new BBFloat{value = 5};
		private float currentTime;

		public bool inactiveWhenLimited = true;


		public override void OnGraphStarted(){
			executedCount = 0;
			currentTime = 0;
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			if (decoratedConnection == null)
				return Status.Resting;

			if (limitMode == LimitMode.CoolDown){

				if (currentTime > 0)
					return inactiveWhenLimited? Status.Resting : Status.Failure;

				status = decoratedConnection.Execute(agent, blackboard);
				if (status == Status.Success || status == Status.Failure)
					StartCoroutine(Cooldown());
			}
			else
			if (limitMode == LimitMode.LimitNumberOfTimes){

				if (executedCount >= maxCount.value)
					return inactiveWhenLimited? Status.Resting : Status.Failure;

				status = decoratedConnection.Execute(agent, blackboard);
				if (status == Status.Success || status == Status.Failure)
					executedCount += 1;
			}

			return status;
		}

		IEnumerator Cooldown(){

			currentTime = coolDownTime.value;
			while (currentTime > 0){
				currentTime -= Time.deltaTime;
				yield return null;
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			if (limitMode == LimitMode.CoolDown){
				GUILayout.Label("", GUILayout.Height(25));
				var pRect = new Rect(5, GUILayoutUtility.GetLastRect().y, nodeRect.width - 10, 20);
				UnityEditor.EditorGUI.ProgressBar(pRect, currentTime/coolDownTime.value, currentTime > 0? "Cooling..." : "Cooled");
			}
			else
			if (limitMode == LimitMode.LimitNumberOfTimes){
				GUILayout.Label(executedCount + " / " + maxCount.value + " Accessed Times");
			}
		}

		protected override void OnNodeInspectorGUI(){

			limitMode = (LimitMode)UnityEditor.EditorGUILayout.EnumPopup("Mode", limitMode);

			if (limitMode == LimitMode.CoolDown){
				coolDownTime = (BBFloat)EditorUtils.BBVariableField("CoolDown Time", coolDownTime);
			}
			else
			if (limitMode == LimitMode.LimitNumberOfTimes){
				maxCount = (BBInt)EditorUtils.BBVariableField("Max Times", maxCount);
			}

			inactiveWhenLimited = UnityEditor.EditorGUILayout.Toggle("Inactive When Limited", inactiveWhenLimited);
		}
		
		#endif
	}
}
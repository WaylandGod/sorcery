using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using NodeCanvas.Variables;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Probability Selector")]
	[Category("Composites")]
	[Description("Select a child to execute based on it's chance to be selected and return Success if it returns Success, otherwise pick another.\nReturns Failure if no child returns Success or a direct 'Failure Chance' is introduced")]
	[Icon("ProbabilitySelector")]
	public class BTProbabilitySelector : BTComposite {

		[SerializeField]
		private List<BBFloat> childWeights = new List<BBFloat>();
		[SerializeField]
		private BBFloat failChance = new BBFloat();

		private float probability;
		private float currentProbability;
		private float total;

		public override string nodeName{
			get {return string.Format("<color=#b3ff7f>{0}</color>", base.nodeName.ToUpper());}
		}

		public override void OnPortConnected(int index){
			childWeights.Insert(index, new BBFloat{value = 1, bb = graphBlackboard});
			UpdateNodeBBFields(graphBlackboard);
		}

		public override void OnPortDisconnected(int index){
			childWeights.RemoveAt(index);
		}

		//To create a new probability when BT starts
		public override void OnGraphStarted(){
			OnReset();
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			currentProbability = probability;

			for (int i = 0; i < outConnections.Count; i++){

				if (currentProbability >= childWeights[i].value){
					currentProbability -= childWeights[i].value;
					continue;
				}

				status = outConnections[i].Execute(agent, blackboard);
				if (status == Status.Running || status == Status.Success)
					return status;
			}

			return Status.Failure;
		}

		protected override void OnReset(){

			CalcTotal();
			probability = Random.Range(0f, total);
		}


		void CalcTotal(){
			
			total = failChance.value;
			foreach (BBFloat weight in childWeights)
				total += weight.value;
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeGUI(){
		
			if (outConnections.Count == 0){
				GUILayout.Label("No Connections");
				return;
			}

			CalcTotal();

			if (total == 0){
				GUILayout.Label("100% Failure");
				return;
			}

			string weightsString = string.Empty;
			for (int i = 0; i < childWeights.Count; i++)
				weightsString += Mathf.Round( (childWeights[i].value/total) * 100 ) + "%" + ( (i == childWeights.Count - 1)? " " : ", ");

			GUILayout.Label(weightsString);
		}

		protected override void OnNodeInspectorGUI(){

			if (outConnections.Count == 0){
				GUILayout.Label("Make some connections first");
				return;
			}

			CalcTotal();

			for (int i = 0; i < childWeights.Count; i++){

				GUILayout.BeginHorizontal();
				childWeights[i] = (BBFloat)EditorUtils.BBVariableField("Weight", childWeights[i]);
				GUILayout.Label( Mathf.Round( (childWeights[i].value/total) * 100 ) + "%", GUILayout.Width(30));
				GUILayout.EndHorizontal();
			}

			GUILayout.Space(5);

			GUILayout.BeginHorizontal();
			failChance = (BBFloat)EditorUtils.BBVariableField("Direct Failure Chance", failChance);
			GUILayout.Label( Mathf.Round( (failChance.value/total) * 100 ) + "%", GUILayout.Width(30));
			GUILayout.EndHorizontal();
		}
		
		#endif
	}
}
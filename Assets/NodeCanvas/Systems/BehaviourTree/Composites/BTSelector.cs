using UnityEngine;
using System.Collections.Generic;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Selector")]
	[Category("Composites")]
	[Description("Execute the child nodes in order or randonly until the first that returns Success and return Success as well. If none returns Success, then returns Failure.\nIf is Dynamic, then higher priority children Status are revaluated and if one returns Success the Selector will select that one and bail out immediately in Success too")]
	[Icon("Selector")]
	public class BTSelector : BTComposite{

		public bool isDynamic;
		public bool isRandom;

		private int lastRunningNodeIndex= 0;

		public override string nodeName{
			get {return string.Format("<color=#b3ff7f>{0}</color>", base.nodeName.ToUpper());}
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			for ( int i= isDynamic? 0 : lastRunningNodeIndex; i < outConnections.Count; i++){

				status = outConnections[i].Execute(agent, blackboard);
				
				if (status == Status.Running){

					if (isDynamic && i < lastRunningNodeIndex)
						outConnections[lastRunningNodeIndex].ResetConnection();

					lastRunningNodeIndex = i;
					return status;
				}

				if (status == Status.Success){
					
					if (isDynamic && i < lastRunningNodeIndex)
						outConnections[lastRunningNodeIndex].ResetConnection();

					return status;
				}
			}

			return Status.Failure;
		}

		protected override void OnReset(){
			lastRunningNodeIndex = 0;
			if (isRandom)
				outConnections = Shuffle(outConnections);
		}

		public override void OnPortDisconnected(int index){
			if (index != 0 && index == lastRunningNodeIndex)
				lastRunningNodeIndex--;
		}

		public override void OnGraphStarted(){
			OnReset();
		}

		//Fisher-Yates shuffle algorithm
		private List<Connection> Shuffle(List<Connection> list){
			for ( int i= list.Count -1; i > 0; i--){
				var j = (int)Mathf.Floor(Random.value * (i + 1));
				var temp = list[i];
				list[i] = list[j];
				list[j] = temp;
			}

			return list;
		}

		
		/////////////////////////////////////////
		/////////GUI AND EDITOR STUFF////////////
		/////////////////////////////////////////
		#if UNITY_EDITOR

		protected override void OnNodeGUI(){

			if (isDynamic)
				GUILayout.Label("Dynamic");
			if (isRandom)
				GUILayout.Label("Random");

			if (!isDynamic && !isRandom)
				GUILayout.Label("", GUILayout.Height(1));
		}

		#endif
	}
}
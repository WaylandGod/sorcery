using UnityEngine;
using System.Collections.Generic;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	[Name("Parallel")]
	[Category("Composites")]
	[Description("Execute all child nodes once but simultaneously and return Success or Failure depending on the selected Policy.\nIf is Dynamic higher priority chilren status are revaluated")]
	[Icon("Parallel")]
	public class BTParallel : BTComposite{

		public enum Policy {FirstFailure, FirstSuccess}
		public Policy policy = Policy.FirstFailure;
		public bool isDynamic;

		private List<Connection> finishedConnections = new List<Connection>();

		public override string nodeName{
			get {return string.Format("<color=#ff64cb>{0}</color>", base.nodeName.ToUpper());}
		}

		protected override Status OnExecute(Component agent, Blackboard blackboard){

			for ( int i= 0; i < outConnections.Count; i++){

				if (!isDynamic){
					if (finishedConnections.Contains(outConnections[i]))
						continue;
				}

				status = outConnections[i].Execute(agent, blackboard);

				if (status == Status.Failure && policy == Policy.FirstFailure){
					ResetRunning();
					return Status.Failure;
				}

				if (status == Status.Success && policy == Policy.FirstSuccess){
					ResetRunning();
					return Status.Success;
				}

				if ( status != Status.Running && !finishedConnections.Contains(outConnections[i]))
					finishedConnections.Add(outConnections[i]);
			}

			if (finishedConnections.Count == outConnections.Count){
				if (policy == Policy.FirstFailure)
					return Status.Success;
				if (policy == Policy.FirstSuccess)
					return Status.Failure;
			}

			return Status.Running;
		}

		protected override void OnReset(){

			finishedConnections.Clear();
		}

		void ResetRunning(){
			for (int i = 0; i < outConnections.Count; i++){
				if (outConnections[i].connectionStatus == Status.Running)
					outConnections[i].ResetConnection();
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			GUILayout.Label(policy.ToString());
		}

		#endif
	}
}
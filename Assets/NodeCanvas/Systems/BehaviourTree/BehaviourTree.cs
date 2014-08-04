using UnityEngine;
using System.Collections;

namespace NodeCanvas.BehaviourTrees{

	[AddComponentMenu("")]
	///The actual Behaviour Tree
	public class BehaviourTree : Graph{

		///Should the tree repeat forever?
		public bool runForever = true;
		///The frequenct in seconds for the tree to repeat if set to run forever.
		public float updateInterval = 0;

		private float intervalCounter = 0;
		private Status _rootStatus = Status.Resting;

		///The last status of the root
		public Status rootStatus{
			get{return _rootStatus;}
			private set {_rootStatus = value;}
		}

		public override System.Type baseNodeType{
			get {return typeof(BTNodeBase);}
		}

		protected override void OnGraphStarted(){

			intervalCounter = updateInterval;
			rootStatus = primeNode.status;
		}

		protected override void OnGraphUpdate(){

			if (intervalCounter >= updateInterval){

				intervalCounter = 0;

				Tick(agent, blackboard);

				if (!runForever && rootStatus != Status.Running)
					StopGraph();
			}

			intervalCounter += Time.deltaTime;
		}

		///Tick the tree once for the provided agent and with the provided blackboard
		public void Tick(Component agent, Blackboard blackboard){

			if (rootStatus != Status.Running)
				primeNode.ResetNode();

			rootStatus = primeNode.Execute(agent, blackboard);
		}


		////////////////////////////////////////
		#if UNITY_EDITOR
		
		[UnityEditor.MenuItem("NC/Create BehaviourTree")]
		public static void CreateBehaviourTree(){
			BehaviourTree newBT = new GameObject("BehaviourTree").AddComponent(typeof(BehaviourTree)) as BehaviourTree;
			UnityEditor.Selection.activeObject = newBT;
		}		
		#endif
	}
}
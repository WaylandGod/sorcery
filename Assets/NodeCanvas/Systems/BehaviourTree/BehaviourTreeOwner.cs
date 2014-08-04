using UnityEngine;

namespace NodeCanvas.BehaviourTrees{

	///This class is essentially a front-end that wraps the execution of a BehaviourTree
	[AddComponentMenu("NodeCanvas/Behaviour Tree Owner")]
	public class BehaviourTreeOwner : GraphOwner {

		public BehaviourTree BT;

		public override Graph behaviour{
			get { return BT;}
			set { BT = (BehaviourTree)value;}
		}
		
		public override System.Type graphType{
			get {return typeof(BehaviourTree);}
		}

		///Should the assigned BT reset and rexecute after a cycle? Sets the BehaviourTree's runForever
		public bool runForever{
			get {return BT != null? BT.runForever : true;}
			set {if (BT != null) BT.runForever = value;}
		}

		///The interval in seconds to update the BT. 0 for every frame. Sets the BehaviourTree's updateInterval
		public float updateInterval{
			get {return BT != null? BT.updateInterval : 0;}
			set {if (BT != null) BT.updateInterval = value;}
		}

		///The last status of the assigned Behaviour Tree's root node (aka Start Node)
		public Status rootStatus{
			get {return BT != null? BT.rootStatus : Status.Resting;}
		}


		///Tick the assigned Behaviour Tree for this owner and retruns it's root status. Same as BehaviourTree.Tick()
		public Status Tick(){
			
			if (BT == null){
				Debug.LogWarning("There is no Behaviour Tree assigned", gameObject);
				return Status.Resting;
			}

			BT.Tick(this, blackboard);
			return BT.rootStatus;
		}

		[System.Obsolete("Use PauseGraph() instead")]
		public void Pause(){
			PauseGraph();
		}
	}
}
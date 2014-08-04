using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace NodeCanvas.StateMachines{

	[AddComponentMenu("")]
	///The actual State Machine
	public class FSM : Graph{

		private FSMState currentState;
		private FSMState lastState;
		private List<FSMAnyState> anyStates = new List<FSMAnyState>();
		private Dictionary<MonoBehaviour, MethodInfo> enterMethods = new Dictionary<MonoBehaviour, MethodInfo>();
		private Dictionary<MonoBehaviour, MethodInfo> stayMethods  = new Dictionary<MonoBehaviour, MethodInfo>();
		private Dictionary<MonoBehaviour, MethodInfo> exitMethods  = new Dictionary<MonoBehaviour, MethodInfo>();

		///The current status name. Null if none
		public string currentStateName{
			get {return currentState != null? currentState.nodeName : null; }
		}

		///The last status name. Not the current! Null if none
		public string lastStateName{
			get	{return lastState != null? lastState.nodeName : null; }
		}

		public override System.Type baseNodeType{
			get {return typeof(FSMState);}
		}

		protected override void OnGraphStarted(){

			GatherMethodInfo();
			anyStates.Clear();
			foreach(FSMState node in allNodes){

				if (node.GetType() == typeof(FSMConcurrentState))
					node.Execute(agent, blackboard);

				if (node.GetType() == typeof(FSMAnyState))
					anyStates.Add(node as FSMAnyState);
			}

			EnterState(lastState == null? primeNode as FSMState : lastState);
		}

		protected override void OnGraphUpdate(){

			foreach(FSMAnyState anyState in anyStates)
				anyState.UpdateAnyState();

			currentState.OnUpdate();
			CallbackStay(currentState);
		}

		protected override void OnGraphStoped(){

			lastState = null;
			currentState = null;
		}

		protected override void OnGraphPaused(){
			lastState = currentState;
			currentState = null;
		}

		///Enter a state providing the state itself
		public bool EnterState(FSMState newState){

			if (!isRunning){
				Debug.LogWarning("Tried to EnterState on an FSM that was not running", gameObject);
				return false;
			}

			if (newState == null){
				Debug.LogWarning("Tried to Enter Null State");
				return false;
			}

			if (currentState != null){
				
				currentState.Finish();
				currentState.ResetNode();
				CallbackExit(currentState);
				
				//for editor..
				foreach (Connection inConnection in currentState.inConnections)
					inConnection.connectionStatus = Status.Resting;
				///
			}

			lastState = currentState;
			currentState = newState;
			currentState.Execute(agent, blackboard);
			CallbackEnter(currentState);
			return true;
		}

		///Trigger a state to enter by it's name. Returns the state found and entered if any
		public FSMState TriggerState(string stateName){

			var state = GetStateWithName(stateName);
			if (state != null){
				EnterState(state);
				return state;
			}

			Debug.LogWarning("No State with name '" + stateName + "' found on FSM '" + graphName + "'");
			return null;
		}

		///Get all State Names
		public List<string> GetStateNames(){

			var names = new List<string>();
			foreach(FSMState node in allNodes){
				if (node.allowAsPrime)
					names.Add(node.nodeName);
			}
			return names;
		}

		///Get a state by it's name
		public FSMState GetStateWithName(string name){

			foreach (FSMState node in allNodes){
				if (node.allowAsPrime && node.nodeName == name)
					return node;
			}
			return null;
		}

		void CallbackEnter(IState state){
			foreach (KeyValuePair<MonoBehaviour, MethodInfo> pair in enterMethods)
				pair.Value.Invoke(pair.Key, new object[]{state});
		}

		void CallbackStay(IState state){
			foreach (KeyValuePair<MonoBehaviour, MethodInfo> pair in stayMethods)
				pair.Value.Invoke(pair.Key, new object[]{state});
		}

		void CallbackExit(IState state){
			foreach (KeyValuePair<MonoBehaviour, MethodInfo> pair in exitMethods)
				pair.Value.Invoke(pair.Key, new object[]{state});			
		}

		void GatherMethodInfo(){

			enterMethods.Clear();
			stayMethods.Clear();
			exitMethods.Clear();

			foreach (MonoBehaviour mono in agent.gameObject.GetComponents<MonoBehaviour>()){

				var enterMethod = mono.GetType().GetMethod("OnStateEnter", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var stayMethod = mono.GetType().GetMethod("OnStateUpdate", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				var exitMethod = mono.GetType().GetMethod("OnStateExit", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				if (enterMethod != null)
					enterMethods[mono] = enterMethod;
				if (stayMethod != null)
					stayMethods[mono] = stayMethod;
				if (exitMethod != null)
					exitMethods[mono] = exitMethod;
			}
		}

		////////////////////////////////////////
		#if UNITY_EDITOR
		
		[UnityEditor.MenuItem("NC/Create FSM")]
		public static void CreateFSM(){
			FSM newFSM= new GameObject("FSM").AddComponent(typeof(FSM)) as FSM;
			UnityEditor.Selection.activeObject = newFSM;
		}		
		
		#endif
	}
}
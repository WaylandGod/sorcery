using UnityEngine;
using System.Collections.Generic;
using System;

namespace NodeCanvas{

	///The base class where BehaviourTreeOwner and FSMOwner derive from. GraphOwners simply wrap the execution of a Graph and act as a front-end to the user.
	abstract public class GraphOwner : MonoBehaviour {

		public enum EnableAction{
			StartBehaviour,
			DoNothing
		}

		public enum DisableAction{
			StopBehaviour,
			PauseBehaviour,
			DoNothing
		}

		///What will happen OnEnable
		public EnableAction onEnable = EnableAction.StartBehaviour;
		///What will happen OnDisable
		public DisableAction onDisable = DisableAction.StopBehaviour;

		[SerializeField]
		private Blackboard _blackboard;
		private Dictionary<Graph, Graph> instances = new Dictionary<Graph, Graph>();
		private static bool isQuiting;

		///Is the assigned behaviour currently running?
		public bool isRunning{
			get {return behaviour != null? behaviour.isRunning : false;}
		}

		///Is the assigned behaviour currently paused?
		public bool isPaused{
			get {return behaviour != null? behaviour.isPaused : false;}
		}

		///The blackboard that the assigned behaviour will be Started with
		public Blackboard blackboard{
			get {return _blackboard;}
			set {_blackboard = value; if (behaviour != null) behaviour.blackboard = value;}
		}

		///The current behaviour assigned
		abstract public Graph behaviour{ get; set; }
		///The Graph type this Owner should be assigned
		abstract public Type graphType{ get; }

		[Obsolete("Use 'onEnable = GraphOwer.EnableAction.StartBehaviour")]
		public bool executeOnStart{	get {return onEnable == EnableAction.StartBehaviour;} set {onEnable = EnableAction.StartBehaviour;}	}
		[Obsolete("Use 'behaviour' instead")]
		public Graph graph {get{return behaviour;} set {behaviour = value;}}
		[Obsolete("Use 'StartBehaviour' instead")]
		public void StartGraph(){StartBehaviour();}
		[Obsolete("Use 'StartBehaviour' instead")]
		public void StartGraph(Action callback){StartBehaviour(callback);}
		[Obsolete("Use 'StartBehaviour' instead")]
		public void StartGraph(Graph newGraph){StartBehaviour(newGraph);}
		[Obsolete("Use 'StartBehaviour' instead")]
		public void StartGraph(Graph newGraph, Action callback){StartBehaviour(newGraph, callback);}
		[Obsolete("Use 'PauseBehaviour' instead")]
		public void PauseGraph(){PauseBehaviour();}
		[Obsolete("Use 'StopBehaviour' instead")]
		public void StopGraph(){StopBehaviour();}
		[Obsolete("Use 'SwitchBehaviour' instead")]
		public void SwitchGraph(Graph newGraph){SwitchBehaviour(newGraph);}


		///Start the behaviour assigned
		public void StartBehaviour(){
			
			behaviour = GetInstance(behaviour);
			if (behaviour != null)
				behaviour.StartGraph(this, blackboard);
		}

		///Start the behaviour assigned providing a callback for when it ends
		public void StartBehaviour(Action callback){

			behaviour = GetInstance(behaviour);
			if (behaviour != null)
				behaviour.StartGraph(this, blackboard, callback);
		}

		///Start a new behaviour on this owner
		public void StartBehaviour(Graph newGraph){
			SwitchBehaviour(newGraph);
		}

		///Start a new behaviour on this owner and get a call back
		public void StartBehaviour(Graph newGraph, Action callback){
			SwitchBehaviour(newGraph, callback);
		}

		///Stop the behaviour assigned
		public void StopBehaviour(){
			if (behaviour != null)
				behaviour.StopGraph();
		}

		///Pause the assigned Behaviour. Same as Graph.PauseGraph
		public void PauseBehaviour(){
			if (behaviour != null)
				behaviour.PauseGraph();
		}

		///
		public void SwitchBehaviour(Graph newGraph){
			SwitchBehaviour(newGraph, null);
		}

		///Use to switch or set graphs at runtime and optionaly get a callback
		public void SwitchBehaviour(Graph newGraph, Action callback){
			
			if (newGraph.GetType() != graphType){
				Debug.LogWarning("Incompatible graph types." + this.GetType().Name + " can be assigned graphs of type " + graphType.Name);
				return;
			}

			StopBehaviour();
			behaviour = newGraph;
			StartBehaviour(callback);
		}

		///Send an event through the behaviour (To be used with CheckEvent for example). Same as Graph.SendEvent
		public void SendEvent(string eventName){
			if (behaviour != null)
				behaviour.SendEvent(eventName);
		}

		///Thats the same as calling the static Graph.SendGlobalEvent
		public void SendGlobalEvent(string eventName){
			Graph.SendGlobalEvent(eventName);
		}

		new public void SendMessage(string name){
			SendMessage(name, null);
		}

		///Sends a message to all tasks in the behaviour as well as this gameobject as usual. Same as Graph.SendMessage
		new public void SendMessage(string name, object arg){
			if (behaviour != null)
				behaviour.SendMessage(name, arg);
		}

		public void SendTaskMessage(string name){
			SendTaskMessage(name, null);
		}

		///Sends a message to all Task of the assigned behaviour. Same as Graph.SendTaskMessage
		public void SendTaskMessage(string name, object arg){
			if (behaviour != null)
				behaviour.SendTaskMessage(name, arg);
		}

		//Gets the instance graph for this owner of the provided graph
		Graph GetInstance(Graph originalGraph){

			if (!Application.isPlaying)
				return originalGraph;

			if (originalGraph == null)
				return null;

			Graph instance;

			//it means that the behaviour is not used as template
			if (originalGraph.transform.parent == this.transform){
			
				instance = originalGraph;
			
			} else {

				if (instances.ContainsKey(originalGraph)){
					instance = instances[behaviour];

				} else {

					instance = (Graph)Instantiate(originalGraph, transform.position, transform.rotation);
					instance.transform.parent = this.transform; //organization
					instances[originalGraph] = instance;
				}
			}

			instance.gameObject.hideFlags = Graph.doHide? HideFlags.HideAndDontSave : 0;
			return instance;
		}

		protected void OnEnable(){
			
			if (onEnable == EnableAction.StartBehaviour)
				StartBehaviour();
		}

		protected void OnDisable(){

			if (isQuiting)
				return;

			if (onDisable == DisableAction.StopBehaviour)
				StopBehaviour();

			if (onDisable == DisableAction.PauseBehaviour)
				PauseBehaviour();
		}

		protected void OnApplicationQuit(){
			isQuiting = true;
		}

		protected void Reset(){

			blackboard = gameObject.GetComponent<Blackboard>();
			if (blackboard == null)
				blackboard = gameObject.AddComponent<Blackboard>();		
		}

		protected void OnDrawGizmos(){
			Gizmos.DrawIcon(transform.position, "GraphOwnerGizmo.png", true);
		}
	}
}
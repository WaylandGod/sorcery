#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using NodeCanvas.Variables;

namespace NodeCanvas{

	///The base class for all Actions and Conditions. You dont actually use or derive this class. Instead derive from ActionTask and ConditionTask
	abstract public class Task : MonoBehaviour{

		[Serializable]
		///A special BBVariable for the task agent
		class TaskAgent : BBVariable{

			[SerializeField]
			private bool _isOverride;
			public bool isOverride{
				get {return _isOverride;}
				set {_isOverride = value;}
			}

			//Runtime checks
			private Component _current;
			public Component current{
				get {return _current;}
				set {_current = value;}
			}
			
			[SerializeField]
			private Component _value;
			public Component value{
				get {return useBlackboard? (Read<GameObject>()? Read<GameObject>().transform : null) : _value ;}
				set {_value = value;}
			}
			public override Type varType{
				get {return typeof(GameObject);}
			}
			public override string ToString(){
				if (useBlackboard) return base.ToString();
				return "<b>" + (_value != null? _value.name : "NULL") + "</b>";
			}
		}

		[SerializeField]
		private MonoBehaviour _ownerSystem;

		[SerializeField]
		private TaskAgent taskAgent = new TaskAgent();
		private Blackboard _blackboard;
		
		//store to avoid spamming reflection
		private System.Type _agentType;
		private string _taskName;
		private string _taskDescription;
		//

		//These are special so I write them first
		public void SetOwnerSystem(ITaskSystem newOwnerSystem){

			if (newOwnerSystem == null)
				return;

			_ownerSystem = (MonoBehaviour)newOwnerSystem;
			UpdateBBFields(newOwnerSystem.blackboard);
		}

		///The system this task belongs to from which defaults are taken from.
		protected ITaskSystem ownerSystem{
			get {return _ownerSystem as ITaskSystem;}
		}

		private Component ownerAgent{
			get
			{
				if (_ownerSystem == null)
					return null;
				return (_ownerSystem as ITaskSystem).agent;
			}
		}

		private Blackboard ownerBlackboard{
			get
			{
				if (_ownerSystem == null)
					return null;
				return (_ownerSystem as ITaskSystem).blackboard;
			}
		}
		///


		///The type that the agent will be set to by getting component from itself when the task initialize
		///You can omit this to keep the agent passed as is or if there is no need for specific type
		public System.Type agentType{

			get
			{
				if (_agentType == null){
					AgentTypeAttribute agentTypeAttribute = this.GetType().GetCustomAttributes(typeof(AgentTypeAttribute), true).FirstOrDefault() as AgentTypeAttribute;
					if (agentTypeAttribute != null && typeof(Component).IsAssignableFrom(agentTypeAttribute.type) ){
						_agentType = agentTypeAttribute.type;
					} else {
						_agentType = typeof(Component);
					}
				}
				return _agentType;
			}
		}

		///The friendly task name
		public string taskName{
			get
			{
				if (string.IsNullOrEmpty(_taskName) ){
					NameAttribute nameAttribute = this.GetType().GetCustomAttributes(typeof(NameAttribute), false).FirstOrDefault() as NameAttribute;
					if (nameAttribute != null){

						_taskName = nameAttribute.name;

					} else {

						#if UNITY_EDITOR
						_taskName = EditorUtils.CamelCaseToWords(EditorUtils.TypeName(this.GetType()));
						#endif
						#if !UNITY_EDITOR
						_taskName = this.GetType().Name;
						#endif
					}
				}
				return _taskName;
			}
		}

		public string taskDescription{
			get
			{
				if (_taskDescription == null ){
					DescriptionAttribute descAtt = this.GetType().GetCustomAttributes(typeof(DescriptionAttribute), false).FirstOrDefault() as DescriptionAttribute;
					if (descAtt != null){
						_taskDescription = descAtt.description;
					} else {
						_taskDescription = string.Empty;
					}
				}
				return _taskDescription;				
			}
		}

		[System.Obsolete("override 'info' instead")]
		virtual protected string actionInfo{
			get {return taskInfo;}
		}

		[System.Obsolete("override 'info' instead")]
		virtual protected string conditionInfo{
			get {return taskInfo;}
		}

		///A short summary of what the task will finaly do. Derived tasks may override this.
		virtual public string taskInfo{
			get {return taskName;}
		}

		///Summary info to display final agent string within task info if needed
		public string agentInfo{
			get	{ return agentIsOverride? taskAgent.ToString() : "<b>owner</b>"; }
		}

		///Is the agent overriden or the default taken from owner system will be used?
		public bool agentIsOverride{
			get { return taskAgent.isOverride; }
			private set
			{
				if (value == false && taskAgent.isOverride == true){
					taskAgent.useBlackboard = false;
					taskAgent.value = null;
				}
				taskAgent.isOverride = value;
			}
		}

		///The current or last executive agent of this task
		protected Component agent{
			get { return taskAgent.current? taskAgent.current : (agentIsOverride? taskAgent.value : ownerAgent); }
		}

		///The current or last blackboard of this task
		protected Blackboard blackboard{
			get
			{
				if (_blackboard == null){
					_blackboard = ownerBlackboard;
					UpdateBBFields(_blackboard);
				}

				return _blackboard;
			}
			private set
			{
				if (_blackboard != value)
					UpdateBBFields(value);

				_blackboard = value;
			}
		}

		//////////

		protected void Awake(){
			enabled = false;
			SubscribeToEvents(agent);
			CheckNullBBFields();
			OnAwake();
		}

		///Override in your own Tasks. Use this instead of Awake
		virtual protected void OnAwake(){

		}

		//Tasks can start coroutine through MonoManager for even when they are disabled
		new protected Coroutine StartCoroutine(IEnumerator routine){
			return MonoManager.current.StartCoroutine(routine);
		}

		///Sends an event to the owner system to handle (same as calling ownerSystem.SendEvent)
		protected void SendEvent(string eventName){

			if (ownerSystem != null)
				ownerSystem.SendEvent(eventName);
		}

		///Override in your own Tasks. This is called after a NEW agent is set, after initialization and before execution
		///Return null if everything is ok, or a string with the error if not.
		virtual protected string OnInit(){
			return null;
		}

		//Actions and Conditions call this before execution. Returns if the task was sucessfully initialized as well
		protected bool Set(Component newAgent, Blackboard newBB){

			//set blackboard with normal setter first
			blackboard = newBB;

			if (agentIsOverride){
				if (taskAgent.current && taskAgent.value && taskAgent.current.gameObject == taskAgent.value.gameObject)
					return true;
				return Initialize(TransformAgent(taskAgent.value, agentType));
			}

			if (taskAgent.current && taskAgent.current.gameObject == newAgent.gameObject)
				return true;			
			return Initialize(TransformAgent(newAgent, agentType));
		}

		//helper function
		Component TransformAgent(Component newAgent, System.Type type){
			return (type != typeof(Component) && newAgent != null)? newAgent.GetComponent(type) : newAgent;
		}

		//Initialize whenever agent is set to a new value. Essentially usage of the attributes
		bool Initialize(Component newAgent){

			taskAgent.current = newAgent;

			if (newAgent == null){
				Debug.LogError("<b>Task Init:</b> Failed to change Agent to type '" + agentType + "', for Task '" + taskName + "' or new Agent is NULL. Does the Agent has that Component?", this);
				return false;			
			}

			//Subscribe to events for the agent
			SubscribeToEvents(newAgent);

			//Usage of [RequiredField] and [GetFromAgent] attributes
			foreach (FieldInfo field in this.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)){

				var value = field.GetValue(this);
				
				var requiredAttribute = field.GetCustomAttributes(typeof(RequiredFieldAttribute), true).FirstOrDefault() as RequiredFieldAttribute;
				if (requiredAttribute != null){

					if (value == null || value.Equals(null)){
						Debug.LogError("<b>Task Init:</b> A required field for Task '" + taskName + "', is not set! Field: '" + field.Name + "' ", this);
						return false;
					}

					if (field.FieldType == typeof(string) && string.IsNullOrEmpty((string)value) ){
						Debug.LogError("<b>Task Init:</b> A required string for Task '" + taskName + "', is not set! Field: '" + field.Name + "' ", this);
						return false;
					}

					if (typeof(BBVariable).IsAssignableFrom(field.FieldType) && (value as BBVariable).isNull ) {
						Debug.LogError("<b>Task Init:</b> A required BBVariable value for Task '" + taskName + "', is not set! Field: '" + field.Name + "' ", this);
						return false;
					}
				}

				var getterAttribute = field.GetCustomAttributes(typeof(GetFromAgentAttribute), true).FirstOrDefault() as GetFromAgentAttribute;
				if (getterAttribute != null){

					if (typeof(Component).IsAssignableFrom(field.FieldType)){

						field.SetValue(this, newAgent.GetComponent(field.FieldType));
						if ( (field.GetValue(this) as UnityEngine.Object) == null){
							Debug.LogError("<b>Task Init:</b> GetFromAgent Attribute failed to get the required component of type '" + field.FieldType + "' from '" + agent.gameObject.name + "'. Does it exist?", agent.gameObject);
							return false;
						}
					
					} else {

						Debug.LogError("<b>Task Init:</b> You've set a GetFromAgent Attribute on a field (" + field.Name + ") whos type does not derive Component on Task '" + taskName + "'", this);
						return false;
					}
				}

				//BBGameObject has a special attribute [RequiresComponent]. We do that here.
				if (field.FieldType == typeof(BBGameObject)){
					var rc = field.GetCustomAttributes(typeof(RequiresComponentAttribute), true).FirstOrDefault() as RequiresComponentAttribute;
					var bbGO = value as BBGameObject;
					if (rc != null && !bbGO.isNull && bbGO.value.GetComponent(rc.type) == null){
						Debug.LogError("<b>Task Init:</b> BBGameObject requires missing Component of type '" + rc.type + "'.", agent.gameObject);
						return false;
					}
				}
			}

			//let user make further adjustments and inform us if there was an error
			string errorString = OnInit();
			if (errorString != null){
				Debug.LogError("<b>Task Init:</b> " + errorString + ". Task '" + taskName + "'");
				return false;
			}
			return true;
		}

		//Usage of [EventListener]. This is done in awake and init
		void SubscribeToEvents(Component newAgent){

			var msgAttribute = this.GetType().GetCustomAttributes(typeof(EventListenerAttribute), true).FirstOrDefault() as EventListenerAttribute;
			if (msgAttribute != null){
				var agentUtils = newAgent.GetComponent<AgentUtilities>();
				if (agentUtils == null)
					agentUtils = newAgent.gameObject.AddComponent<AgentUtilities>();

				foreach (string msg in msgAttribute.messages)
					agentUtils.Listen(this, msg);
			}
		}

		//Set the target blackboard for all BBVariables found in class. This is done every time the blackboard of the Task is set to a new value
		void UpdateBBFields(Blackboard bb){
			BBVariable.SetBBFields(bb, this);
			taskAgent.bb = bb;
		}

		//Helper to ensure that BBVariables are not null.
		void CheckNullBBFields(){
			BBVariable.CheckNullBBFields(this);
		}

		public override string ToString(){
			return string.Format("{0} ({1})", taskName, taskInfo);
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		[SerializeField]
		private bool _unfolded = true;
		private static Task _copyTask;

		public bool unfolded{
			get {return _unfolded;}
			set {_unfolded = value;}
		}

		public static Task copyTask{
			get {return _copyTask;}
			set {_copyTask = value;}
		}

		virtual protected void Reset(){
			enabled = false;
			CheckNullBBFields();
		}

		virtual protected void OnValidate(){
			enabled = false;
			CheckNullBBFields();
		}

		virtual public void ShowInspectorGUI(){

			if (Application.isPlaying && agentIsOverride && taskAgent.value == null){
				GUI.color = EditorUtils.lightRed;
				EditorGUILayout.LabelField("Missing Agent Reference!");
				GUI.color = Color.white;
				return;
			}

			if (agentType != typeof(Component) && typeof(Component).IsAssignableFrom(agentType))
				AgentField();
		}

		virtual public Task CopyTo(GameObject go){

			if (this == null)
				return null;

			Task copiedTask = (Task)go.AddComponent(this.GetType());
			Undo.RegisterCreatedObjectUndo(copiedTask, "Copy Task");
			UnityEditor.EditorUtility.CopySerialized(this, copiedTask);
			return copiedTask;
		}

		///Draw an auto editor inspector for this task.
		protected void DrawDefaultInspector(){
			
			EditorUtils.ShowAutoEditorGUI(this);
		}

		//Shows the agent field
		void AgentField(){

			Undo.RecordObject(this, "Agent Field Change");

			var isMissing = agent == null || agent.GetComponent(agentType) == null;
			var infoString = isMissing? "<color=#ff5f5f>" + EditorUtils.TypeName(agentType) + "</color>": EditorUtils.TypeName(agentType);

			GUI.color = new Color(1f,1f,1f, agentIsOverride? 1f : 0.5f);
			GUI.backgroundColor = GUI.color;
			GUILayout.BeginVertical("box");
			GUILayout.BeginHorizontal();
			if (!taskAgent.useBlackboard){

				if (agentIsOverride){
					taskAgent.value = EditorGUILayout.ObjectField(taskAgent.value, agentType, true) as Component;
				} else {
					GUILayout.BeginHorizontal();
					var compIcon = EditorGUIUtility.FindTexture(EditorUtils.TypeName(agentType) + " Icon");
					if (compIcon != null)
						GUILayout.Label(compIcon, GUILayout.Width(16), GUILayout.Height(16));
					GUILayout.Label("Owner Agent (" + infoString + ")");
					GUILayout.EndHorizontal();
				}

			} else {

				GUI.color = new Color(0.9f,0.9f,1f,1f);
				if (taskAgent.bb){

					var dataNames = taskAgent.bb.GetDataNames(typeof(GameObject)).ToList();
					if (dataNames.Contains(taskAgent.dataName) || string.IsNullOrEmpty(taskAgent.dataName) ){
						taskAgent.dataName = EditorUtils.StringPopup(taskAgent.dataName, dataNames, false, true);
					} else {
						taskAgent.dataName = EditorGUILayout.TextField("Override", taskAgent.dataName);
					}

				} else {
					taskAgent.dataName = EditorGUILayout.TextField("Override", taskAgent.dataName);
				}
			}

			GUI.color = Color.white;

			if (agentIsOverride){
				if (isMissing)
					GUILayout.Label("(" + infoString + ")", GUILayout.Height(15));
				taskAgent.useBlackboard = EditorGUILayout.Toggle(taskAgent.useBlackboard, EditorStyles.radioButton, GUILayout.Width(18));
			}

			if (!Application.isPlaying)
				agentIsOverride = EditorGUILayout.Toggle(agentIsOverride, GUILayout.Width(18));

			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;

			if (GUI.changed)
				EditorUtility.SetDirty(this);
		}

		#endif


		///If the field is deriving Component then it will be retrieved from the agent. The field is also considered Required for correct initialization
		[AttributeUsage(AttributeTargets.Field)]
		protected class GetFromAgentAttribute : Attribute{

		}

		///Designates that the task requires Unity messages to be forwarded from the agent and to this task
		[AttributeUsage(AttributeTargets.Class)]
		protected class EventListenerAttribute : Attribute{

			public string[] messages;

			public EventListenerAttribute(params string[] args){
				this.messages = args;
			}
		}

		///Designates what type of component to get and set the agent from the agent itself on initialization.
		///That component type is also considered required for correct task init.
		[AttributeUsage(AttributeTargets.Class)]
		protected class AgentTypeAttribute : Attribute{

			public System.Type type;

			public AgentTypeAttribute(System.Type type){
				this.type = type;
			}
		}
	}
}
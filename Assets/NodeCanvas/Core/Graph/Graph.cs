#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NodeCanvas{

	///This is the base and main class of NodeCanvas and graphs. All graph Systems are deriving from this.
	abstract public class Graph : MonoBehaviour, ITaskSystem {

		[SerializeField]
		private string _graphName = string.Empty;
		[SerializeField]
		private Node _primeNode;
		[SerializeField]
		private List<Node> _allNodes = new List<Node>();
		[SerializeField]
		private Component _agent;
		[SerializeField]
		private Blackboard _blackboard;
		[HideInInspector]
		public Transform _nodesRoot;
		private bool _isRunning;
		private bool _isPaused;

		private System.Action FinishCallback;
		public static bool doHide = false;

		/////
		/////

		public string graphName{
			get
			{
				if (string.IsNullOrEmpty(_graphName))
					_graphName = gameObject.name;
				return _graphName;
			}

			set
			{
				_graphName = value;
				if (gameObject.name != value && !string.IsNullOrEmpty(value))
					gameObject.name = value;
			}
		}

		///The base type of all nodes that can live in this system
		virtual public System.Type baseNodeType{
			get {return typeof(Node);}
		}

		///Is this system allowed to start with a null agent?
		virtual protected bool allowNullAgent{
			get {return false;}
		}

		///The node to execute first. aka 'START'
		public Node primeNode{
			get {return _primeNode;}
			set
			{
				if (value && value.allowAsPrime == false){
					Debug.Log("Node '" + value.nodeName + "' can't be set as Start");
					return;
				}
				
				if (isRunning){
					if (_primeNode != null)	_primeNode.ResetNode();
					if (value != null) value.ResetNode();
				}

				#if UNITY_EDITOR //To save me some sanity
				Undo.RecordObject(this, "Mark Start");
				#endif
				
				_primeNode = value;
			}
		}

		///All nodes assigned to this system. The list is already sorted by their IDs
		public List<Node> allNodes{
			get {return _allNodes;}
			private set {_allNodes = value;}
		}

		///The agent currently assigned to the graph
		public Component agent{
			get {return _agent;}
			set
			{
				if (_agent != value){
					_agent = value;
					SendTaskOwnerDefaults();
				}
				_agent = value;
			}
		}

		///The blackboard currently assigned to the graph
		public Blackboard blackboard{
			get {return _blackboard;}
			set
			{
				if (_blackboard != value){
					_blackboard = value;
					UpdateAllNodeBBFields();
					SendTaskOwnerDefaults();
				}
				_blackboard = value;
			}
		}

		///Is the graph running?
		public bool isRunning{
			get {return _isRunning;}
			private set {_isRunning = value;}
		}

		///Is the graph paused?
		public bool isPaused{
			get {return _isPaused;}
			private set {_isPaused = value;}
		}

		public Transform nodesRoot{
			get
			{
				if (_nodesRoot == null){
					_nodesRoot = new GameObject("__ALLNODES__").transform;
					_nodesRoot.gameObject.AddComponent<NodesRootUtility>().parentGraph = this;
				}

				if (_nodesRoot.parent != this.transform)
					_nodesRoot.parent = this.transform;

				_nodesRoot.gameObject.hideFlags = doHide? HideFlags.HideInHierarchy : 0;
				_nodesRoot.localPosition = Vector3.zero;
				return _nodesRoot;			
			}
		}

		///////
		///////

		//To ensure that if it doesn't exist on applicaiton quit, we dont get a leaking GO
		protected void Awake(){
			MonoManager.Create();
		}

		//Sets all graph's Tasks' owner (which is this)
		public void SendTaskOwnerDefaults(){

			foreach (Task task in nodesRoot.GetComponentsInChildren<Task>(true))
				task.SetOwnerSystem(this);
		}

		//Update all graph node's BBFields
		private void UpdateAllNodeBBFields(){

			foreach (Node node in allNodes)
				node.UpdateNodeBBFields(blackboard);
		}

		///Sends a OnCustomEvent message to the tasks that needs them. Tasks subscribe to events using EventListener attribute
		public void SendEvent(string eventName){

			if (!string.IsNullOrEmpty(eventName) && isRunning && agent != null)
				agent.gameObject.SendMessage("OnCustomEvent", eventName, SendMessageOptions.DontRequireReceiver);
		}

		///Sends an event to all graphs
		public static void SendGlobalEvent(string eventName){

			foreach(Graph graph in FindObjectsOfType<Graph>())
				graph.SendEvent(eventName);
		}

		new public void SendMessage(string name){
			SendMessage(name, null);
		}

		///Similar to Unity SendMessage + it sends the message to all tasks of the graph as well.
		new public void SendMessage(string name, object argument){

			if (agent)
				agent.gameObject.SendMessage(name, argument, SendMessageOptions.DontRequireReceiver);

			SendTaskMessage(name, argument);
		}

		public void SendTaskMessage(string name){
			SendTaskMessage(name, null);
		}

		public void NotifyTask(string method){
			SendTaskMessage(method);
		}

		///Send a message to all tasks in this graph and nested graphs.
		public void SendTaskMessage(string name, object argument){

			foreach (Task c in nodesRoot.GetComponentsInChildren<Task>(true)){
				MethodInfo method = c.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				if (method != null){
					if (method.GetParameters().Length == 0){
						method.Invoke(c, null);
					}
					else
					if (method.GetParameters().Length == 1){
						method.Invoke(c, new object[] {argument} );
					}
				}
			}
		}

		public void StartGraph(){
			StartGraph(this.agent, this.blackboard, null);
		}

		///Start the graph with the already assigned agent and blackboard
		///optionaly providing a callback for when it is finished
		public void StartGraph(System.Action callback){
			StartGraph(this.agent, this.blackboard, callback);
		}

		public void StartGraph(Component agent){
			StartGraph(agent, this.blackboard, null);
		}

		public void StartGraph(Component agent, System.Action callback){
			StartGraph(agent, this.blackboard, callback);
		}

		public void StartGraph(Component agent, Blackboard blackboard){
			StartGraph(agent, blackboard, null);
		}

		///Start the graph for the agent and blackboard provided.
		///Optionally provide a callback for when the graph stops or ends
		public void StartGraph(Component agent, Blackboard blackboard, System.Action callback){

			if (isRunning){
				Debug.LogWarning("Graph allready Active");
				return;
			}

			if (primeNode == null){
				Debug.LogWarning("You tried to Start a Graph that has no Start Node.", gameObject);
				return;
			}

			if (agent == null && allowNullAgent == false){
				Debug.LogWarning("You've tried to start a graph with null Agent.");
				return;
			}
			
			if (blackboard == null && agent != null){
				Debug.Log("Graph started with null blackboard. Looking for blackboard on agent '" + agent.gameObject + "'", agent.gameObject);
				blackboard = agent.GetComponent<Blackboard>();
			}

			this.blackboard = blackboard;
			this.agent = agent;
			if (callback != null)
				this.FinishCallback = callback;

			isRunning = true;
			SendTaskMessage("OnGraphStarted");
			if (!isPaused){
				foreach (Node node in allNodes)
					node.OnGraphStarted();
			}

			UpdateNodeIDsInGraph();
			MonoManager.current.AddMethod(OnGraphUpdate);
			OnGraphStarted();
			isPaused = false;
		}

		///Override for graph specific stuff to run when the graph is started or resumed
		virtual protected void OnGraphStarted(){

		}

		///Override for graph specific per frame logic. Called every frame if the graph is running
		virtual protected void OnGraphUpdate(){

		}

		///Stops the graph completely and resets all nodes.
		public void StopGraph(){

			if (!isRunning && !isPaused)
				return;

			MonoManager.current.RemoveMethod(OnGraphUpdate);
			isRunning = false;
			isPaused = false;
			SendTaskMessage("OnGraphStoped");

			foreach(Node node in allNodes){
				node.ResetNode(false);
				node.OnGraphStoped();
			}

			OnGraphStoped();

			if (FinishCallback != null)
				FinishCallback();
			FinishCallback = null;
		}

		///Override for graph specific stuff to run when the graph is stoped
		virtual protected void OnGraphStoped(){

		}

		///Pauses the graph from updating as well as notifying all nodes and tasks.
		public void PauseGraph(){

			if (!isRunning)
				return;

			MonoManager.current.RemoveMethod(OnGraphUpdate);
			isRunning = false;
			isPaused = true;
			SendTaskMessage("OnGraphPaused");

			foreach (Node node in allNodes)
				node.OnGraphPaused();

			OnGraphPaused();
		}

		///Override this for when the graph is paused
		virtual protected void OnGraphPaused(){

		}

		protected void OnDestroy(){
			MonoManager.current.RemoveMethod(OnGraphUpdate);
		}

		///Get a node by it's ID, null if not found
		public Node GetNodeWithID(int searchID){

			if (searchID <= allNodes.Count && searchID >= 0)	
				return allNodes[searchID - 1];

			return null;
		}

		///Get a node by it's tag name
		public T GetNodeWithTag<T>(string name) where T:Node{

			foreach (T node in allNodes.OfType<T>()){
				if (node.tagName == name)
					return node;
			}
			return default(T);
		}

		///Get all nodes taged with such tag name
		public List<T> GetNodesWithTag<T>(string name) where T:Node{

			var nodes = new List<T>();
			foreach (T node in allNodes.OfType<ITaggable>()){
				if (node.tagName == name)
					nodes.Add(node);
			}
			return nodes;
		}

		///Get all taged nodes regardless tag name
		public List<T> GetAllTagedNodes<T>() where T:Node{

			var nodes = new List<T>();
			foreach (T node in allNodes.OfType<ITaggable>()){
				if (!string.IsNullOrEmpty(node.tagName))
					nodes.Add(node);
			}
			return nodes;
		}

		///Get a node by it's name
		public T GetNodeWithName<T>(string name) where T:Node{
			foreach(T node in allNodes){
				if (StripNameColor(node.nodeName).ToLower() == name.ToLower())
					return node;
			}
			return default(T);
		}

		//removes the text color that some nodes add with html tags
		string StripNameColor(string name){
			if (name.StartsWith("<") && name.EndsWith(">")){
				name = name.Replace( name.Substring (0, name.IndexOf(">")+1), "" );
				name = name.Replace( name.Substring (name.IndexOf("<"), name.LastIndexOf(">")+1 - name.IndexOf("<")), "" );
			}
			return name;
		}

		///Get all nodes of the graph that have no incomming connections
		public List<Node> GetRootNodes(){

			var rootNodes = new List<Node>();
			foreach(Node node in allNodes){
				if (node.inConnections.Count == 0)
					rootNodes.Add(node);
			}

			return rootNodes;
		}

		///Update the IDs of the nodes in the graph. Is automatically called whenever a change happens in the graph by the adding removing connecting etc.
		public void UpdateNodeIDsInGraph(){

			int lastID = 0;

			//start with the prime node
			if (primeNode != null)
				lastID = primeNode.AssignIDToGraph(this, lastID);

			//then set remaining nodes that are not connected
			foreach (Node node in allNodes.ToArray()){
				if (node.inConnections.Count == 0)
					lastID = node.AssignIDToGraph(this, lastID);
			}

			allNodes = allNodes.OrderBy(node => node.ID).ToList();

			//reset the check
			foreach (Node node in allNodes.ToArray())
				node.ResetRecursion();
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		public string graphComments = string.Empty;
		public bool showComments = true;
		private Graph _nestedGraphView;
		private Rect blackboardRect = new Rect(15, 55, 0, 0);
		private Rect inspectorRect = new Rect(15, 55, 0, 0);
		private Vector2 inspectorScrollPos;

		public static System.Action PostGUI;
		private static Object _currentSelection;
		public static Vector2 scrollOffset;
		public static bool allowClick;

		//
		public static bool showNodeInfo{
			get {return EditorPrefs.GetBool("NodeCanvas_showNodeInfo");}
			set {EditorPrefs.SetBool("NodeCanvas_showNodeInfo", value);}
		}

		public static bool isLocked{
			get {return EditorPrefs.GetBool("NodeCanvas_isLocked");}
			set {EditorPrefs.SetBool("NodeCanvas_isLocked", value);}
		}		

		public static bool iconMode{
			get {return EditorPrefs.GetBool("NodeCanvas_iconMode");}
			set {EditorPrefs.SetBool("NodeCanvas_iconMode", value);}
		}

		public static int curveMode{
			get {return EditorPrefs.GetInt("NodeCanvas_curveMode");}
			set {EditorPrefs.SetInt("NodeCanvas_curveMode", value);}
		}

		public static bool doSnap{
			get{return EditorPrefs.GetBool("NodeCanvas_doSnap");}
			set{EditorPrefs.SetBool("NodeCanvas_doSnap", value);}
		}

		private bool showBlackboard{
			get {return EditorPrefs.GetBool("NodeCanvas_showBlackboard");}
			set {EditorPrefs.SetBool("NodeCanvas_showBlackboard", value);}
		}

		private bool autoConnect{
			get {return EditorPrefs.GetBool("NodeCanvas_autoConnect");}
			set {EditorPrefs.SetBool("NodeCanvas_autoConnect", value);}
		}
		//

		public static bool useExternalInspector{get;set;}

		public static Object currentSelection{
			get
			{
				//special check to bypass unity operator 
				if (_currentSelection as Object == null)
					return null;
				return _currentSelection;
			}
			set {GUIUtility.keyboardControl = 0; _currentSelection = value;}
		}

		public Graph nestedGraphView{
			get {return _nestedGraphView;}
			set
			{
				Undo.RecordObject(this, "Change View");
				if (value)
					value.nestedGraphView = null;

				currentSelection = null;
				_nestedGraphView = value;
				if (_nestedGraphView != null){
					_nestedGraphView.agent = this.agent;
					_nestedGraphView.blackboard = this.blackboard;
				}
			}
		}

		private Node focusedNode{
			get
			{
				if (currentSelection == null)
					return null;
				if (typeof(Node).IsAssignableFrom(currentSelection.GetType()))
					return currentSelection as Node;			
				return null;
			}
		}

		private Connection focusedConnection{
			get
			{
				if (currentSelection == null)
					return null;
				if (typeof(Connection).IsAssignableFrom(currentSelection.GetType()))
					return currentSelection as Connection;			
				return null;
			}
		}

		///

		[ContextMenu("Reset")]
		protected void Reset(){
			//Disabled for safety
		}

		[ContextMenu("Copy Component")]
		protected void CopyComponent(){
			Debug.Log("Unsupported");
		}

		[ContextMenu("Paste Component Values")]
		protected void PasteComponentValues(){
			Debug.Log("Unsupported");
		}

		///Create a new nested graph for the provided INestedNode parent.
		public static Graph CreateNested(INestedNode parent, System.Type type, string name){
			var newGraph = new GameObject(name).AddComponent(type) as Graph;
			newGraph.graphName = name;
			Undo.RegisterCreatedObjectUndo(newGraph.gameObject, "New Graph");
			
			if (parent != null){
				Undo.RecordObject(parent as Node, "New Graph");
				newGraph.transform.parent = (parent as Node).graph.transform;
				newGraph.transform.localPosition = Vector3.zero;
			}

			parent.nestedGraph = newGraph;
			return newGraph;
		}

		///Add a new node to this graph
		public Node AddNewNode(System.Type nodeType){

			if (!baseNodeType.IsAssignableFrom(nodeType)){
				Debug.Log(nodeType + " can't be assigned to " + this.GetType() + " graph");
				return null;
			}

			var newNode = Node.Create(this, nodeType);
			Undo.RegisterCreatedObjectUndo(newNode.gameObject, "New Node");
			Undo.RecordObject(this, "New Node");
			allNodes.Add(newNode);

			if (primeNode == null)
				primeNode = newNode;

			Undo.RecordObject(this, "New Node");
			UpdateNodeIDsInGraph();

			return newNode;
		}

		///Disconnects and then removes a node from this graph by ID
		public void RemoveNode(int id){
			RemoveNode(GetNodeWithID(id));
		}

		///Disconnects and then removes a node from this graph
		public void RemoveNode(Node nodeToDelete){
 
			foreach (Connection outConnection in nodeToDelete.outConnections.ToArray())
				RemoveConnection(outConnection);

			foreach (Connection inConnection in nodeToDelete.inConnections.ToArray())
				RemoveConnection(inConnection);

			Undo.RecordObject(this, "Delete Node");
			allNodes.Remove(nodeToDelete);
			Undo.DestroyObjectImmediate(nodeToDelete.gameObject);

			if (nodeToDelete == primeNode)
				primeNode = GetNodeWithID(1);

			Undo.RecordObject(this, "Delete Node");
			UpdateNodeIDsInGraph();

			INestedNode nestNode = nodeToDelete as INestedNode;
			if (nestNode != null && nestNode.nestedGraph != null){
				var isPrefab = PrefabUtility.GetPrefabType(nestNode.nestedGraph) == PrefabType.Prefab;
				if (!isPrefab && EditorUtility.DisplayDialog("Deleting Nested Node", "Delete assign nested graph as well?", "Yes", "No"))
					Undo.DestroyObjectImmediate(nestNode.nestedGraph.gameObject);
			}
		}
		
		///Connect two nodes together to the next available port of the source node
		public Connection ConnectNode(Node sourceNode, Node targetNode){
			return ConnectNode(sourceNode, targetNode, sourceNode.outConnections.Count);
		}

		///Connect two nodes together to a specific port index of the source node
		public Connection ConnectNode(Node sourceNode, Node targetNode, int indexToInsert){

			if (targetNode.IsNewConnectionAllowed(sourceNode) == false)
				return null;

			Undo.RecordObject(sourceNode, "New Connection");
			Undo.RecordObject(targetNode, "New Connection");
			var newConnection = Connection.Create(sourceNode, targetNode, indexToInsert);
			Undo.RegisterCreatedObjectUndo(newConnection.gameObject, "New Connection");

			Undo.RecordObject(sourceNode, "New Connection");
			sourceNode.OnPortConnected(indexToInsert);

			Undo.RecordObject(this, "New Connection");
			UpdateNodeIDsInGraph();
			return newConnection;
		}

		///Removes a connection
		public void RemoveConnection(Connection connection){

			Undo.RecordObject(connection.sourceNode, "Delete Connection");			
			connection.sourceNode.OnPortDisconnected(connection.sourceNode.outConnections.IndexOf(connection));

			if (Application.isPlaying)
				connection.ResetConnection();

			Undo.RecordObject(connection.targetNode, "Delete Connection");
			Undo.RecordObject(connection.sourceNode, "Delete Connection");
			connection.sourceNode.outConnections.Remove(connection);
			connection.targetNode.inConnections.Remove(connection);
			Undo.DestroyObjectImmediate(connection.gameObject);

			Undo.RecordObject(this, "Delete Connection");
			UpdateNodeIDsInGraph();
		}

		///Clears the whole graph
		public void ClearGraph(){

			foreach(INestedNode node in allNodes.OfType<INestedNode>() ){
				if (node.nestedGraph && node.nestedGraph.transform.parent == this.transform){
					Undo.RecordObject(node as Node, "Delete Nested");
					Undo.DestroyObjectImmediate(node.nestedGraph.gameObject);
				}
			}

			Undo.RecordObject(this, "Clear Graph");
			allNodes.Clear();
			primeNode = null;

			Undo.DestroyObjectImmediate(nodesRoot.gameObject);
		}

		//This is called while within Begin/End windows and ScrollArea from the GraphEditor. This is the main function that calls others
		public void ShowNodesGUI(Rect drawCanvas){

			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
		
			UpdateNodeIDsInGraph();

			if (primeNode)
				GUI.Box(new Rect(primeNode.nodeRect.x, primeNode.nodeRect.y - 20, primeNode.nodeRect.width, 20), "<b>START</b>");

			for (int i= 0; i < allNodes.Count; i++){
				
				//Panning nodes
				if (Event.current.button == 2 && Event.current.type == EventType.MouseDrag)
					allNodes[i].PanNode(Event.current.delta, false);

				if (RectContainsRect(drawCanvas, allNodes[i].nodeRect))
					allNodes[i].ShowNodeGUI();
			}

			//This better be done in seperate pass
			for (int i= 0; i < allNodes.Count; i++)
					allNodes[i].DrawNodeConnections();
		}

		bool RectContainsRect(Rect a, Rect b){
			return a.Contains(   new Vector2( (b.x > a.x? b.x : b.xMax),  (b.y > a.y? b.y : b.yMax) )  );
		}

		//This is called outside
		public void ShowGraphControls(){

			ShowToolbar();
			ShowInspectorGUI();
			ShowBlackboardGUI();
			ShowGraphCommentsGUI();
			DoGraphControls();
			//AcceptDrops();

			if (PostGUI != null){
				PostGUI();
				PostGUI = null;
			}
		}

		//TODO
		void AcceptDrops(){
			var e = Event.current;
			if (e.type == EventType.DragUpdated && DragAndDrop.objectReferences.Length == 1)
				DragAndDrop.visualMode = DragAndDropVisualMode.Link;

			if (e.type == EventType.DragPerform){
				if (DragAndDrop.objectReferences.Length != 1)
					return;
				DragAndDrop.AcceptDrag();
				OnDropAccepted(DragAndDrop.objectReferences[0]);
			}
		}

		virtual public void OnDropAccepted(Object o){

		}

		//This is called outside Begin/End Windows from GraphEditor.
		void ShowToolbar(){

			Event e = Event.current;
		
			GUILayout.BeginHorizontal(EditorStyles.toolbar);
			GUI.backgroundColor = new Color(1f,1f,1f,0.5f);

			if (GUILayout.Button("Select", EditorStyles.toolbarButton, GUILayout.Width(60)))
				Selection.activeObject = agent != null? agent : this;

			GUILayout.Space(10);

			showBlackboard = GUILayout.Toggle(showBlackboard, "Blackboard", EditorStyles.toolbarButton);
			showComments = GUILayout.Toggle(showComments, "Comments", EditorStyles.toolbarButton);

			GUILayout.Space(10);

			if (GUILayout.Button("Options", new GUIStyle(EditorStyles.toolbarDropDown))){
				var menu = new GenericMenu();
				menu.AddItem (new GUIContent ("Icon Mode"), iconMode, delegate{iconMode = !iconMode;});
				menu.AddItem (new GUIContent ("Help Info"), showNodeInfo, delegate{showNodeInfo = !showNodeInfo;});
				menu.AddItem (new GUIContent ("Auto Connect"), autoConnect, delegate{autoConnect = !autoConnect;});
				menu.AddItem (new GUIContent ("Grid Snap"), doSnap, delegate{doSnap = !doSnap;});
				menu.AddItem (new GUIContent ("Curve Mode/Smooth"), curveMode == 0, delegate{curveMode = 0;});
				menu.AddItem (new GUIContent ("Curve Mode/Stepped"), curveMode == 1, delegate{curveMode = 1;});
				menu.ShowAsContext();
			}

			GUILayout.Space(10);
			GUILayout.FlexibleSpace();

			isLocked = GUILayout.Toggle(isLocked, "Lock", EditorStyles.toolbarButton);

			GUI.backgroundColor = new Color(1, 0.8f, 0.8f, 1);
			if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50))){
				if (EditorUtility.DisplayDialog("Clear Canvas", "This will delete all nodes of the currently viewing graph!\nAre you sure?", "DO IT", "NO!")){
					ClearGraph();
					e.Use();
					return;
				}
			}

			GUILayout.EndHorizontal();
			GUI.backgroundColor = Color.white;
		}

		void DoGraphControls(){

			var e = Event.current;
			//variable is set as well, so that  nodes know if they can be clicked
			allowClick = !inspectorRect.Contains(e.mousePosition) && !blackboardRect.Contains(e.mousePosition);
			if (allowClick){
	
				//canvas click to deselect all
				if (e.button == 0 && e.isMouse && e.type == EventType.MouseDown){
					currentSelection = null;
					return;
				}

				//right click to add node
				if (e.button == 1 && e.type == EventType.MouseDown){
					var pos = e.mousePosition + scrollOffset;
					System.Action<System.Type> Selected = delegate(System.Type type){
						
						var newNode = AddNewNode(type);
						newNode.nodeRect.center = pos;
						if (autoConnect && focusedNode != null && (focusedNode.outConnections.Count < focusedNode.maxOutConnections || focusedNode.maxOutConnections == -1) ){
							ConnectNode(focusedNode, newNode);
						} else {
							currentSelection = newNode;
						}
					};

					EditorUtils.ShowTypeSelectionMenu(baseNodeType, Selected );
					e.Use();
				}
			}

			//Contract all nodes
			if (e.isKey && e.alt && e.keyCode == KeyCode.Q){
				foreach (Node node in allNodes){
					Undo.RecordObject(node, "Contract All Nodes");
					node.nodeRect.width = Node.minSize.x;
					node.nodeRect.height = Node.minSize.y;
					Undo.CollapseUndoOperations(Undo.GetCurrentGroup());
				}
				e.Use();
			}

			//Duplicate
			if (e.isKey && e.control && e.keyCode == KeyCode.D && focusedNode != null){
				currentSelection = focusedNode.Duplicate();
				e.Use();
			}
		}

		//Show the comments window
		void ShowGraphCommentsGUI(){

			if (showComments && !string.IsNullOrEmpty(graphComments)){
				GUI.backgroundColor = new Color(1f,1f,1f,0.3f);
				GUI.Box(new Rect(15, Screen.height - 100, 330, 60), graphComments, new GUIStyle("textArea"));
				GUI.backgroundColor = Color.white;
			}
		}

		//This is the window shown at the top left with a GUI for extra editing opions of the selected node.
		void ShowInspectorGUI(){
			
			if (!focusedNode && !focusedConnection || useExternalInspector){
				inspectorRect.height = 0;
				return;
			}

			inspectorRect.width = 330;
			inspectorRect.x = 15;
			inspectorRect.y = 30;
			GUISkin lastSkin = GUI.skin;
			GUI.Box(inspectorRect, "", "windowShadow");

			var viewRect = new Rect(inspectorRect.x + 1, inspectorRect.y, inspectorRect.width + 18, Screen.height - inspectorRect.y - 30);
			inspectorScrollPos = GUI.BeginScrollView(viewRect, inspectorScrollPos, inspectorRect);

			GUILayout.BeginArea(inspectorRect, (focusedNode? focusedNode.nodeName : "Connection"), new GUIStyle("editorPanel"));
			GUILayout.Space(5);
			GUI.skin = null;

			if (focusedNode)
				focusedNode.ShowNodeInspectorGUI();
			
			if (focusedConnection)
				focusedConnection.ShowConnectionInspectorGUI();

			GUILayout.Box("", GUILayout.Height(5), GUILayout.Width(inspectorRect.width - 10));
			GUI.skin = lastSkin;
			if (Event.current.type == EventType.Repaint)
				inspectorRect.height = GUILayoutUtility.GetLastRect().yMax + 5;

			GUILayout.EndArea();
			GUI.EndScrollView();

			if (GUI.changed && currentSelection != null)
				EditorUtility.SetDirty(currentSelection);
		}


		//Show the target blackboard window
		void ShowBlackboardGUI(){

			if (!showBlackboard || blackboard == null){
				blackboardRect.height = 0;
				return;
			}

			blackboardRect.width = 330;
			blackboardRect.x = Screen.width - 350;
			blackboardRect.y = 30;
			GUISkin lastSkin = GUI.skin;
			GUI.Box(blackboardRect, "", "windowShadow" );

			GUILayout.BeginArea(blackboardRect, "Variables", new GUIStyle("editorPanel"));
			GUILayout.Space(5);
			GUI.skin = null;

			blackboard.ShowVariablesGUI();

			GUILayout.Box("", GUILayout.Height(5), GUILayout.Width(blackboardRect.width - 10));
			GUI.skin = lastSkin;
			if (Event.current.type == EventType.Repaint)
				blackboardRect.height = GUILayoutUtility.GetLastRect().yMax + 5;
			GUILayout.EndArea();		
		}

		#endif
	}
}
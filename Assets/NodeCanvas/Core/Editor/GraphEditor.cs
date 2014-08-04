using UnityEngine;
using System.Collections;
using UnityEditor;
using NodeCanvas;

namespace NodeCanvasEditor{

	public class GraphEditor : EditorWindow{

		public static GraphEditor current;
		public static Graph currentGraph;

		private Graph _rootGraph;
		private int rootGraphID;

		private GraphOwner _targetOwner;
		private int targetOwnerID;

		private Rect canvas= new Rect(0, 0, 200, 200);
		private Rect bottomRect;
		private GUISkin guiSkin;
		private Vector2 scrollPos= Vector2.zero;
		private float topMargin = 20;
		private float bottomMargin = 5;
		private int repaintCounter;

		private Graph rootGraph{
			get
			{
				if (_rootGraph == null)
					_rootGraph = EditorUtility.InstanceIDToObject(rootGraphID) as Graph;
				return _rootGraph;
			}
			set
			{
				_rootGraph = value;
				if (value != null)
					rootGraphID = value.GetInstanceID();
			}
		}

		private GraphOwner targetOwner{
			get
			{
				if (_targetOwner == null)
					_targetOwner = EditorUtility.InstanceIDToObject(targetOwnerID) as GraphOwner;
				return _targetOwner;
			}
			set
			{
				_targetOwner = value;
				targetOwnerID = value != null? value.GetInstanceID() : 0;
			}
		}

		private Rect actualCanvas{
			get { return new Rect(5, topMargin, position.width - 10, position.height - (topMargin + bottomMargin));	}
		}

		private Rect canvasLimits{
			get
			{
				float minX = 0;
				float minY = 0;
				float maxX = 0;
				float maxY = 0;
				
				for (int i= 0; i < currentGraph.allNodes.Count; i++){
					var node = currentGraph.allNodes[i];
					minX = Mathf.Min(minX, node.nodeRect.x-20);
					minY = Mathf.Min(minY, node.nodeRect.y-20);
					maxX = Mathf.Max(maxX, node.nodeRect.xMax+20);
					maxY = Mathf.Max(maxY, node.nodeRect.yMax+20);
				}

				return new Rect(minX, minY, maxX, maxY);				
			}
		}

		void OnEnable(){
			
			current = this;
			title = "NodeCanvas";
			guiSkin = EditorGUIUtility.isProSkin? (GUISkin)Resources.Load("NodeCanvasSkin") : (GUISkin)Resources.Load("NodeCanvasSkinLight");
			Repaint();
		}

		void OnInspectorUpdate(){
			Repaint();
		}

		void OnGUI(){

			if (EditorApplication.isCompiling){
				ShowNotification(new GUIContent("Compiling Please Wait..."));
				return;			
			}

			if (targetOwner != null)
				rootGraph = targetOwner.behaviour;

			if (rootGraph == null){
				ShowNotification(new GUIContent("Please select a GameObject with a Graph Owner or a Graph itself"));
				return;
			}

			currentGraph = GetCurrentGraph(rootGraph);

	        if (PrefabUtility.GetPrefabType(currentGraph) == PrefabType.Prefab){
	            ShowNotification(new GUIContent("Editing is not allowed when prefab asset is selected for safety. Please place the prefab in a scene, edit and apply it"));
	            return;
	        }

			GUI.skin = guiSkin;
			var e = Event.current;

			if (mouseOverWindow == this && (e.isMouse || e.isKey) )
				repaintCounter += 2;

			if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed"){
                GUIUtility.hotControl = 0;
                GUIUtility.keyboardControl = 0;
				return;
			}
			if (e.type == EventType.MouseUp)
				SnapNodes();

			if (e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.KeyUp){
				if (PrefabUtility.GetPrefabType(currentGraph) == PrefabType.PrefabInstance){
					ShowNotification(new GUIContent("Prefab Disconnected. Apply when done."));
					PrefabUtility.DisconnectPrefabInstance(currentGraph);
				}
			}

			//Canvas Scroll pan
			if (e.button == 0 && e.isMouse && e.type == EventType.MouseDrag && e.alt){
				scrollPos += e.delta * 2;
				e.Use();
			}

			Graph.scrollOffset = scrollPos;

			//Set canvas limits for the nodes
			canvas.width = canvasLimits.width;
			canvas.height = canvasLimits.height;

			GUI.Box(actualCanvas, "NodeCanvas v1.5.1", "canvasBG");
			DrawGrid();
			
			//Begin windows and ScrollView for the nodes.
			scrollPos = GUI.BeginScrollView (actualCanvas, scrollPos, canvas);

			BeginWindows();
			currentGraph.ShowNodesGUI(new Rect(scrollPos.x, scrollPos.y, actualCanvas.width, actualCanvas.height));
			EndWindows();
			GUI.EndScrollView();
			//End windows and scrollview for the nodes.

			//Hierarchy
			GUILayout.BeginArea(new Rect(20, topMargin + 5, Screen.width, Screen.height));
			GetDrawHierarchy(rootGraph);
			GUILayout.EndArea();
			//

			currentGraph.ShowGraphControls();

			if (repaintCounter > 0 || currentGraph.isRunning){
				repaintCounter = Mathf.Max (repaintCounter -1, 0);
				Repaint();
			}

			GUI.Box(actualCanvas,"", "canvasBorders");
			GUI.skin = null;
			GUI.color = Color.white;
			GUI.backgroundColor = Color.white;
		}

		//Draw a simple grid
		void DrawGrid(){

			Handles.color = new Color(0,0,0,0.07f);
			for (int i = 0; i < Screen.width; i++){
				if (i % 30 == 0)
					Handles.DrawLine(new Vector3(i,0,0), new Vector3(i,Screen.height,0));
				if (i % 15 == 0)
					Handles.DrawLine(new Vector3(i,0,0), new Vector3(i,Screen.height,0));
			}
			
			var yOffset = -7 - scrollPos.y;
			for (int i = 0; i < Screen.height + scrollPos.y; i++){
				if (i % 30 == 0)
					Handles.DrawLine(new Vector3(0, i + yOffset, 0), new Vector3(Screen.width, i + yOffset, 0));
				if (i % 15 == 0)
					Handles.DrawLine(new Vector3(0, i + yOffset, 0), new Vector3(Screen.width, i + yOffset, 0));
			}
			Handles.color = Color.white;
		}

		//Recursively get the currenlty showing graph starting from the parent
		Graph GetCurrentGraph(Graph parent){
			if (parent.nestedGraphView == parent || parent.nestedGraphView == null)
				return parent;
			return GetCurrentGraph(parent.nestedGraphView);
		}

		//This is the hierarchy shown at top middle. Recusrsively show the nested path
		void GetDrawHierarchy(Graph root){

			if (Graph.currentSelection != null && !Graph.useExternalInspector)
				return;

			var agentInfo = root.agent != null? root.agent.gameObject.name : "No Agent";
			var bbInfo = root.blackboard? root.blackboard.blackboardName : "No Blackboard";

			GUI.color = new Color(1f,1f,1f,0.5f);
			GUILayout.BeginVertical();
			if (root.nestedGraphView == this || root.nestedGraphView == null){

				var labelStyle = new GUIStyle("label");
//				labelStyle.alignment = useExternalInspector? TextAnchor.UpperLeft : TextAnchor.UpperCenter;
				if (root.agent == null && root.blackboard == null){

					GUILayout.Label("<size=18>" + root.graphName + "</size>", labelStyle);	
		
				} else {

					GUILayout.Label("<size=18>" + root.graphName + "</size>" + "\n<size=10>" + agentInfo + " | " + bbInfo + "</size>", labelStyle);
				}

			} else {

				GUILayout.BeginHorizontal();
/*				
				if (!useExternalInspector)
					GUILayout.FlexibleSpace();
*/
				if (GUILayout.Button("^ " + root.graphName, new GUIStyle("button"))){
					root.nestedGraphView = null;
					return;
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (root.nestedGraphView != null)
					GetDrawHierarchy(root.nestedGraphView);
			}

			GUILayout.EndVertical();
			GUI.color = Color.white;
		}

		//Snap all nodes
		void SnapNodes(){

			if (!Graph.doSnap)
				return;

			foreach(Node node in currentGraph.allNodes){
				var snapedPos = new Vector2(node.nodeRect.xMin, node.nodeRect.yMin);
				snapedPos.y = Mathf.Round(snapedPos.y / 15) * 15;
				snapedPos.x = Mathf.Round(snapedPos.x / 15) * 15;
				node.nodeRect = new Rect(snapedPos.x, snapedPos.y, node.nodeRect.width, node.nodeRect.height);
			}
		}

		//Change viewing graph
		void OnSelectionChange(){
			
			if (Selection.activeGameObject != null){
				var foundOwner = Selection.activeGameObject.GetComponent<GraphOwner>();
				if (!Graph.isLocked && foundOwner != null){
					targetOwner = foundOwner;
					Graph.currentSelection = null;
				}
			}
		}

	    //Opeining the window for a graph owner
	    public static GraphEditor OpenWindow(GraphOwner owner){
	    	var window = OpenWindow(owner.behaviour, owner, owner.blackboard);
	    	window.targetOwner = owner;
	    	return window;
	    }

	    //For opening the window from gui button in the nodegraph's Inspector.
	    public static GraphEditor OpenWindow(Graph newGraph){
	    	return OpenWindow(newGraph, newGraph.agent, newGraph.blackboard);
	    }

	    //...
	    public static GraphEditor OpenWindow(Graph newGraph, Component agent, Blackboard blackboard) {

	        GraphEditor window = GetWindow(typeof(GraphEditor)) as GraphEditor;
	        newGraph.agent = agent;
	        newGraph.blackboard = blackboard;
	        newGraph.SendTaskOwnerDefaults();
	        newGraph.nestedGraphView = null;
	        newGraph.UpdateNodeIDsInGraph();

	        window.rootGraph = newGraph;
	        window.targetOwner = null;
	        Graph.currentSelection = null;
	        return window;
	    }
	}
}
using UnityEditor;
using UnityEngine;
using System.Collections;
using NodeCanvas;

namespace NodeCanvasEditor{

	[InitializeOnLoad]
	public class HierarchyIcons{
		
		static HierarchyIcons(){
/*
			if (!System.IO.Directory.Exists(Application.dataPath + "/Gizmos"))
				AssetDatabase.CreateFolder("Assets", "Gizmos");
			AssetDatabase.MoveAsset("Assets/NodeCanvas/GraphOwner.png", "Assets/Gizmos/GraphOwner.png");
*/
			EditorApplication.hierarchyWindowItemOnGUI += ShowIcon;
		}

		static void ShowIcon(int ID, Rect r){
			r.x = r.xMax - 18;
			r.width = 18;
			var go = EditorUtility.InstanceIDToObject(ID) as GameObject;
			if (go != null){
				if (go.GetComponent<GraphOwner>() != null)
					GUI.Label(r, "♟");
				if (go.GetComponent<Graph>() != null)
					GUI.Label(r, "⑆");
			}
		}
	}

	public class GraphOwnerInspector : Editor {

		private string debugEvent;

		GraphOwner owner{
			get{return target as GraphOwner;}
		}

		void OnDestroy(){

			if (owner == null){
				if (owner.behaviour != null){
					var selectedOption = EditorUtility.DisplayDialogComplex("Removing Graph Owner", "When removing Owner, it's assigned behaviour is not deleted automaticaly since it might be shared amongst many Owners.\nDo you want to delete the assigned Graph?", "Yes", "No", "No & Select Graph");
					if (selectedOption == 0)
						Undo.DestroyObjectImmediate(owner.behaviour.gameObject);
					if (selectedOption == 2)
						Selection.activeObject = owner.behaviour;
				}
			}
		}

		public override void OnInspectorGUI(){

			Undo.RecordObject(owner, "Owner Inspector");
			
			var label = EditorUtils.TypeName(owner.graphType);

			if (owner.behaviour == null){
				
				EditorGUILayout.HelpBox(label + "Owner needs " + label + ". Assign or Create a new one", MessageType.Info);
				if (GUILayout.Button("CREATE NEW")){
				
					if (owner.behaviour == null){
						owner.behaviour = new GameObject(label).AddComponent(owner.graphType) as Graph;
						owner.behaviour.transform.parent = owner.transform;
						owner.behaviour.transform.localPosition = Vector3.zero;
						Undo.RegisterCreatedObjectUndo(owner.behaviour.gameObject, "New Graph");
					}

					owner.behaviour.agent = owner;
				}

				owner.behaviour = (Graph)EditorGUILayout.ObjectField(label, owner.behaviour, owner.graphType, true);
				return;
			}

			GUILayout.Space(10);

			owner.behaviour.graphName = EditorGUILayout.TextField(label + " Name", owner.behaviour.graphName);
            if (string.IsNullOrEmpty(owner.behaviour.graphName))
              owner.behaviour.graphName = owner.behaviour.gameObject.name;
			owner.behaviour.graphComments = GUILayout.TextArea(owner.behaviour.graphComments, GUILayout.Height(45));
			EditorUtils.TextFieldComment(owner.behaviour.graphComments);

			GUI.backgroundColor = EditorUtils.lightBlue;
			if (GUILayout.Button("OPEN BEHAVIOUR"))
				GraphEditor.OpenWindow(owner);
		
			GUI.backgroundColor = Color.white;
			GUI.color = new Color(1, 1, 1, 0.5f);
			owner.behaviour = (Graph)EditorGUILayout.ObjectField("Current " + label, owner.behaviour, owner.graphType, true);
			GUI.color = Color.white;

			owner.blackboard = (Blackboard)EditorGUILayout.ObjectField("Blackboard", owner.blackboard, typeof(Blackboard), true);
			owner.onEnable = (GraphOwner.EnableAction)EditorGUILayout.EnumPopup("On Enable", owner.onEnable);
			owner.onDisable = (GraphOwner.DisableAction)EditorGUILayout.EnumPopup("On Disable", owner.onDisable);

			OnExtraOptions();

			if (owner.behaviour != null && !(PrefabUtility.GetPrefabType(owner.behaviour) == PrefabType.Prefab) && Application.isPlaying){

				var behaviour = owner.behaviour;
				var pressed = new GUIStyle(GUI.skin.GetStyle("button"));
				pressed.normal.background = GUI.skin.GetStyle("button").active.background;

				GUILayout.BeginHorizontal("box");
				GUILayout.FlexibleSpace();

				if (GUILayout.Button(EditorUtils.playIcon, behaviour.isRunning || behaviour.isPaused? pressed : "button")){
					if (behaviour.isRunning || behaviour.isPaused) behaviour.StopGraph();
					else behaviour.StartGraph();
				}

				if (GUILayout.Button(EditorUtils.pauseIcon, behaviour.isPaused? pressed : "button")){	
					if (behaviour.isPaused) behaviour.StartGraph();
					else behaviour.PauseGraph();
				}

				OnGrapOwnerControls();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}


			EditorUtils.EndOfInspector();

			if (GUI.changed){
				EditorUtility.SetDirty(owner);
				EditorUtility.SetDirty(owner.behaviour);
			}
		}

		virtual protected void OnExtraOptions(){
			
		}

		virtual protected void OnGrapOwnerControls(){

		}
	}
}
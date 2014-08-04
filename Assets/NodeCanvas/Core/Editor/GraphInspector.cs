using UnityEngine;
using System.Collections;
using UnityEditor;
using NodeCanvas;

namespace NodeCanvasEditor{

    [CustomEditor(typeof(Graph))]
    public class GraphInspector : Editor {

        private Graph graph{
            get {return target as Graph;}
        }


        void OnEnable(){
            graph.nodesRoot.gameObject.hideFlags = Graph.doHide? HideFlags.HideInHierarchy : 0;
        }

        //hack
        void OnDestroy(){
            if (graph == null){
                var root = graph._nodesRoot;
                if (root != null)
                    DestroyImmediate(root.gameObject, true);
            }
        }

        public override void OnInspectorGUI(){

            Undo.RecordObject(graph, "Graph Inspector");

            if (IsPrefab())
                return;
            
            ShowBasicGUI();
            ShowTargetsGUI();

            if (GUI.changed)
                EditorUtility.SetDirty(graph);
        }

        //for use in derived inspectors
    	bool IsPrefab(){

            bool isPrefab= (PrefabUtility.GetPrefabType(graph) == PrefabType.Prefab);
            if (isPrefab)
                EditorGUILayout.HelpBox("Editing is not allowed when prefab asset is selected. Please place the prefab in a scene, edit and apply it", MessageType.Warning);
            return isPrefab;
        }


        void ShowBasicGUI(){

           if (graph.isRunning)
                EditorUtils.CoolLabel("Now Running!");

            GUILayout.Space(10);
            graph.graphName = EditorGUILayout.TextField("Graph Name", graph.graphName);
            if (string.IsNullOrEmpty(graph.graphName))
              graph.graphName = graph.gameObject.name;
            graph.graphComments = GUILayout.TextArea(graph.graphComments, GUILayout.Height(45));
            EditorUtils.TextFieldComment(graph.graphComments);

            GUI.backgroundColor = new Color(0.8f,0.8f,1);
            if (GUILayout.Button("EDIT IN NODECANVAS"))
                GraphEditor.OpenWindow(graph);
            GUI.backgroundColor = Color.white;
            EditorUtils.BoldSeparator();
        }

        void ShowTargetsGUI(){

            GUI.color = new Color(1f,1f,1f,0.2f);
            GUILayout.Label("Current Owner References (Dont edit unless you know why):");
            //EditorGUILayout.LabelField("Agent", graph.agent? graph.agent.name : "NONE");
            //EditorGUILayout.LabelField("Blackboard", graph.blackboard? graph.blackboard.name : "NONE");
            graph.agent = EditorGUILayout.ObjectField("Agent", graph.agent, typeof(Component), true) as Component;
            graph.blackboard = EditorGUILayout.ObjectField("Blackboard", graph.blackboard, typeof(Blackboard), true) as Blackboard;
            GUI.color = Color.white;
        }
    }
}
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;

namespace NodeCanvas.DialogueTrees{

	[AddComponentMenu("")]
	[Name("Dialogue")]
	[Category("Nested")]
	[Description("Execute a nested Dialogue Tree. When that nested Dialogue Tree is finished, this node will continue instead if it has a connection. The Actor selected will be used as 'Owner' for the nested Dialogue Tree.\nUseful for making reusable and contained Dialogue Trees.")]
	[Icon("Dialogue")]
	public class DLGNestedDLG : DLGNodeBase, INestedNode{

		[SerializeField]
		private DialogueTree _nestedDLG;

		private DialogueTree nestedDLG{
			get {return _nestedDLG;}
			set {_nestedDLG = value;}
		}

		public Graph nestedGraph{
			get {return nestedDLG;}
			set {nestedDLG = (DialogueTree)value;}
		}

		public override string nodeName{
			get {return "#" + ID + " DIALOGUE";}
		}

		protected override Status OnExecute(){

			if (!nestedDLG){
				DLGTree.StopGraph();
				return Error("No Nested Dialogue Tree assigned!");
			}


			DLGTree.currentNode = this;

			CopyActors();

			nestedDLG.StartGraph(finalActor, finalBlackboard, Continue );
			return Status.Running;
		}

		public override void OnGraphStoped(){

			if (nestedDLG)
				nestedDLG.StopGraph();
		}

		public override void OnGraphPaused(){

			if (nestedDLG)
				nestedDLG.PauseGraph();
		}

		private void CopyActors(){
			foreach (string actorName in this.DLGTree.dialogueActorNames){
				if (!nestedDLG.dialogueActorNames.Contains(actorName))
					nestedDLG.dialogueActorNames.Add(actorName);
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR
		
		protected override void OnNodeGUI(){

			if (nestedDLG){

				GUILayout.Label(nestedDLG.graphName);
			
			} else {

				if (GUILayout.Button("CREATE")){
					nestedDLG = (DialogueTree)Graph.CreateNested(this, typeof(DialogueTree), "Nested Dialogue");
					CopyActors();
				}
			}
		}

		protected override void OnNodeInspectorGUI(){

			base.OnNodeInspectorGUI();
			nestedDLG = EditorGUILayout.ObjectField("Nested Dialogue Tree", nestedDLG, typeof(DialogueTree), true) as DialogueTree;
			if (nestedDLG == DLGTree){
				Debug.LogWarning("Nested DialogueTree can't be itself! Please select another");
				nestedDLG = null;
			}

			if (nestedDLG != null)
				nestedDLG.graphName = EditorGUILayout.TextField("Name", nestedDLG.graphName);
		}

		#endif
	}
}
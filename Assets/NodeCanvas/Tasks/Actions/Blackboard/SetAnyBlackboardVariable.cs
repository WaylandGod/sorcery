using UnityEngine;
using System.Linq;
using NodeCanvas;
using NodeCanvas.Variables;
 
namespace NodeCanvas.Actions{
 
    [Category("✫ Blackboard")]
    [AgentType(typeof(Blackboard))]
    public class SetAnyBlackboardVariable : ActionTask {

        [RequiredField]
        public string targetVariableName;

        [SerializeField]
    	private BBVariableSet variableSet = new BBVariableSet();
       
        protected override string info{
            get {return string.Format("Set '{0}' = {1}", targetVariableName, variableSet.selectedBBVariable != null? variableSet.selectedBBVariable.ToString() : ""); }
        }

        protected override void OnExecute(){
           
            (agent as Blackboard).SetDataValue(targetVariableName, variableSet.selectedObjectValue);
            EndAction();
        }

        ////////////////////////////////////////
        ///////////GUI AND EDITOR STUFF/////////
        ////////////////////////////////////////
        #if UNITY_EDITOR
        
        [SerializeField]
        private int selectedIndex;

        protected override void OnTaskInspectorGUI(){

        	DrawDefaultInspector();
        	selectedIndex = UnityEditor.EditorGUILayout.Popup("Type", selectedIndex, variableSet.availableTypes.Select(t => EditorUtils.TypeName(t)).ToArray());
        	variableSet.selectedType = variableSet.availableTypes[selectedIndex];
        	if (variableSet.selectedBBVariable != null)
        		EditorUtils.BBVariableField("Value", variableSet.selectedBBVariable);
        }
        
        #endif
    }
}
 
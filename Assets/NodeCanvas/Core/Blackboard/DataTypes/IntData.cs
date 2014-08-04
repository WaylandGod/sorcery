using UnityEngine;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class IntData : VariableData{

		public int value;

		public override object objectValue{
			get {return value;}
			set {this.value = (int)value;}
		}

		//////////////////////////
		///////EDITOR/////////////
		//////////////////////////
		#if UNITY_EDITOR

		public override void ShowDataGUI(){
			GUI.backgroundColor = new Color(0.7f,1,0.7f);
			value = UnityEditor.EditorGUILayout.IntField(value, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
		}

		#endif
	}
}
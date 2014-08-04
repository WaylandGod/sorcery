using UnityEngine;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class StringData : VariableData{

		public string value = string.Empty;

		public override object objectValue{
			get {return value;}
			set {this.value = (string)value;}
		}


		//////////////////////////
		///////EDITOR/////////////
		//////////////////////////
		#if UNITY_EDITOR	

		public override void ShowDataGUI(){
			GUI.backgroundColor = new Color(0.5f,0.5f,0.5f);
			value = UnityEditor.EditorGUILayout.TextField(value, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
		}

		#endif
	}
}
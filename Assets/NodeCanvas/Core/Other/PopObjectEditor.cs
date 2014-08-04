#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System.Collections;

namespace NodeCanvasEditor{

	///A generic popup editor for all types
	public class PopObjectEditor : EditorWindow{

		private object targetObject;
		private System.Type targetType;

		void OnEnable(){
			title = "NCObjectEditor";
		}

		void OnGUI(){

			if (targetObject == null || targetType == null){
				Close();
				return;
			}

			GUILayout.Space(10);
			NodeCanvas.EditorUtils.GenericField(NodeCanvas.EditorUtils.TypeName(targetType), targetObject, targetType);
			Repaint();
		}

		public static void Show(object o, System.Type t){

			PopObjectEditor window = GetWindow(typeof(PopObjectEditor)) as PopObjectEditor;
			if (o == null)
				o = System.Activator.CreateInstance(t);

			window.targetObject = o;
			window.targetType = t;
			window.Show();
		}
	}
}

#endif
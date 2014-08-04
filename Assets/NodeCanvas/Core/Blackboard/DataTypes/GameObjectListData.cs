using UnityEngine;
using System.Collections.Generic;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class GameObjectListData : VariableData{

		public List<GameObject> value = new List<GameObject>();

		public override object objectValue{
			get {return value;}
			set {this.value = (List<GameObject>)value;}
		}

		public override object GetSerialized(){

			var goPaths = new List<string>();
			foreach (GameObject go in value){

				var obj = go;
				if (obj == null){
					goPaths.Add(null);
					continue;
				}

				string path= "/" + obj.name;

				while (obj.transform.parent != null){
					obj = obj.transform.parent.gameObject;
					path = "/" + obj.name + path;
				}
				
				goPaths.Add(path);
			}

			return goPaths;
		}

		public override void SetSerialized(object obj){

			var goPaths = new List<string>(obj as List<string>);
			foreach (string goPath in goPaths){
				var go = GameObject.Find(goPath);
				value.Add(go);
				if (go == null && !string.IsNullOrEmpty(goPath))
					Debug.LogWarning("GameObjectListData Failed to load a GameObject in the list. GameObject was not found in scene. Path '" + goPath + "'");
			}
		}
	}
}
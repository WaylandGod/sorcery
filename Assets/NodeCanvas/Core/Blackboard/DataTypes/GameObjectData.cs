using UnityEngine;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class GameObjectData : VariableData{

		public GameObject value;

		public override object objectValue{
			get {return value;}
			set {this.value = (GameObject)value;}
		}

		public override object GetSerialized(){

			GameObject obj= value;
			if (obj == null)
				return null;

			string path= "/" + obj.name;
			while (obj.transform.parent != null){
				obj = obj.transform.parent.gameObject;
				path = "/" + obj.name + path;
			}
			
			return path;
		}

		public override void SetSerialized(object obj){

			value = GameObject.Find(obj as string);
			if (value == null && !string.IsNullOrEmpty((string)obj))
				Debug.LogWarning("GameObjectData Failed to load. GameObject is not in scene. Path '" + (obj as string) + "'");
		}
	}
}
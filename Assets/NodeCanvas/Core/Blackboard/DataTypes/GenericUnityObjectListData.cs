/*

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class GenericUnityObjectListData : Data {

		public List<Object> value = new List<Object>();

		[SerializeField]
		private string _typeName = typeof(List<Object>).AssemblyQualifiedName;

		public System.Type type{
			get {return System.Type.GetType(_typeName);}
			set
			{
				_typeName = value.AssemblyQualifiedName;
				if (this.value != null && this.value.GetType() != value)
					this.value.Clear();
			}
		}

		public override System.Type varType{
			get {return type;}
		}

		public override object objectValue{
			get {return value;}
			set
			{
				this.value = (value as IList).Cast<Object>().ToList();
				if (value != null && value.GetType() != type)
					type = value.GetType();
			}
		}

		////////////////////////////////////////
		///////////GUI AND EDITOR STUFF/////////
		////////////////////////////////////////
		#if UNITY_EDITOR

		void OnValidate(){
			if (type == null)
				type = typeof(List<Object>);
		}

		void Reset(){
			type = typeof(List<Object>);
		}

		public override void ShowDataGUI(){

			if (GUILayout.Button("(" + EditorUtils.TypeName(type) + ")" + (value != null? value.Count.ToString() : ""), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true)))
				NodeCanvasEditor.PopObjectEditor.Show(value, type);

			if (GUILayout.Button("", GUILayout.Width(10), GUILayout.Height(14))){
				var menu = new UnityEditor.GenericMenu();
				menu.AddItem(new GUIContent("Object"), false, Selected, typeof(Object));
				menu.AddItem(new GUIContent("GameObject"), false, Selected, typeof(GameObject));
				menu.AddItem(new GUIContent("Component"), false, Selected, typeof(Component));
				menu.AddItem(new GUIContent("Transform"), false, Selected, typeof(Transform));
				menu.AddItem(new GUIContent("Rigidbody"), false, Selected, typeof(Rigidbody));
				menu.AddItem(new GUIContent("Collider"), false, Selected, typeof(Collider));
				menu.AddItem(new GUIContent("Collider2D"), false, Selected, typeof(Collider2D));
				menu.AddItem(new GUIContent("Texture2D"), false, Selected, typeof(Texture2D));
				menu.AddItem(new GUIContent("Material"), false, Selected, typeof(Material));
				menu.AddItem(new GUIContent("AudioClip"), false, Selected, typeof(AudioClip));
				menu.AddItem(new GUIContent("AnimationClip"), false, Selected, typeof(AnimationClip));
				menu.AddItem(new GUIContent("Sprite"), false, Selected, typeof(Sprite));
				menu.AddSeparator("/");

				foreach(System.Type t in EditorUtils.GetAssemblyTypes(typeof(Object))){
					
					if (typeof(Component).IsAssignableFrom(t))
						continue;

					var friendlyName = t.Assembly.GetName().Name + "/" + (string.IsNullOrEmpty(t.Namespace)? "" : t.Namespace + "/") + t.Name;
					menu.AddItem(new GUIContent("More/" + friendlyName), false, Selected, t);
				}

				menu.ShowAsContext();
			}
		}

		void Selected(object t){
			var genericT = typeof(List<>);
			type = genericT.MakeGenericType(new System.Type[]{ (System.Type)t} );
		}		

		#endif
	}
}
*/
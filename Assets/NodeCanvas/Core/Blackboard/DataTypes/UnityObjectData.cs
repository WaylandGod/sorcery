using UnityEngine;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class UnityObjectData : VariableData {

		public Object value;

		[SerializeField]
		private string _typeName = typeof(Object).AssemblyQualifiedName;

		private System.Type type{
			get {return value != null? value.GetType() : System.Type.GetType(_typeName); }
			set
			{
				_typeName = value.AssemblyQualifiedName;
				if (this.value != null && !value.IsAssignableFrom(this.value.GetType()))
					this.value = null;
			}
		}

		public override System.Type varType{
			get {return type;}
		}

		public override object objectValue{
			get {return value;}
			set
			{
				this.value = (Object)value;
				if (value != null && !type.IsAssignableFrom(value.GetType()))
					type = value.GetType();
			}
		}

		//////////////////////////
		///////EDITOR/////////////
		//////////////////////////
		#if UNITY_EDITOR

		void OnValidate(){
			if (type == null)
				type = typeof(Object);
		}

		void Reset(){
			type = typeof(Object);
		}

		public override void ShowDataGUI(){

			value = UnityEditor.EditorGUILayout.ObjectField(value, varType, true, GUILayout.MaxWidth(90), GUILayout.ExpandWidth(true)) as Object;

			if (GUILayout.Button("", GUILayout.Width(10), GUILayout.Height(14))){
				var menu = new UnityEditor.GenericMenu();
				menu.AddItem(new GUIContent("Object"), false, Selected, typeof(Object));
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
					var friendlyName = t.Assembly.GetName().Name + "/" + (string.IsNullOrEmpty(t.Namespace)? "" : t.Namespace + "/") + t.Name;
					menu.AddItem(new GUIContent("More/" + friendlyName), false, Selected, t);
				}

				menu.ShowAsContext();
			}
		}

		void Selected(object t){
			type = (System.Type)t;
		}

		#endif

	}
}
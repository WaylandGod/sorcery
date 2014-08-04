using UnityEngine;
using System.Reflection;
using System.Collections;

namespace NodeCanvas.Variables{
	
	///Variables are mostly stored in Blackboard. Derived classes of this store the correct type respectively depending on the class
	abstract public class VariableData : MonoBehaviour{

		public string dataName;

		private System.Type _type;
		private FieldInfo _valueField;

		///The Type this Variable holds
		virtual public System.Type varType{
			get
			{
				if (_type == null)
					_type = GetType().GetField("value").FieldType;
				return _type;
			}
		}

		///The System.Object value of the Variable object
		virtual public object objectValue{
			get
			{
				if (_valueField == null)
					_valueField = GetType().GetField("value");
				return _valueField.GetValue(this);
			}
			set
			{
				if (_valueField == null)
					_valueField = GetType().GetField("value");
				_valueField.SetValue(this, value);
			}
		}

		//Used when saving to get the object information
		virtual public object GetSerialized(){
			return objectValue;
		}

		//Used when loading to set the object information
		virtual public void SetSerialized(object obj){
			objectValue = obj;
		}

		public override string ToString(){
			return objectValue != null? objectValue.ToString() : "NULL";
		}

		//////////////////////////
		///////EDITOR/////////////
		//////////////////////////
		#if UNITY_EDITOR

		virtual public void ShowDataGUI(){

			var field = this.GetType().GetField("value");
			if (field == null){
				GUILayout.Label("No public field named 'value' found");
				return;
			}

			if (varType == typeof(object)){
				GUILayout.Label("(System.Object)", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
				return;
			}

			if (typeof(Object).IsAssignableFrom(varType)){

				field.SetValue(this, (Object)UnityEditor.EditorGUILayout.ObjectField( (Object)field.GetValue(this), varType, true, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true) ));

			} else if (!varType.IsValueType){

				var isList = typeof(IList).IsAssignableFrom(varType) && field.GetValue(this) != null;
				if (GUILayout.Button("(" + EditorUtils.TypeName(varType) + ")" + (isList? (field.GetValue(this) as IList).Count.ToString() : ""), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true)))
					NodeCanvasEditor.PopObjectEditor.Show(field.GetValue(this), varType);
			
			} else {

				GUILayout.Label("Can't show");
			}
		}

		#endif
	}
}
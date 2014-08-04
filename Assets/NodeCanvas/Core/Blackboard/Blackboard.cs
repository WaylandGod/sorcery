#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;
using System;
using NodeCanvas.Variables;

namespace NodeCanvas{

	[ExecuteInEditMode]
	[AddComponentMenu("NodeCanvas/Blackboard")]
	///Blackboard holds data and is able to save and load itself, but if so the name must be unique. It's usefull for interop
	///communication within the program
	public class Blackboard : MonoBehaviour{

		[SerializeField]
		private string _blackboardName = String.Empty;
		[SerializeField]
		private bool _doLoadSave;
		[SerializeField]
		private List<VariableData> variables = new List<VariableData>();
		[SerializeField]
		private bool _isGlobal;

		private static Dictionary<string, Blackboard> _allBlackboards;

		public bool isGlobal{
			get {return _isGlobal;}
			set {_isGlobal = value;}
		}

		public static Dictionary<string, Blackboard> allBlackboards{
			get
			{
				if (_allBlackboards == null){
					_allBlackboards = new Dictionary<string, Blackboard>();
					foreach (Blackboard bb in FindObjectsOfType<Blackboard>())
						_allBlackboards[bb.blackboardName] = bb;
				}
				return _allBlackboards;
			}
			set {_allBlackboards = value;}
		}

		public string blackboardName{
			get {return _blackboardName;}
			set
			{
				if (string.IsNullOrEmpty(value))
					value = gameObject.name + "_BB";

				if (_blackboardName != value){
					allBlackboards.Remove(_blackboardName);
					_blackboardName = value;
					allBlackboards[_blackboardName] = this;
				}
			}
		}

		public bool doLoadSave{
			get {return _doLoadSave;}
			set {_doLoadSave = value;}
		}


		///Get all data of the blackboard
		public List<VariableData> GetAllData(){
			return new List<VariableData>(variables);
		}

		private VariableData AddData(string name, object value){
			
			if (value == null)
				return null;
			
			var newData = AddData(name, value.GetType());
			if (newData != null)
				newData.objectValue = value;

			return newData;
		}

		private VariableData AddData(string name, Type type){

			VariableData newData = null;

			var typePairs = GetTypeDataRelation();
			foreach (KeyValuePair<Type, Type> pair in typePairs){
				if ( pair.Value == type || (pair.Value.IsAssignableFrom(type) && !typePairs.Values.ToList().Contains(type)) ){
					newData = (VariableData)gameObject.AddComponent(pair.Key);
					break;
				}
			}

			if (newData == null)
				newData = gameObject.AddComponent<SystemObjectData>();

			newData.dataName = name;
			variables.Add(newData);
			newData.hideFlags = HideFlags.HideInInspector;
			return newData;
		}

		//Get the Type to Type relation of VariableData type and their contained value type.
		private Dictionary<Type, Type> GetTypeDataRelation(){

			var pairs = new Dictionary<Type, Type>();
			foreach (System.Reflection.Assembly ass in System.AppDomain.CurrentDomain.GetAssemblies()){
				foreach (Type t in ass.GetTypes()){
					if (typeof(VariableData).IsAssignableFrom(t) && !t.IsAbstract && t.GetCustomAttributes(typeof(ObsoleteAttribute), true).FirstOrDefault() == null ){
						var valueField = t.GetField("value");
						if (valueField != null)
							pairs[t] = valueField.FieldType;
					}
				}
			}
			return pairs;
		}

		///Set the value of the VariableData variable defined by its name. If a data by that name and type doesnt exist, a new data is added by that name
		public VariableData SetDataValue(string name, object value){

			if (string.IsNullOrEmpty(name))
				return null;

			VariableData data = GetData(name, value != null? value.GetType() : typeof(object) );

			if (data != null){

				data.objectValue = value;
			
			} else if (value != null){

				Debug.Log("No VariableData of name '" + name + "' and type '" + value.GetType().Name + "' exists. Adding new instead...");
				return AddData(name, value);
			}

			return data;			
		}

		///Generic way of getting data. Reccomended
		public T GetDataValue<T>(string name){
			var data = GetData(name, typeof(T));
			if (data == null || data.objectValue == null || data.objectValue.Equals(null))
				return default(T);
			
			return (T)data.objectValue;
		}

		///Non generic method of geting a variable value
		public object GetDataValue(string name, Type type){
			VariableData data= GetData(name, type);
			return data != null? data.objectValue : null;
		}

		///Does the blackboard has the data of type and name?
		public bool HasData<T>(string dataName){
			return GetData(dataName, typeof(T)) != null;
		}

		///Deletes the VariableData of name provided
		public void DeleteData(string dataName){

			VariableData data= GetData(dataName, typeof(object));

			if (data != null){

				variables.Remove(data);
				DestroyImmediate(data,true);
			}
		}

		///Get the VariableData object of a certain value type itself.
		public VariableData GetData(string dataName, Type ofType){

			foreach (VariableData data in variables){

				if (data.dataName == dataName){
				
					if (ofType == null)
						return data;

					if (data.varType.IsAssignableFrom(ofType) || data.varType == ofType || ofType.IsAssignableFrom(data.varType))
						return data;
				}
			}

			return null;
		}


		///Get all data names of the blackboard
		public string[] GetDataNames(){

			string[] foundNames= new String[variables.Count];

			for (int i = 0; i < variables.Count; i++)
				foundNames[i] = variables[i].dataName;

			return foundNames;
		}

		///Get all data names of the blackboard and of specified type
		public string[] GetDataNames(System.Type ofType){

			List<string> foundNames = new List<string>();
			foreach (VariableData data in variables){
				if (ofType.IsAssignableFrom(data.varType))
					foundNames.Add(data.dataName);
			}

			return foundNames.ToArray();
		}

		///Gets a Blackboard by its name
		public static Blackboard Find(string bbName){

			if (allBlackboards.ContainsKey(bbName))
				return allBlackboards[bbName];
			return null;
		}

		void Awake(){

			allBlackboards[blackboardName] = this;
			if (doLoadSave && Application.isPlaying)
				Load();
		}

		void OnApplicationQuit(){
			if (doLoadSave && Application.isPlaying)
				Save();
		}

		void OnDestroy(){

			allBlackboards.Remove(blackboardName);

			#if !UNITY_EDITOR
			foreach(VariableData data in variables)
				DestroyImmediate(data);
			#endif

			#if UNITY_EDITOR //avoids destroy multiple times warning
			foreach(VariableData data in variables)
				UnityEditor.EditorApplication.delayCall += ()=> {if (data) DestroyImmediate(data, true); };
			#endif
		}


		////////////////////
		//SAVING & LOADING//
		////////////////////
		
		///Serrialize the blackboard's data as an 64String in PlayerPrefs. The name of the blackboard is important
		///The final string format that the blackboard is saved as, is returned.
		public string Save(){

			if (!Application.isPlaying){

				Debug.Log("You can only Save a blackboard in runtime for safety...");
				return null;
			}

			string stringFormat= "Blackboard-" + blackboardName;

			var formatter = new BinaryFormatter();
			var stream = new MemoryStream();
			var dataList = new List<SerializedData>();
			
			foreach (VariableData data in variables){
				var serValue = data.GetSerialized();
				if (serValue == null || (serValue != null && serValue.GetType().IsSerializable) ) {
					dataList.Add(new SerializedData(data.dataName, data.GetType(), serValue ) );
				} else {
					dataList.Add(new SerializedData(data.dataName, data.GetType(), null ) );
					Debug.LogWarning("Blackbaord '" + data.varType + "' data doesn't support save/load. '" + data.dataName + "'");
				}
			}

			formatter.Serialize(stream, dataList);
			PlayerPrefs.SetString(stringFormat, Convert.ToBase64String(stream.GetBuffer()));

			Debug.Log("Saved: " + stringFormat, gameObject);
			return stringFormat;
		}

		///Deserialize and load back all data. The name of the blackboard is used as a string format. Returns false if no saves were found.
		public bool Load(){

			if (!Application.isPlaying){
				Debug.Log("You can only Load a blackboard in runtime for safety...");
				return false;
			}

			string stringFormat= "Blackboard-" + blackboardName;
			var dataString = PlayerPrefs.GetString(stringFormat);

			if (dataString == String.Empty){
				Debug.Log("No Save found for: " + stringFormat);
				return false;
			}

			foreach (VariableData data in variables)
				DestroyImmediate(data, true);

			variables.Clear();

			var formatter = new BinaryFormatter();
			var stream = new MemoryStream(Convert.FromBase64String(dataString));		
			var loadedData = formatter.Deserialize(stream) as List<SerializedData>;

			foreach (SerializedData serializedData in loadedData){
				VariableData newData = gameObject.AddComponent(serializedData.type) as VariableData;
				newData.hideFlags = HideFlags.HideInInspector;
				newData.dataName = serializedData.name;
				newData.SetSerialized(serializedData.value);
				variables.Add(newData);
			}				

			Debug.Log("Loaded: " + stringFormat, gameObject);
			return true;
		}

		//The class that is actually serialized and deserialized by Save and Load
		[Serializable]
		private class SerializedData{

			public string name;
			public Type type;
			public object value;

			public SerializedData(string name, Type type, object value){

				this.name = name;
				this.type = type;
				this.value = value;
			}
		}

		//////////////////////////////////
		///////GUI & EDITOR STUFF/////////
		//////////////////////////////////
		#if UNITY_EDITOR


		[ContextMenu("Reset")]
		void Reset(){

		}

		[ContextMenu("Copy Component")]
		void CopyComponent(){
			Debug.Log("Unsupported");
		}

		[ContextMenu("Paste Component Values")]
		void PasteComponentValues(){
			Debug.Log("Unsupported");
		}

		void OnValidate(){
			for (int i = 0; i < variables.Count; i++){
				if ( (variables[i] as UnityEngine.Object) == null)
					variables.RemoveAt(i);
			}
		}

		public void ShowBlackboardGUI(){

			Undo.RecordObject(this, "Blackboard Inspector");

			GUILayout.BeginHorizontal();
			blackboardName = EditorGUILayout.TextField("Blackboard Name", blackboardName, new GUIStyle("textfield"), GUILayout.ExpandWidth(true));
			GUILayout.Label("Global", GUILayout.Width(40));
			isGlobal = EditorGUILayout.Toggle(isGlobal, EditorStyles.radioButton, GUILayout.Width(20));
			GUILayout.EndHorizontal();

			ShowVariablesGUI();
			ShowSaveLoadGUI();

			if (GUI.changed)
		        EditorUtility.SetDirty(this);
		}

		public void ShowVariablesGUI(){

			GUI.backgroundColor = new Color(0.8f,0.8f,1);
			if (GUILayout.Button("Add Variable")){

				GenericMenu.MenuFunction2 Selected = delegate(object selectedType){
					Undo.RecordObject(this, "New Variable");
					var newData = AddData("my" + EditorUtils.TypeName((Type)selectedType), (Type)selectedType);
					Undo.RegisterCreatedObjectUndo(newData, "New Variable");
				};

				var menu = new GenericMenu();
				foreach (KeyValuePair<Type, Type> pair in GetTypeDataRelation()){
					menu.AddItem(new GUIContent( (!string.IsNullOrEmpty(pair.Value.Namespace)? pair.Value.Namespace + "/": "") + EditorUtils.TypeName(pair.Value)), false, Selected, pair.Value);
					menu.ShowAsContext();
					Event.current.Use();
				}
			}

			GUI.backgroundColor = Color.white;
			if (variables.Count != 0){
				GUILayout.BeginHorizontal();
				GUI.color = Color.yellow;
				GUILayout.Label("Name", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
				GUILayout.Label("Value", GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
				GUI.color = Color.white;
				GUILayout.EndHorizontal();
			} else {
				EditorGUILayout.HelpBox("There are no variables in the Blackboard. Add some with the button above", MessageType.Info);
			}

			foreach ( VariableData data in variables.ToArray()){

				if (data != null){

					GUILayout.BeginHorizontal();

					if (!Application.isPlaying){

						GUI.backgroundColor = new Color(0.7f,0.7f,0.7f, 0.3f);
						data.dataName = EditorGUILayout.TextField(data.dataName, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));

					} else {

						GUI.backgroundColor = new Color(0.7f,0.7f,0.7f);
						GUI.color = new Color(0.8f,0.8f,1f);
						GUILayout.Label(data.dataName, GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true));
					}
					
					GUI.color = Color.white;
					GUI.backgroundColor = Color.white;

					Undo.RecordObject(data, "Data Change");
					data.ShowDataGUI();

					if (GUI.changed)
				        EditorUtility.SetDirty(data);

					GUI.color = Color.white;
					GUI.backgroundColor = Color.white;
					if (GUILayout.Button("X", GUILayout.Width(20), GUILayout.Height(16))){
						
						if (EditorUtility.DisplayDialog("Delete Data '" + data.dataName + "'", "Are you sure?", "Yes", "No!")){
							Undo.DestroyObjectImmediate(data);
							Undo.RecordObject(this, "Delete Data");
							variables.Remove(data);
						}
					}

					GUI.backgroundColor = new Color(0.7f,0.7f,0.7f);
					GUILayout.EndHorizontal();
				}
			}

			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;

			if (GUI.changed)
		        EditorUtility.SetDirty(this);
		}

		private void ShowSaveLoadGUI(){

			GUI.backgroundColor = new Color(1,1,1,0.5f);
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("Load OnAwake | Save OnQuit");
			doLoadSave = EditorGUILayout.Toggle(doLoadSave);
			GUI.color = new Color(1f,1f,1f,0.5f);
			if (GUILayout.Button("Delete Saves")){
				if (!PlayerPrefs.HasKey("Blackboard-" + blackboardName))
					Debug.Log("No saved Blackboard with name '" + blackboardName + "' found to delete...", gameObject);
				else
					Debug.Log("Blackboard '" + blackboardName + "' saves deleted...");
				PlayerPrefs.DeleteKey("Blackboard-" + blackboardName);
			}
			GUILayout.EndHorizontal();
			
			GUI.backgroundColor = Color.white;
			GUI.color = Color.white;
		}

		#endif
	}
}
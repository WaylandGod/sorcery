using UnityEngine;

namespace NodeCanvas.Variables{

	[AddComponentMenu("")]
	public class SystemObjectData : VariableData{

		public object value;

		public override System.Type varType{
			get {return value != null? value.GetType() : typeof(object);}
		}

		public override object objectValue{
			get {return value;}
			set {this.value = value;}
		}
	}
}
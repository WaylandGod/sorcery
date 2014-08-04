using UnityEngine;
using System.Collections.Generic;

namespace NodeCanvas.Variables{

	public class UnityObjectListData : VariableData {

		public List<Object> value = new List<Object>();
		public override object GetSerialized(){
			return null;
		}
	}
}
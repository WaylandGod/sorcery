using UnityEngine;
using NodeCanvas.Variables;

namespace NodeCanvas.Conditions{

	[Category("✫ Blackboard")]
	[Description("Check if a game object is contained in the target list")]
	public class CheckGameObjectList : ConditionTask {

		[RequiredField]
		public BBGameObjectList targetList = new BBGameObjectList{blackboardOnly = true};
		[RequiredField]
		public BBGameObject ckeckGameObject;

		protected override string info{
			get {return targetList + " contains " + ckeckGameObject;}
		}

		protected override bool OnCheck(){

			return targetList.value.Contains(ckeckGameObject.value);
		}
	}
}
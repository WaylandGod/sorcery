using UnityEngine;
using System.Collections;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	[Category("ITween")]
	public class ITweenStop : ActionTask {

		[RequiredField]
		public BBString id;

		protected override void OnExecute(){
			iTween.StopByName(id.value);
			EndAction();
		}
	}
}
using UnityEngine;
using NodeCanvas.Variables;

namespace NodeCanvas.Actions{

	[Category("✫ Utility")]
	[Description("Display a UI label on the agent's position if seconds to run is not 0, else simply logs the message")]
	[AgentType(typeof(Transform))]
	public class DebugAction : ActionTask{

		public string log;
		public float YOffset;
		public float secondsToRun;
		public bool actionReturn = true;

		private Texture2D _tex;
		private Texture2D tex{
			get
			{
				if (!_tex){
					_tex = new Texture2D(1,1);
					_tex.SetPixel(0, 0, Color.white);
					_tex.Apply();
				}
				return _tex;			
			}
		}

		protected override string info{
			get {return (secondsToRun > 0? "UI Log '" : "Log '") + log + "'" + (secondsToRun > 0? " for " + secondsToRun + " sec." : ""); }
		}

		protected override void OnExecute(){

			if (secondsToRun <= 0){
				Debug.Log(log);
				EndAction(actionReturn);
			}

			useGUILayout = !string.IsNullOrEmpty(log);
		}

		protected override void OnUpdate(){

			if (elapsedTime >= secondsToRun){
				EndAction(actionReturn);
			}
		}

		void OnGUI(){
			
			if (Camera.main == null || string.IsNullOrEmpty(log) || agent == null)
				return;

			var point = Camera.main.WorldToScreenPoint(agent.transform.position + new Vector3(0, YOffset, 0));
			var size = new GUIStyle("label").CalcSize(new GUIContent(log));
			var r = new Rect(point.x - size.x /2, Screen.height - point.y/2, size.x +10, size.y);
			GUI.color = new Color(1f,1f,1f,0.5f);
			GUI.DrawTexture(r, tex);
			GUI.color = new Color(0.2f, 0.2f, 0.2f, 1);
			r.x += 4;
			GUI.Label(r, log);
			GUI.color = Color.white;
		}
	}
}
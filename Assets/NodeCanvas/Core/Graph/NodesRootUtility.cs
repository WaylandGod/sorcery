using UnityEngine;

namespace NodeCanvas{
	
	//Utility class for the nodes root game object
	//Future case use
	[AddComponentMenu("")]
	public class NodesRootUtility : MonoBehaviour {

		[SerializeField]
		Graph _parentGraph;

		public Graph parentGraph{
			get {return _parentGraph;}
			set {_parentGraph = value;}
		}
	}
}
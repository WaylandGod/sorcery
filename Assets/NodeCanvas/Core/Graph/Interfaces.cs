using UnityEngine;

namespace NodeCanvas{

	///An interface used to provide default agent and blackboard references to tasks
	public interface ITaskSystem{

		Component agent {get;}
		Blackboard blackboard {get;}

		void SendTaskOwnerDefaults();
		void SendEvent(string eventName);
	}

	///Denotes that the node type is auto sortable based on position on the canvas
	public interface IAutoSortable{}

	///Denotes that the class is taggable
	public interface ITaggable{}

	///Denotes that the node holds a nested graph
	public interface INestedNode{
		Graph nestedGraph {get;set;}
	}

	///Denotes that the node can be assigned a Task and it's workings are based on that task, either condition or action.
	public interface ITaskAssignable{
		Task task{get;set;}
	}
}
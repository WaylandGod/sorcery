using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Collections.Generic;

namespace NodeCanvas{

	///Handles subscribers and dispatches messages. Works with MonoBehaviour subscribers and method subscribers as well.
	///If you want to debug events send and subscribers, add the EventDebugger component somewhere
	public static class EventHandler{

		public static bool logEvents;
		public static Dictionary<string, List<SubscribedMember>> subscribedMembers = new Dictionary<string, List<SubscribedMember>>();

		public static void Subscribe(MonoBehaviour mono, Enum eventEnum, int invokePriority = 0, bool unsubscribeWhenReceive = false){
			Subscribe(mono, eventEnum.ToString(), invokePriority, unsubscribeWhenReceive);
		}

		///Subscribes a MonoBehaviour to an Event along with options. When the Event is dispatched a funtion
		///with the same name as the Event will be called on the subscribed MonoBehaviour. Events are provided by an Enum or string
		public static void Subscribe(MonoBehaviour mono, string eventName, int invokePriority = 0, bool unsubscribeWhenReceive = false){

			var method = mono.GetType().GetMethod(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
			if (method == null){
				Debug.LogError("EventHandler: No Method with name '" + eventName + "' exists on '" + mono.GetType().Name + "' Subscribed Type");
				return;
			}

			if (!subscribedMembers.ContainsKey(eventName))
				subscribedMembers[eventName] = new List<SubscribedMember>();

			foreach (SubscribedMember member in subscribedMembers[eventName]){
				if (member.subscribedMono == mono){
					Debug.LogWarning("Mono " + mono + " is allready subscribed to " + eventName);
					return;
				}
			}

			if (logEvents)
				Debug.Log("@@@ " + mono + " subscribed to " + eventName);
			
			subscribedMembers[eventName].Add(new SubscribedMember(mono, invokePriority, unsubscribeWhenReceive));
			subscribedMembers[eventName] = subscribedMembers[eventName].OrderBy(member => -member.invokePriority).ToList();
		}


		//Subscribe a function to an Event by enum name
		public static void SubscribeFunction(Action<object> func, Enum eventEnum){
			SubscribeFunction(func, eventEnum.ToString());
		}

		//Subscribe a function to an Event by string name
		public static void SubscribeFunction(Action<object> func, string eventName){

			if (!subscribedMembers.ContainsKey(eventName))
				subscribedMembers[eventName] = new List<SubscribedMember>();

			foreach (SubscribedMember member in subscribedMembers[eventName]) {
				
				if (member.subscribedFunction == func){
					
					if (logEvents)
						Debug.Log("Function allready subscribed to " + eventName);
					
					return;
				}
			}

			subscribedMembers[eventName].Add(new SubscribedMember(func, 0, false));
		}


		///Unsubscribe a MonoBehaviour member from all Events
		public static void Unsubscribe(MonoBehaviour mono){

			if (!mono)
				return;

			foreach (string eventName in subscribedMembers.Keys){
				foreach (SubscribedMember member in subscribedMembers[eventName].ToArray()){

					if (member.subscribedMono == mono){

						subscribedMembers[eventName].Remove(member);

						if (logEvents)
							Debug.Log("XXX " + mono + "Unsubscribed from everything!");
					}
				}
			}
		}

		public static void Unsubscribe(MonoBehaviour mono, Enum eventEnum){
			Unsubscribe(mono, eventEnum.ToString());
		}		

		///Unsubscribes a MonoBehaviour member from an Event
		public static void Unsubscribe(MonoBehaviour mono, string eventName){

			if (!mono || !subscribedMembers.ContainsKey(eventName))
				return;

			foreach (SubscribedMember member in subscribedMembers[eventName].ToArray()){

				if (member.subscribedMono == mono){

					subscribedMembers[eventName].Remove(member);

					if (logEvents)
						Debug.Log("XXX Member " + mono + " Unsubscribed from " + eventName);

					return;
				}
			}

			if (logEvents)
				Debug.Log("You tried to Unsubscribe " + mono + " from " + eventName + ", but it was never subscribed there!");
		}

		//Unsubscribes a Function member from everything
		public static void UnsubscribeFunction(Action<object> func){

			if (func == null)
				return;

			foreach (string eventName in subscribedMembers.Keys){
				foreach (SubscribedMember member in subscribedMembers[eventName].ToArray()){
					if (member.subscribedFunction != null && member.subscribedFunction.ToString() == func.ToString())
						subscribedMembers[eventName].Remove(member);
				}
			}

			if (logEvents)
				Debug.Log("XXX " + func.ToString() + " Unsubscribed from everything");
		}

		public static bool Dispatch(Enum eventEnum, object arg = null){
			return Dispatch(eventEnum.ToString(), arg);
		}

		///Dispatches a new Event. On any subscribers listening, a function of the same name as the Event will be called. An Object may be passed as an argument.
		public static bool Dispatch(string eventName, object arg = null){

			if (logEvents)
				Debug.Log(">>> Event " + eventName + " Dispatched. (" + arg.GetType() + ") Argument");

			if (!subscribedMembers.ContainsKey(eventName)){
				Debug.LogWarning("EventHandler: Event '" + eventName + "' was not received by anyone!");
				return false;
			}

			foreach (SubscribedMember member in subscribedMembers[eventName].ToArray()){

				var mono = member.subscribedMono;

				//clean up by-product
				if (mono == null && member.subscribedFunction == null){
					subscribedMembers[eventName].Remove(member);
					continue;
				}

				if (logEvents)
					Debug.Log("<<< Event " + eventName + " Received by " + mono);

				if (member.unsubscribeWhenReceive)
					Unsubscribe(mono, eventName);

				if (member.subscribedFunction != null){
					member.subscribedFunction(arg);
					continue;
				}
				
				var method = mono.GetType().GetMethod(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
				if (method == null){
					Debug.LogWarning("Method '" + eventName + "' not found on subscribed MonoBehaviour '" + mono.name + "'");
					continue;
				}

				var parameters = method.GetParameters();
				if (parameters.Length > 1){
					Debug.LogError("Subscribed function to call '" + method.Name + "' has more than one parameter on " + mono + ". It should only have one.", mono.gameObject);
					continue;
				}

				var args = arg != null && parameters.Length == 1 && parameters[0].ParameterType.IsAssignableFrom(arg.GetType()) ? new object[]{arg} : null;
				if (method.ReturnType == typeof(IEnumerator)){
					mono.enabled = true;
					mono.gameObject.SetActive(true);
					MonoManager.current.StartCoroutine( (IEnumerator)method.Invoke(mono, args) );
				} else {
					method.Invoke(mono, args);
				}
			}

			return true;
		}

		///Describes a member to be handled by the EventHandler.
		public class SubscribedMember{

			public MonoBehaviour subscribedMono;
			public Action<object> subscribedFunction;
			public int invokePriority = 0;
			public bool unsubscribeWhenReceive;

			public SubscribedMember(MonoBehaviour mono, int invokePriority, bool unsubscribeWhenReceive){

				this.subscribedMono = mono;
				this.invokePriority = invokePriority;
				this.unsubscribeWhenReceive = unsubscribeWhenReceive;
			}

			public SubscribedMember(Action<object> func, int invokePriority, bool unsubscribeWhenReceive){

				this.subscribedFunction = func;
				this.invokePriority = invokePriority;
				this.unsubscribeWhenReceive = unsubscribeWhenReceive;
			}
		}

	}
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SDGraph;
using GraphProcessor;
using UnityEngine;

namespace NodeGraphProcessor.Examples
{
	//[Serializable, NodeMenuItem("Control Flow/Wait")]
	public class WaitNode : WaitableNode
	{
		public override string name => "Wait";

		[SerializeField, Input(name = "Seconds")]
		public float waitTime = 1f;

		private static WaitMonoBehaviour waitMonoBehaviour;

		protected override void Enable()
		{
			base.Enable();
		}

	


		protected override IEnumerator Execute()
		{
			if(waitMonoBehaviour == null)
			{
				var go = new GameObject(name: "WaitGameObject");
				waitMonoBehaviour = go.AddComponent<WaitMonoBehaviour>();
			}

			waitMonoBehaviour.Process(waitTime, ProcessFinished);
			yield return null;
		}

		public override void Process()
		{
			//	We should check where this Process() called from. But i don't know if this is an elegant and performant way to do that.
			//	If this function is called from other than the ConditionalNode, then there will be problems, errors, unforeseen consequences, tears.
			// var isCalledFromConditionalProcessor = new StackTrace().GetFrame(5).GetMethod().ReflectedType == typeof(ConditionalProcessor);
			// if(!isCalledFromConditionalProcessor) return;
			
			if(waitMonoBehaviour == null)
			{
				var go = new GameObject(name: "WaitGameObject");
				waitMonoBehaviour = go.AddComponent<WaitMonoBehaviour>();
			}

			waitMonoBehaviour.Process(waitTime, ProcessFinished);
		}	
	}

	public class WaitMonoBehaviour : MonoBehaviour
	{
		public void Process(float time, Action callback)
		{
			Debug.Log("test");
			StartCoroutine(_Process(time, callback));
		}

		private IEnumerator _Process(float time, Action callback)
		{
			yield return new WaitForSeconds(time);
			callback.Invoke();
		}
	}
}
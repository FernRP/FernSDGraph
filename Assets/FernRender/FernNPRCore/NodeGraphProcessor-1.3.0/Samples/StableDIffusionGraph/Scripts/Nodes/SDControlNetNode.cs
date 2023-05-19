using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Text;
using FernNPRCore.StableDiffusionGraph;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEngine.Networking;

namespace FernNPRCore.SDNodeGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD ControlNet")]
	public class SDControlNetNode : LinearSDProcessorNode
	{
		[Input("Image")] public Texture controlNetImg;
		[Input("Weight"), ShowAsDrawer] public float weight = 1;
		[Input("Resize Mode"), ShowAsDrawer] public ResizeMode resize_mode = ResizeMode.ScaleToFit_InnerFit;
		[Input("Low Vrm"), ShowAsDrawer] public bool lowvram = false;
		[Input("Processor Res"), ShowAsDrawer] public int processor_res = 64;
		[Input("Threshold a"), ShowAsDrawer] public int threshold_a = 64;
		[Input("Threshold b"), ShowAsDrawer] public int threshold_b = 64;
		[Input("Guidance Start"), ShowAsDrawer] public float guidance_start = 0.0f;
		[Input("Guidance End"), ShowAsDrawer] public float guidance_end = 1.0f;
		[Input("Guidance"), ShowAsDrawer] public float guidance = 1f;
		[Input("Control Mode"), ShowAsDrawer] public ControlMode control_mode = ControlMode.Balanced;
		[Output("ControlNet"), SerializeField] public ControlNetData controlNet;
		
		[HideInInspector]
		public string module = "none";
		[HideInInspector]
		public string model = "none";
		[HideInInspector]
		public List<string> modelList = new List<string>();
		[HideInInspector]
		public int currentModelListIndex = 0;
		[HideInInspector]
		public List<string> moudleList = new List<string>();
		[HideInInspector]
		public int currentMoudleListIndex = 0;

		public override string		name => "SD ControlNet";

		protected override void Enable()
		{
			base.Enable();
			hasPreview = false;
			controlNet = new ControlNetData();
			EditorCoroutineUtility.StartCoroutine(ControlNetModelListAsync(), this);
			EditorCoroutineUtility.StartCoroutine(ControlNetMoudleList(), this);
		}
		
		/// <summary>
		/// Get the list of available Stable Diffusion models.
		/// </summary>
		/// <returns></returns>
		IEnumerator ControlNetModelListAsync()
		{
			// Stable diffusion API url for getting the models list
			string url = SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.ControlNetModelList;

			UnityWebRequest request = new UnityWebRequest(url, "GET");
			request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");

			
			if (SDGraphResource.SdGraphDataHandle.GetUseAuth() && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
			{
				SDUtil.Log("Using API key to authenticate");
				byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() + ":" + SDGraphResource.SdGraphDataHandle.GetPassword());
				string encodedCredentials = Convert.ToBase64String(bytesToEncode);
				request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
			}

			yield return request.SendWebRequest();

			try
			{
				// Deserialize the response to a class
				ControlNetModel ms = JsonConvert.DeserializeObject<ControlNetModel>(request.downloadHandler.text);
				modelList.Clear();

				foreach (var m in ms.model_list)
				{
					modelList.Add(m);
				}

				//model = modelList[0];
			}
			catch (Exception)
			{
				SDUtil.Log("Server needs and API key authentication. Please check your settings!");
			}
		}
		
		/// <summary>
		/// Get the list of available Stable Diffusion models.
		/// </summary>
		/// <returns></returns>
		IEnumerator ControlNetMoudleList()
		{
			// Stable diffusion API url for getting the models list
			string url = SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.ControlNetMoudleList;

			UnityWebRequest request = new UnityWebRequest(url, "GET");
			request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
			request.SetRequestHeader("Content-Type", "application/json");

			if (SDGraphResource.SdGraphDataHandle.GetUseAuth() && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
			{
				SDUtil.Log("Using API key to authenticate");
				byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() + ":" + SDGraphResource.SdGraphDataHandle.GetPassword());
				string encodedCredentials = Convert.ToBase64String(bytesToEncode);
				request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
			}

			yield return request.SendWebRequest();

			try
			{
				// Deserialize the response to a class
				ControlNetMoudle ms = JsonConvert.DeserializeObject<ControlNetMoudle>(request.downloadHandler.text);
				moudleList.Clear();

				foreach (var m in ms.module_list)
				{
					moudleList.Add(m);
				}

				module = moudleList[0];
			}
			catch (Exception)
			{
				SDUtil.Log("Server needs and API key authentication. Please check your settings!");
			}
		}
		
		/// <summary>
		/// Get the list of available Stable Diffusion models.
		/// </summary>
		/// <returns></returns>
		IEnumerator ControlNetDetect()
		{
			if (model.Equals("none"))
			{
				yield return ControlNetModelListAsync();
			}
			if (module.Equals("none"))
			{
				yield return ControlNetMoudleList();
			}
            
			controlNet.module = module;
			controlNet.model = model;
			controlNet.weight = weight;
			controlNet.resize_mode = (int)resize_mode;
			controlNet.lowvram = lowvram;
			controlNet.processor_res = processor_res;
			controlNet.threshold_a = threshold_a;
			controlNet.threshold_b = threshold_b;
			controlNet.guidance_start = guidance_start;
			controlNet.guidance_end = guidance_end;
			controlNet.guidance = guidance;
			controlNet.control_mode = (int)control_mode;
			if (controlNetImg != null)
			{
				byte[] inputImgBytes = SDUtil.TextureToTexture2D(controlNetImg).EncodeToPNG();
				string inputImgString = Convert.ToBase64String(inputImgBytes);
				controlNet.input_image = inputImgString;
			}
			yield return null;
		}

		protected override void Execute()
		{
			if (controlNet == null) controlNet = new ControlNetData();
			// if (modelList == null || modelList.Count <= 0)
			// {
			// 	EditorCoroutineUtility.StartCoroutine(ControlNetModelListAsync(), this);
			// }
			//
			// if (moudleList == null || moudleList.Count <= 0)
			// {
			// 	EditorCoroutineUtility.StartCoroutine(ControlNetMoudleList(), this);
			// }
			
			//GetPort(nameof(controlNet), null).PushData();

			EditorCoroutineUtility.StartCoroutine(ControlNetDetect(), this);
			GetPort(nameof(controlNet), null).PushData();
		}
	}
}


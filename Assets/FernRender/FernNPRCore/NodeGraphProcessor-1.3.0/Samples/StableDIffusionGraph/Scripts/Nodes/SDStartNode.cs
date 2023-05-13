using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;

namespace FernNPRCore.SDNodeGraph
{
	
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Start")]
	public class SDStartNode : WaitableNode
	{
		[Input("Model")] public string Model;

		[Output(name = "Out")]
		public float output;
		
		public bool overrideSettings = false;

		[VisibleIf(nameof(overrideSettings), true)]
		public string serverURL = "http://127.0.0.1:7860";
		[VisibleIf(nameof(overrideSettings), true)]
		public bool useAuth = false;
		[VisibleIf(nameof(useAuth), true)]
		public string user = "";
		[VisibleIf(nameof(useAuth), true)]
		public string pass = "";
		
		public override string	name => "SD Start";


		protected override void Process()
		{
			if (overrideSettings&&!string.IsNullOrEmpty(serverURL))
			{
				SDGraphDataHandle.Instance.OverrideSettings = true;
				SDGraphDataHandle.Instance.OverrideServerURL = serverURL;
				SDGraphDataHandle.Instance.OverrideUseAuth = useAuth;
				SDGraphDataHandle.Instance.OverrideUsername = user;
				SDGraphDataHandle.Instance.OverridePassword = pass;
			}
			else
			{
				SDGraphDataHandle.Instance.OverrideSettings = false;
			}
			SDUtil.Log($"Use {Model}");
			EditorCoroutineUtility.StartCoroutine(SetModelAsync(Model, ProcessFinished), this);
		}
		
		// <summary>
        /// Set a model to use by Stable Diffusion.
        /// </summary>
        /// <param name="modelName">Model to set</param>
        /// <returns></returns>
        public IEnumerator SetModelAsync(string modelName, Action callback)
        {
            // Stable diffusion API url for setting a model
            string url = SDGraphDataHandle.Instance.GetServerURL()+SDGraphDataHandle.Instance.OptionAPI;

            // Load the list of models if not filled already
            if (string.IsNullOrEmpty(Model))
            {
                SDUtil.Log("Model is null");
                yield return null;
            }

            try
            {
                // Tell Stable Diffusion to use the specified model using an HTTP POST request
                HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                if (SDGraphDataHandle.Instance.GetUseAuth() && !string.IsNullOrEmpty(SDGraphDataHandle.Instance.GetUserName()) && !string.IsNullOrEmpty(SDGraphDataHandle.Instance.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphDataHandle.Instance.GetUserName() + ":" + SDGraphDataHandle.Instance.GetPassword());
                    string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encodedCredentials);
                }

                // Write to the stream the JSON parameters to set a model
                {
                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        // Model to use
                        SDOption sd = new SDOption();
                        sd.sd_model_checkpoint = modelName;

                        // Serialize into a JSON string
                        string json = JsonConvert.SerializeObject(sd);

                        // Send the POST request to the server
                        streamWriter.Write(json);
                    }

                    // Get the response of the server
                    Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();
                    var httpResponse = webResponse.Result;
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        string result = streamReader.ReadToEnd();
                        SDUtil.Log(result);
                    }
                }
            }
            catch (WebException e)
            {
                SDUtil.Log("Error: " + e.Message);
            }
            callback?.Invoke();
        }
	}

}

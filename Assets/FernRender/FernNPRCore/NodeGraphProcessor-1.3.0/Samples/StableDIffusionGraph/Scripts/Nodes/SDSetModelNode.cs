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
using FernNPRCore.StableDiffusionGraph;
using Newtonsoft.Json;
using NodeGraphProcessor.Examples;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Events;

namespace FernNPRCore.SDNodeGraph
{
	
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Set Model")]
	public class SDSetModelNode : WaitableNode
	{
		[Input("Server URL")] public string ServerURL;

		[Output(name = "Out")]
		public float output;
		
        [HideInInspector]
        public string[] modelNames;
        
        [HideInInspector]
        public int currentIndex = 0;

        public string Model;
        
		public override string	name => "SD Set Model";


		protected override void Process()
		{
			EditorCoroutineUtility.StartCoroutine(SetModelAsync(ServerURL, ProcessFinished), this);
		}
		
		
		public void GetModelList()
		{
            Debug.Log(ServerURL);
			EditorCoroutineUtility.StartCoroutine(ListModelsAsync(), this);
		}
		
		/// <summary>
        /// Get the list of available Stable Diffusion models.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListModelsAsync(UnityAction unityAction = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                string serverUrl = "http://127.0.0.1:7860";
                serverUrl = string.IsNullOrEmpty(ServerURL) ?  SDDataHandle.Instance.GetServerURL() : ServerURL;
                string url = serverUrl + SDDataHandle.Instance.ModelsAPI;

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                NetAuthorizationUtil.SetRequestAuthorization(httpWebRequest);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n\n" + e.StackTrace);
            }
            if (httpWebRequest != null)
            {
                // Wait that the generation is complete before procedding
                Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();
                while (!webResponse.IsCompleted)
                {           
#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
                    yield return new WaitForSeconds(100);
                }
                // Stream the result from the server
                var httpResponse = webResponse.Result;
                

                try
                {
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {

                        // SDUtil.Log(request.downloadHandler.text);
                        // Decode the response as a JSON string
                        string result = streamReader.ReadToEnd();
                        // Deserialize the response to a class
                        SDModel[] ms = JsonConvert.DeserializeObject<SDModel[]>(result);

                        // Keep only the names of the models
                        List<string> modelsNames = new List<string>();

                        foreach (SDModel m in ms) 
                            modelsNames.Add(m.model_name);

                        // Convert the list into an array and store it for futur use
                        modelNames = modelsNames.ToArray();
                        SDUtil.Log(modelsNames.Count.ToString());
                    }
                }
                catch (Exception)
                {
                    SDUtil.Log("Server needs and API key authentication. Please check your settings!");
                }
            }
            unityAction?.Invoke();
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

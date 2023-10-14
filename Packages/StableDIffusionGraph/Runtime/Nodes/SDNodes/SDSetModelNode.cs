using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GraphProcessor;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine.Events;


namespace FernNPRCore.SDNodeGraph
{
	
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Set Model")]
	public class SDSetModelNode : LinearSDProcessorNode
	{
		[Input("Server URL")] public string ServerURL;
		
        [HideInInspector]
        public string[] modelNames;
        
        [HideInInspector]
        public int currentIndex = 0;

        [HideInInspector]
        public string Model;
        
		public override string	name => "SD Set Model";
        
		protected override IEnumerator Execute()
		{
			yield return SetModelAsync(Model, null);
		}
		
		// <summary>
        /// Set a model to use by Stable Diffusion.
        /// </summary>
        /// <param name="modelName">Model to set</param>
        /// <returns></returns>
        public IEnumerator SetModelAsync(string modelName, Action callback)
        {
            // Stable diffusion API url for setting a model
            string url = SDGraphResource.SdGraphDataHandle.GetServerURL()+SDGraphResource.SdGraphDataHandle.OptionAPI;

            // Load the list of models if not filled already
            if (string.IsNullOrEmpty(Model))
            {
                SDUtil.Log("Model is null");
                yield return null;
            }

            HttpWebRequest httpWebRequest = null;
            try
            {
                // Tell Stable Diffusion to use the specified model using an HTTP POST request
                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                if (SDGraphResource.SdGraphDataHandle.GetUseAuth() && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() + ":" + SDGraphResource.SdGraphDataHandle.GetPassword());
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
                }
            }
            catch (WebException e)
            {
                SDUtil.Log("Error: " + e.Message);
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
                        string result = streamReader.ReadToEnd();
                        SDUtil.Log("Set Model");
                    }
                }
                catch (WebException e)
                {
                    SDUtil.Log("Error: " + e.Message);
                }
            }

            
            callback?.Invoke();
        }
	}
}

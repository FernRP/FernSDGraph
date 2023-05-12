using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FernGraph;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDCheckPoint : Node
    {
        [Output] public string Model;

        public string[] modelNames;
        public int currentIndex = 0;

        public override void OnAddedToGraph()
        {
            base.OnAddedToGraph();
            EditorCoroutineUtility.StartCoroutine(ListModelsAsync(), this);
        }

        /// <summary>
        /// Get the list of available Stable Diffusion models.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListModelsAsync(UnityAction unityAction=null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                // Stable diffusion API url for getting the models list
                string url = SDDataHandle.Instance.GetServerURL() + SDDataHandle.Instance.ModelsAPI;

                httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.SetRequestAuthorization();
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
                        unityAction?.Invoke();
                    }
                }
                catch (Exception)
                {
                    SDUtil.Log("Server needs and API key authentication. Please check your settings!");
                }
            }
            
            
        }

        public override object OnRequestValue(Port port)
        {
            return Model;
        }
    }
}
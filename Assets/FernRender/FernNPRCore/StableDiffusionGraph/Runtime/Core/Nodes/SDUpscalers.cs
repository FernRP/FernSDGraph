using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FernGraph;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace FernNPRCore.StableDiffusionGraph
{    
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDHRUpscalers : Node
    {
        [Output] public HiresUpscaler upscaler;
        public List<string> modelNames;

        public SDHRUpscalers()
        {
            modelNames = new List<string>();
            upscaler = new HiresUpscaler();
        }

        public IEnumerator ListModelsAsync(Action action = null)
        {
            HttpWebRequest httpWebRequest = null;
            try
            {
                // Stable diffusion API url for getting the models list
                string url = SDDataHandle.Instance.GetServerURL() + SDDataHandle.Instance.UpscalersAPI;

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
                        SDUpscalerModel[] ms = JsonConvert.DeserializeObject<SDUpscalerModel[]>(result);

                        modelNames.Clear();
                        foreach (SDUpscalerModel m in ms) 
                            modelNames.Add(m.name);
                        action?.Invoke();
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
            return upscaler;
        }
    }
}
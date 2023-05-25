using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using FernNPRCore.StableDiffusionGraph;
using GraphProcessor;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{    
    [NodeCustomEditor(typeof(SDTxt2ImgUpscalers))]
    public class SDTxt2ImgUpscalersView : SDNodeView
    {
        public List<string> modelNames = new List<string>();
        public static string[] modelNames_default =
        {
            "Latent", 
            "Latent (antialiased)",
            "Latent (bicubic)",
            "Latent (bicubic antialiased)",
            "Latent (nearest)",
            "Latent (nearest-exact)",
        };
        public override void Enable()
        {
            base.Enable();
            extensionContainer.Clear();
            OnAsync();
            var button = new Button(OnAsync);
            button.text = "Refresh Model List";
            extensionContainer.Add(button);
            extensionContainer.Add(modelsContainer);
            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            RefreshExpandedState();
        }

        private void OnGUI()
        {
            if(nodeTarget is not SDTxt2ImgUpscalers upscalers) return;
            if (modelNames is not { Count: > 0 }) return;
            
            EditorGUILayout.BeginVertical();
            EditorGUI.BeginChangeCheck();
            
                    
            var index = modelNames.IndexOf(upscalers.upscaler.hr_upscaler);
            if (index == -1)
            {
                index = 0;
                upscalers.upscaler.hr_upscaler = modelNames[0];
            }

            EditorGUIUtility.labelWidth -= 80;
            index = EditorGUILayout.Popup("model", index, modelNames.ToArray());
            var steps = EditorGUILayout.IntSlider("steps", upscalers.upscaler.hr_second_pass_steps, 0, 150);
            var denoising = EditorGUILayout.Slider("denoising", upscalers.upscaler.denoising_strength, 0f, 4f);
            var scale = EditorGUILayout.Slider("scale", upscalers.upscaler.hr_scale, 1f, 4f);
            var resizeX = EditorGUILayout.IntSlider("resizeX", upscalers.upscaler.hr_resize_x, 0, 2048);
            var resizeY = EditorGUILayout.IntSlider("resizeY", upscalers.upscaler.hr_resize_y, 0, 2048);
            EditorGUIUtility.labelWidth += 80;
            if (EditorGUI.EndChangeCheck())
            {
                upscalers.upscaler.hr_upscaler = modelNames[index];
                upscalers.upscaler.hr_second_pass_steps = steps;
                upscalers.upscaler.denoising_strength = denoising;
                upscalers.upscaler.hr_scale = scale;
                upscalers.upscaler.hr_resize_x = resizeX;
                upscalers.upscaler.hr_resize_y = resizeY;
            }
            EditorGUILayout.EndVertical();
            

        }

        VisualElement modelsContainer = new VisualElement();
        private void OnAsync()
        {
            EditorCoroutineUtility.StartCoroutine(ListModelsAsync(), this);
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
                        SDUpscalerModel[] models = JsonConvert.DeserializeObject<SDUpscalerModel[]>(result);
                        if (models != null)
                        {
                            modelNames.Clear();
                            modelNames.AddRange(modelNames_default);
                            foreach (SDUpscalerModel model in models) 
                                modelNames.Add(model.name);
                            action?.Invoke();
                        }
                    }
                }
                catch (Exception)
                {
                    SDUtil.Log("Server needs and API key authentication. Please check your settings!");
                }
            }
        }
    }
}
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
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{    
    [NodeCustomEditor(typeof(UltimateSDUpscale))]
    public class UltimateSDUpscaleView : SDNodeView
    {
        public List<string> modelNames = new List<string>();
        public override void Enable()
        {
            var stylesheet = Resources.Load<StyleSheet>("SDGraphCommon");
            if (styleSheets.Contains(stylesheet)) 
                styleSheets.Remove(stylesheet);
            // Setup a container to render IMGUI content in 
            var container = new IMGUIContainer(OnGUI);
            extensionContainer.Add(container);
            RefreshExpandedState();
        }

        private bool fold_0;
        private bool fold_1;
        private bool fold_2;

        public static readonly GUIContent _refresh = new GUIContent(EditorGUIUtility.IconContent("d_Refresh").image, "refresh");
        private void OnGUI()
        {
            if(nodeTarget is not UltimateSDUpscale sdUpscale) return;
            fold_0 = EditorGUILayout.Foldout(fold_0, "Will upscale the image depending on the selected target size type", true);
            if (fold_0)
            {
                EditorGUI.BeginChangeCheck();
                var target_size_type = sdUpscale.target_size_type;
                var custom_width = sdUpscale.custom_width;
                var custom_height = sdUpscale.custom_height;
                var custom_scale = sdUpscale.custom_scale;
                target_size_type = EditorGUILayout.Popup("Target size type", target_size_type, UltimateSDUpscale.target_size_types);
                if (target_size_type == 1)
                {
                    custom_width = EditorGUILayout.IntSlider("Custom width", custom_width, 64, 8192);
                    custom_height = EditorGUILayout.IntSlider("Custom height", custom_height, 64, 8192);
                }
                if (target_size_type == 2)
                {
                    custom_scale = EditorGUILayout.Slider("Custom scale", custom_scale, 1, 16);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    sdUpscale.target_size_type = target_size_type;
                    sdUpscale.custom_width = custom_width / 64 * 64;
                    sdUpscale.custom_height = custom_height / 64 * 64;
                    sdUpscale.custom_scale = custom_scale;
                }
            }
            fold_1 = EditorGUILayout.Foldout(fold_1, "Redraw options", true);
            if (fold_1)
            {
                EditorGUI.BeginChangeCheck();
                var upscaler_index = sdUpscale.upscaler_index;
                EditorGUILayout.BeginHorizontal();
                upscaler_index = EditorGUILayout.Popup("Upscaler", upscaler_index, modelNames.ToArray());
                if (GUILayout.Button(_refresh, GUILayout.Width(30))) OnAsync();
                EditorGUILayout.EndHorizontal();
                var redraw_mode = sdUpscale.redraw_mode;
                var tile_width = sdUpscale.tile_width;
                var tile_height = sdUpscale.tile_height;
                var mask_blur = sdUpscale.mask_blur;
                var padding = sdUpscale.padding;
                redraw_mode = EditorGUILayout.Popup("Type", redraw_mode, UltimateSDUpscale.redrow_modes);
                tile_width = EditorGUILayout.IntSlider("Tile width", tile_width, 0, 2048);
                tile_height = EditorGUILayout.IntSlider("Tile height", tile_height, 0, 2048);
                mask_blur = EditorGUILayout.IntSlider("Mask blur", mask_blur, 0, 64);
                padding = EditorGUILayout.IntSlider("Padding", padding, 0, 128);

                if (EditorGUI.EndChangeCheck())
                {
                    sdUpscale.upscaler_index = upscaler_index;
                    sdUpscale.redraw_mode = redraw_mode;
                    sdUpscale.tile_width = tile_width;
                    sdUpscale.tile_height = tile_height;
                    sdUpscale.mask_blur = mask_blur;
                    sdUpscale.padding = padding;
                }
            }
            fold_2 = EditorGUILayout.Foldout(fold_2, "Seams fix", true);
            if (fold_2)
            {
                EditorGUI.BeginChangeCheck();
                var seams_fix_type = sdUpscale.seams_fix_type;
                var seams_fix_denoise = sdUpscale.seams_fix_denoise;
                var seams_fix_width = sdUpscale.seams_fix_width;
                var seams_fix_mask_blur = sdUpscale.seams_fix_mask_blur;
                var seams_fix_padding = sdUpscale.seams_fix_padding;
                seams_fix_type = EditorGUILayout.Popup("Type", seams_fix_type, UltimateSDUpscale.seams_fix_types);
                if (seams_fix_type != 0)
                {
                    seams_fix_denoise = EditorGUILayout.Slider("Denoise", seams_fix_denoise, 0, 1);
                    seams_fix_width = EditorGUILayout.IntSlider("Width", seams_fix_width, 0, 128);
                    seams_fix_mask_blur = EditorGUILayout.IntSlider("Mask blur", seams_fix_mask_blur, 0, 64);
                    seams_fix_padding = EditorGUILayout.IntSlider("Padding", seams_fix_padding, 0, 128);
                }
                if (EditorGUI.EndChangeCheck())
                {
                    sdUpscale.seams_fix_type = seams_fix_type;
                    sdUpscale.seams_fix_denoise = seams_fix_denoise;
                    sdUpscale.seams_fix_width = seams_fix_width;
                    sdUpscale.seams_fix_mask_blur = seams_fix_mask_blur;
                    sdUpscale.seams_fix_padding = seams_fix_padding;
                }
            }
            EditorGUI.BeginChangeCheck();
            var save_upscaled_image = sdUpscale.save_upscaled_image;
            var save_seams_fix_image = sdUpscale.save_seams_fix_image;
            save_upscaled_image = EditorGUILayout.ToggleLeft("Upscaled", save_upscaled_image);
            save_seams_fix_image = EditorGUILayout.ToggleLeft("Seams fix", save_seams_fix_image);
            
            if (EditorGUI.EndChangeCheck())
            {
                sdUpscale.save_upscaled_image = save_upscaled_image;
                sdUpscale.save_seams_fix_image = save_seams_fix_image;
            }

        }
        
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
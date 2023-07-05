#define GETLOARMODELS

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GraphProcessor;
using Newtonsoft.Json;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Lora")]
    public class SDLoraNode : SDNode
    {
        [Input("Prompt")] public string prompt;
        [Input("Strength"), ShowAsDrawer] public float strength = 1;
        [Output("Lora")] public string outLoraString;
        
        [HideInInspector]
        public string loraPrompt = "";
        [HideInInspector]
        public string lora = "";
        
        [HideInInspector]
        public string loraDir;
        [HideInInspector]
        public List<string> loraNames;
        [HideInInspector]
        public Dictionary<string, string> loraPrompts;
        [HideInInspector]
        public int currentIndex = 0;

        [HideInInspector] public bool useLoraBlockWeight = false;
        [HideInInspector]
        public string loraBlockWeightPresetName;
        [HideInInspector]
        public int currentLoraBlockWeightPresetIndex = 0;

        /// <summary>
        /// Get the list of available Stable Diffusion models.
        /// </summary>
        /// <returns></returns>
        public IEnumerator ListLoraAsync(UnityAction unityAction=null)
        {
#if GETLOARMODELS
            if (loraNames == null)
                loraNames = new List<string>();
            else
                loraNames.Clear();
            // Stable diffusion API url for getting the models list
            string url = SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.LorasAPI;
            SDUtil.Log(url);

            UnityWebRequest request = new UnityWebRequest(url, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
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
                SDUtil.Log(request.downloadHandler.text);
                // Deserialize the response to a class
                SDLoraModel[] ms = JsonConvert.DeserializeObject<SDLoraModel[]>(request.downloadHandler.text);
                if (loraPrompts == null)
                    loraPrompts = new Dictionary<string, string>();
                foreach (var m in ms)
                {
                    //loraPrompts[m.name] = m.prompt;
                    loraNames.Add(m.name);
                }
                unityAction?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
#else
            // Stable diffusion API url for getting the models list
            string url = SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.DataDirAPI;

            UnityWebRequest request = new UnityWebRequest(url, "GET");
            request.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
        
            if (SDGraphResource.SdGraphDataHandle.UseAuth && !SDGraphResource.SdGraphDataHandle.Username.Equals("") && !SDGraphResource.SdGraphDataHandle.Password.Equals(""))
            {
                SDUtil.Log("Using API key to authenticate");
                byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.Username + ":" + SDGraphResource.SdGraphDataHandle.Password);
                string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            }
        
            yield return request.SendWebRequest();
            
            try
            {
                // Deserialize the response to a class
                SDDataDir m = JsonConvert.DeserializeObject<SDDataDir>(request.downloadHandler.text);
                // Keep only the names of the models
                loraDir = m.lora_dir;
                string[] files = Directory.GetFiles(loraDir, "*.safetensors", SearchOption.AllDirectories);
                if (loraNames == null) loraNames = new List<string>();
                loraNames.Clear();
                foreach (var f in files)
                {
                    loraNames.Add(Path.GetFileNameWithoutExtension(f));
                }
                unityAction?.Invoke();
            }
            catch (Exception)
            {
                SDUtil.LogError(url + " " + request.downloadHandler.text);
            }
#endif
        }

        protected override void Process()
        {
            GetPort(nameof(prompt), null).PullData();
            if (!string.IsNullOrEmpty(loraPrompt)&&!loraPrompt.EndsWith(","))
                loraPrompt += ",";
            string loraBlockWeight = "";
            if (useLoraBlockWeight)
            {
                loraBlockWeight = $":{SDGraphResource.SdGraphDataHandle.loraBlockWeight[loraBlockWeightPresetName]}";
            }
            outLoraString = $"{prompt},{loraPrompt}<lora:{lora}:{strength}{loraBlockWeight}>";
        }
    }

}


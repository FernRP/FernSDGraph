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
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Txt2Img")]
    public class SDTxt2ImgNode : WaitableNode
    {
        [Input(name = "Prompt")] public Prompt prompt;

        [Input(name = "Width"), ShowAsDrawer] public int width = 512;
        [Input(name = "Height"), ShowAsDrawer] public int height = 512;
        [Input(name = "Step"), ShowAsDrawer] public int step = 20;
        [Input(name = "CFG"), ShowAsDrawer] public int cfg = 7;
        [Input(name = "Seed"), ShowAsDrawer]
        public long seed = -1;

        [Output("Image")] public Texture2D outputImage;
        [Output("Seed")] public long outSeed;
       
        public string SamplerMethod = "Euler";
        private float aspect;
       
        [HideInInspector]
        public float progress;
        [HideInInspector]
        public DateTime startTime;
        [HideInInspector]
        public float speed; // it/s
        [HideInInspector]
        public float init_speed; // it/s
        [HideInInspector]
        public int cur_step;
        [HideInInspector]
        public bool isExecuting = false;

        public ControlNetData controlNetData = null;

        public override string name => "SD Txt2Img";
        
        public Action<long, long> OnUpdateSeedField;

        public Texture2D OutputImage
        {
            get
            {
                if (outputImage == null)
                    outputImage = new Texture2D(width, height, DefaultFormat.LDR, TextureCreationFlags.None);

                return outputImage;
            }
        }
        
        public void Complete()
        {
            isExecuting = false;
            EditorPrefs.SetFloat("SD.GPU.it_speed", speed);
            EditorPrefs.SetFloat("SD.GPU.init_speed", init_speed);
        }

        protected override void Disable()
        {
            base.Disable();
            if (outputImage != null)
            {
                Object.DestroyImmediate(outputImage);
                outputImage = null;
            }
        }
        
        public void Init()
        {
            cur_step = -1;
            startTime = DateTime.Now;
            progress = 0;
            speed = EditorPrefs.GetFloat("SD.GPU.it_speed", 0.0001f);
            init_speed = EditorPrefs.GetFloat("SD.GPU.init_speed", 0.0001f);
            isExecuting = true;
        }

        
        private IEnumerator UpdateGenerationProgress()
        {
            // Generate the image
            HttpWebRequest httpWebRequest = null;
            try
            {
                // Make a HTTP POST request to the Stable Diffusion server
                //var txt2ImgAPI = controlNet == defaultControlNet ? SDDataHandle.TextToImageAPI : SDDataHandle.ControlNetTex2Img;
                var txt2ImgAPI = SDGraphResource.SdGraphDataHandle.ProgressAPI;
                httpWebRequest = (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.serverURL + txt2ImgAPI);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "GET";
                httpWebRequest.SetRequestAuthorization();
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message + "\n\n" + e.StackTrace);
            }
            // Read the output of generation
            if (httpWebRequest != null)
            {
                // Wait that the generation is complete before procedding
                Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();

                var currentTime = DateTime.Now;
                TimeSpan oTime = currentTime.Subtract(startTime);
                if (cur_step == -1)
                {
                    var pro = (float)oTime.TotalSeconds * init_speed;
                    progress = Mathf.Min(1, pro);
                    //TODO: Update View
                }
                while (!webResponse.IsCompleted)
                {                
                    yield return new WaitForSeconds(100);
                }
                
                // Stream the result from the server
                var httpResponse = webResponse.Result;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    // Decode the response as a JSON string
                    string result = streamReader.ReadToEnd();
                    
                    // Deserialize the JSON string into a data structure
                    SDResponseProgress json = JsonConvert.DeserializeObject<SDResponseProgress>(result);
                    // If no image, there was probably an error so abort
                    if (json != null && !string.IsNullOrEmpty(json.current_image))
                    {
                        byte[] imageData = Convert.FromBase64String(json.current_image);
                        OutputImage.LoadImage(imageData);
                    
                        if (json.state != null && json.state.sampling_step > cur_step) 
                        {
                            cur_step = json.state.sampling_step;
                            if (cur_step == 0)
                                init_speed = 1 / (float)oTime.TotalSeconds;
                            else
                                speed = cur_step/(float)(oTime.TotalSeconds - 1 / init_speed);
                        }
                    
                        //TODO: Update All View
                    }
                    
                    if (cur_step == -1) yield break;
                    var pro = Mathf.Clamp((((float)oTime.TotalSeconds - 1f / init_speed) * speed) / (step - 1f), cur_step/(step - 1.0f), (cur_step + 1)/(step - 1.0f) - 0.001f);
                    progress = Mathf.Min(1, pro);
                }
            }
        }
        
        IEnumerator GenerateAsync()
        {
            long seed = this.seed;
            if (seed == -1)
                seed = SDUtil.GenerateRandomLong(-1, Int64.MaxValue);
            // Generate the image
            HttpWebRequest httpWebRequest = null;
            try
            {
                // Make a HTTP POST request to the Stable Diffusion server
                //var txt2ImgAPI = controlNet == defaultControlNet ? SDGraphResource.SdGraphDataHandle.TextToImageAPI : SDGraphResource.SdGraphDataHandle.ControlNetTex2Img;
                var txt2ImgAPI = SDGraphResource.SdGraphDataHandle.TextToImageAPI;
                httpWebRequest = (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.GetServerURL() + txt2ImgAPI);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 1000 * 60 * 5; 

                // add auth-header to request
                if (SDGraphResource.SdGraphDataHandle.GetUseAuth() && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() + ":" + SDGraphResource.SdGraphDataHandle.GetPassword());
                    string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encodedCredentials);
                }

                string json;
                // Send the generation parameters along with the POST request
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    if (controlNetData == null)
                    {
                        SDParamsInTxt2Img sd = new SDParamsInTxt2Img();
                        sd.prompt = prompt.positive;
                        sd.negative_prompt = prompt.negative;
                        sd.steps = step;
                        sd.cfg_scale = cfg;
                        sd.width = width;
                        sd.height = height;
                        sd.seed = seed;
                        sd.tiling = false;
                        sd.sampler_name = SamplerMethod;
                        // Serialize the input parameters
                        json = JsonConvert.SerializeObject(sd);
                    }
                    else
                    {
                        SDParamsInTxt2ImgContronlNet sd = new SDParamsInTxt2ImgContronlNet();
                        sd.prompt = prompt.positive;
                        sd.negative_prompt = prompt.negative;
                        sd.steps = step;
                        sd.cfg_scale = cfg;
                        sd.width = width;
                        sd.height = height;
                        sd.seed = seed;
                        sd.tiling = false;
                        sd.sampler_name = SamplerMethod;
                        if (controlNetData != null)
                        {
                            SDUtil.Log("use controlnet");
                            sd.alwayson_scripts = new ALWAYSONSCRIPTS();
                            sd.alwayson_scripts.controlnet = new ControlNetDataArgs();
                            sd.alwayson_scripts.controlnet.args = new[] { controlNetData };
                        }
                        // Serialize the input parameters
                        json = JsonConvert.SerializeObject(sd);
                    }
                    
                    // Send to the server
                    streamWriter.Write(json);
                }
            }
            catch (Exception e)
            {
                SDUtil.LogError(e.Message + "\n\n" + e.StackTrace);
            }

            // Read the output of generation
            if (httpWebRequest != null)
            {
                // Wait that the generation is complete before procedding
                Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();

                while (!webResponse.IsCompleted)
                {
#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
                    yield return UpdateGenerationProgress();
                }


                // Stream the result from the server
                var httpResponse = webResponse.Result;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    // Decode the response as a JSON string
                    string result = streamReader.ReadToEnd();
                    
                    // Deserialize the JSON string into a data structure
                    SDResponseTxt2Img json = JsonConvert.DeserializeObject<SDResponseTxt2Img>(result);

                    // If no image, there was probably an error so abort
                    if (json.images == null || json.images.Length == 0)
                    {
                        SDUtil.LogError(
                            "No image was return by the server. This should not happen. Verify that the server is correctly setup.");

#if UNITY_EDITOR
                        EditorUtility.ClearProgressBar();
#endif
                        yield break;
                    }

                    // Decode the image from Base64 string into an array of bytes
                    byte[] imageData = Convert.FromBase64String(json.images[0]);
                    OutputImage.LoadImage(imageData);
                    Complete();
                    try
                    {
                        // Read the generation info back (only seed should have changed, as the generation picked a particular seed)
                        if (json.info != "")
                        {
                            SDParamsOutTxt2Img info = JsonConvert.DeserializeObject<SDParamsOutTxt2Img>(json.info);

                            // Read the seed that was used by Stable Diffusion to generate this result
                            outSeed = info.seed;
                            if (!Directory.Exists(SDGraphResource.SdGraphDataHandle.SavePath))
                                Directory.CreateDirectory(SDGraphResource.SdGraphDataHandle.SavePath);
                            File.WriteAllBytes($"{SDGraphResource.SdGraphDataHandle.SavePath}/img_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{outSeed}.png", imageData);
                            OnUpdateSeedField?.Invoke(this.seed,outSeed);
                            SDUtil.Log("Txt 2 Img");
                            InvokeOnExecuteFinsih();
                        }
                    }
                    catch (Exception e)
                    {
                        SDUtil.LogError(e.Message + "\n\n" + e.StackTrace);
                    }
                }
            }
            yield return null;
        }

        protected override void Execute()
        {
            GetPort(nameof(prompt), null).PullData();
            EditorCoroutineUtility.StartCoroutine(GenerateAsync(), this);
        }
    }
}
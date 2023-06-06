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
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Txt2Img")]
    public class SDTxt2ImgNode : LinearSDProcessorNode
    {
        [Input(name = "Upscaler")] public HiresUpscaler upscaler;
        [Input(name = "Prompt")] public Prompt prompt;

        [Input(name = "Width"), ShowAsDrawer] public int width = 512;
        [Input(name = "Height"), ShowAsDrawer] public int height = 512;
        [Input(name = "Step"), ShowAsDrawer] public int step = 20;
        [Input(name = "CFG"), ShowAsDrawer] public int cfg = 7;
        [Input(name = "Seed"), ShowAsDrawer] public long seed = -1;
        [Input(name = "Tiling"), ShowAsDrawer] public bool isTiling = false;
        [Input(name = "Extension")] public string extension = null;

        [Output("Image")] public Texture2D outputImage;
        [Output("Seed")] public long outSeed;

        [HideInInspector] public float progress;
        [HideInInspector] public DateTime pre_step_time;
        [HideInInspector] public int pre_step;
        [HideInInspector] public int pre_step_count = 20;
        [HideInInspector] public int pre_job_no;
        [HideInInspector] public int job_no_count;
        [HideInInspector] public float speed; // it/s
        [HideInInspector] public string samplerMethod = "Euler";
        [HideInInspector] public string savePath = null;

        public override string name => "SD Txt2Img";

        public Action<long, long> OnUpdateSeedField;

        public override Texture previewTexture => OutputImage;

        public Texture2D OutputImage
        {
            get
            {
                if (outputImage == null)
                    outputImage = new Texture2D(width, height, DefaultFormat.LDR, TextureCreationFlags.None);

                return outputImage;
            }
        }

        protected override void Enable()
        {
            hasPreview = true;
            base.Enable();
        }

        public override void OnNodeCreated()
        {
            hasPreview = true;
            base.OnNodeCreated();   
        }

        public void Complete()
        {
            pre_job_no = -1;
        }

        protected override void Destroy()
        {
            base.Destroy();
            if (outputImage != null)
            {
                Object.DestroyImmediate(outputImage);
                outputImage = null;
            }
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
                
                httpWebRequest = (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.GetServerURL() + txt2ImgAPI);
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
                while (!webResponse.IsCompleted)
                {                
                    var TotalSeconds = (float)DateTime.Now.Subtract(pre_step_time).TotalSeconds;
                    progress = Mathf.Min((float)pre_step / pre_step_count /*(json.progress)*/ + TotalSeconds * speed / pre_step_count, (pre_step + 1f) / pre_step_count);
                    progress = (pre_job_no + Mathf.Min(1, progress)) / job_no_count;
                    if (job_no_count <= pre_job_no)
                        progress = 1;
                    onProgressUpdate?.Invoke(progress);
                    yield return new WaitForSeconds(100);
                }
                
                // Stream the result from the server
                try
                {
                    var httpResponse = webResponse.Result;

                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        // Decode the response as a JSON string
                        string result = streamReader.ReadToEnd();
                        
                        // Deserialize the JSON string into a data structure
                        SDResponseProgress json = JsonConvert.DeserializeObject<SDResponseProgress>(result);
                        // If no image, there was probably an error so abort
                        if (json == null) yield break;
                        if (!string.IsNullOrEmpty(json.current_image))
                        {
                            byte[] imageData = Convert.FromBase64String(json.current_image);
                            OutputImage.LoadImage(imageData);
                        }

                        if (json.state == null) yield break;
                        pre_step_count = json.state.sampling_steps;
                        job_no_count = json.state.job_count;
                        if (json.state.job_no != pre_job_no)
                        {
                            pre_step_time = DateTime.Now;
                            progress = 0;
                            pre_step = 0;
                            if (json.state.job_no == 0) speed = 0.01f;
                            pre_job_no = json.state.job_no;
                        }

                        if (json.state.sampling_step <= pre_step) yield break;
                        speed = (json.state.sampling_step - pre_step)/(float)DateTime.Now.Subtract(pre_step_time).TotalSeconds;;
                        pre_step_time = DateTime.Now;
                        pre_step = json.state.sampling_step;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
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
                httpWebRequest =
                    (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.GetServerURL() + txt2ImgAPI);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.Timeout = 1000 * 60 * 5;

                // add auth-header to request
                if (SDGraphResource.SdGraphDataHandle.GetUseAuth() &&
                    !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) &&
                    !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() +
                                                                  ":" + SDGraphResource.SdGraphDataHandle
                                                                      .GetPassword());
                    string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encodedCredentials);
                }

                string json;
                // Send the generation parameters along with the POST request
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    SDParamsInTxt2Img sd = new SDParamsInTxt2Img();
                    sd.prompt = prompt.positive;
                    sd.negative_prompt = prompt.negative;
                    sd.steps = step;
                    sd.cfg_scale = cfg;
                    sd.width = width;
                    sd.height = height;
                    sd.tiling = isTiling;
                    sd.seed = seed; 
                    sd.sampler_name = samplerMethod;
                    sd.enable_hr = false;
                    if (upscaler != null && !string.IsNullOrEmpty(upscaler.hr_upscaler) && !upscaler.hr_upscaler.ToLower().Equals("none"))
                    {
                        sd.enable_hr = true;
                        sd.hr_second_pass_steps = upscaler.hr_second_pass_steps;
                        sd.hr_upscaler = upscaler.hr_upscaler;
                        sd.hr_resize_x = upscaler.hr_resize_x;
                        sd.hr_resize_y = upscaler.hr_resize_y;
                        sd.denoising_strength = upscaler.denoising_strength;
                        sd.hr_scale = upscaler.hr_scale;
                    }

                    // Serialize the input parameters
                    json = JsonConvert.SerializeObject(sd);
                    if (!string.IsNullOrEmpty(extension))
                    {
                        var scriptsHeader = ",\"alwayson_scripts\":{";
                        var scriptslast = "}";
                        var scriptsContent = $"{scriptsHeader}{extension}{scriptslast}";
                        json = json.Insert(json.Length - 1, scriptsContent);
                    }
                    SDUtil.Log($"Txt2Img Json Data: {json}");

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

                onProgressStart?.Invoke();
                while (!webResponse.IsCompleted)
                {
                    yield return UpdateGenerationProgress();
                }
                onProgressFinish?.Invoke();

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
                            OutputImage.name = info.seed.ToString();
                            string tempSavePath = null;
                            if (!string.IsNullOrEmpty(savePath) && Directory.Exists(SDGraphResource.SdGraphDataHandle.SavePath))
                            {
                                tempSavePath = savePath;
                            }
                            else
                            {
                                if (!Directory.Exists(SDGraphResource.SdGraphDataHandle.SavePath))
                                    Directory.CreateDirectory(SDGraphResource.SdGraphDataHandle.SavePath);
                                tempSavePath =
                                    $"{SDGraphResource.SdGraphDataHandle.SavePath}/img_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{outSeed}.png";
                            }
                            File.WriteAllBytes(tempSavePath,imageData);
                            OnUpdateSeedField?.Invoke(this.seed, outSeed);
                            AssetDatabase.Refresh();
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

        protected override IEnumerator Execute()
        {
            settings.sizeMode = OutputSizeMode.Absolute;
            settings.width = width;
            settings.height = height;
            pre_job_no = -1;
            yield return GenerateAsync();
        }
    }
}
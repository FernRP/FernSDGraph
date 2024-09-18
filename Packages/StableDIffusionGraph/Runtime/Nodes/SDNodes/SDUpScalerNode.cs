using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GraphProcessor;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD UpScaler")]
    public class SDUpScalerNode : LinearSDProcessorNode
    {
        public enum SizeMode
        {
            Upscaling_Resize = 0,
            Upscaling_ResizeWh = 1
        }
        
        [Input("Image")] public Texture inputImage;
        [Input("Size Mode"), ShowAsDrawer] public SizeMode resize_mode = SizeMode.Upscaling_Resize;
        [Input("Size"), ShowAsDrawer]  public float upscaling_resize = 2;
        [Input("Size Width"), ShowAsDrawer] public int upscaling_resize_w = 512;
        [Input("Size Height"), ShowAsDrawer] public int upscaling_resize_h = 512;
        
        [HideInInspector] public string upscaler_1 = "None";
        [HideInInspector] public int upscaler1_index = 0;
        [HideInInspector] public string upscaler_2 = "None";
        [HideInInspector] public int upscaler2_index = 0;
        [HideInInspector] public bool isAutoSave = false;
        [HideInInspector] public string savePath = null;
        
        [Output] public Texture2D outputImage;
        
        public override string name => "SD  UpScaler";
        
        [HideInInspector] public float progress;
        [HideInInspector] public DateTime startTime;
        [HideInInspector] public float speed; // it/s
        [HideInInspector] public float init_speed = 0.0001f; // it/s
        [HideInInspector] public int cur_step;
        [HideInInspector] public bool isExecuting = false;

        
        public Texture2D OutputImage
        {
            get
            {
                if (outputImage == null)
                {
                    if (inputImage != null)
                    {
                        outputImage = new Texture2D(inputImage.width, inputImage.height, DefaultFormat.LDR, TextureCreationFlags.None);
                    }
                    else
                    {
                        outputImage = new Texture2D(512, 512, DefaultFormat.LDR, TextureCreationFlags.None);
                    }
                }
                
                return outputImage;
            }
        }
        
        public override Texture previewTexture => OutputImage;

        protected override void Enable()
        {
            hasPreview = true;
            base.Enable();
        }
        
        IEnumerator UpdateScale()
        {
            HttpWebRequest httpWebRequest = null;
            //try
            {
                // Make a HTTP POST request to the Stable Diffusion server
                httpWebRequest =
                    (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.ExtraSingleImage);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                SDUtil.Log($"use UpScale server: {SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.ExtraSingleImage}");

                // add auth-header to request
                if (SDGraphResource.SdGraphDataHandle.GetUseAuth() && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetUserName()) && !string.IsNullOrEmpty(SDGraphResource.SdGraphDataHandle.GetPassword()))
                {
                    httpWebRequest.PreAuthenticate = true;
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(SDGraphResource.SdGraphDataHandle.GetUserName() + ":" + SDGraphResource.SdGraphDataHandle.GetPassword());
                    string encodedCredentials = Convert.ToBase64String(bytesToEncode);
                    httpWebRequest.Headers.Add("Authorization", "Basic " + encodedCredentials);
                }
                // Send the generation parameters along with the POST request
                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    string inputImgString = "";
                    if (inputImage != null)
                    {
                        byte[] inputImgBytes = SDUtil.TextureToTexture2D(inputImage).EncodeToPNG();
                        inputImgString = Convert.ToBase64String(inputImgBytes);
                    }
                   

                    string json;
                    ExtraSingleImageParam extra = new ExtraSingleImageParam()
                    {
                        image = inputImgString,
                        resize_mode = (int)this.resize_mode,
                        upscaling_resize = this.upscaling_resize,
                        upscaling_resize_w = this.upscaling_resize_w,
                        upscaling_resize_h = this.upscaling_resize_h,
                        upscaler_1 = this.upscaler_1,
                        upscaler_2 = this.upscaler_2,
                    };

                    json = JsonConvert.SerializeObject(extra);
                    
                    SDUtil.Log($"use UpScale: {json}");
                    
                    // Send to the server
                    streamWriter.Write(json);
                }
            }
            
            // Wait that the generation is complete before procedding
            Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();

            onProgressStart?.Invoke();
            while (!webResponse.IsCompleted)
            {
                yield return UpdateGenerationProgress();
            }
            onProgressFinish?.Invoke();

            // Stream the result from the server
            var httpResponse = webResponse.Result;

            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                // Decode the response as a JSON string
                string result = streamReader.ReadToEnd();

                // Deserialize the JSON string into a data structure
                ExtraSingleImageResponse json = JsonConvert.DeserializeObject<ExtraSingleImageResponse>(result);

                // If no image, there was probably an error so abort
                if (string.IsNullOrEmpty(json.image))
                {
                    SDUtil.LogError(
                        "No image was return by the server. This should not happen. Verify that the server is correctly setup.");

#if UNITY_EDITOR
                    EditorUtility.ClearProgressBar();
#endif
                    yield break;
                }

                // Decode the image from Base64 string into an array of bytes
                byte[] imageData = Convert.FromBase64String(json.image);
                OutputImage.LoadImage(imageData);

                if (isAutoSave)
                {
                    string tempSavePath = null;
                    if (!string.IsNullOrEmpty(savePath) && Directory.Exists(SDGraphResource.SdGraphDataHandle.SavePath_Upscale))
                    {
                        tempSavePath = savePath;
                    }
                    else
                    {
                        if (!Directory.Exists(SDGraphResource.SdGraphDataHandle.SavePath_Upscale))
                            Directory.CreateDirectory(SDGraphResource.SdGraphDataHandle.SavePath_Upscale);
                        tempSavePath =
                            $"{SDGraphResource.SdGraphDataHandle.SavePath_Upscale}/img_{DateTime.Now.ToString("yyyyMMddHHmmss")}.png";
                    }
                    File.WriteAllBytes(tempSavePath,imageData);
                    AssetDatabase.Refresh();
                }
            }

            yield return null;
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

                var currentTime = DateTime.Now;
                TimeSpan oTime = currentTime.Subtract(startTime);
                if (cur_step == -1)
                {
                    var pro = (float)oTime.TotalSeconds * init_speed;
                    progress = Mathf.Min(1, pro);
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
                        progress = json.progress;
                        onProgressUpdate?.Invoke(progress);
                    }
                    
                    if (cur_step == -1) yield break;
                    // TODO:?
                    // var pro = Mathf.Clamp((((float)oTime.TotalSeconds - 1f / init_speed) * speed) / (step - 1f), cur_step/(step - 1.0f), (cur_step + 1)/(step - 1.0f) - 0.001f);
                    // progress = Mathf.Min(1, pro);
                }
            }
        }

        protected override IEnumerator Execute()
        {
            if(inputImage == null) yield break;
            yield return UpdateScale();
            settings.sizeMode = OutputSizeMode.Absolute;
            
            switch (resize_mode)
            {
                case SizeMode.Upscaling_Resize:
                    settings.width = (int)(inputImage.width * upscaling_resize);
                    settings.height = (int)(inputImage.height * upscaling_resize);
                    break;
                case SizeMode.Upscaling_ResizeWh:
                    settings.width = upscaling_resize_w;
                    settings.height = upscaling_resize_h;
                    break;
            }
        }
    }
}
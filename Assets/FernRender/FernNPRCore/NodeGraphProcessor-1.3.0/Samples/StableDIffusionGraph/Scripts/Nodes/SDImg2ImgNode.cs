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
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace FernNPRCore.SDNodeGraph
{
	[System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Img2Img")]
	public class SDImg2ImgNode : LinearSDProcessorNode
	{
		public override string		name => "SD Img2Img"; 
		
		[Input("Image")] public Texture2D InputImage;
		[Input("Mask")] public Texture2D MaskImage;
		[Input("ControlNet")] public ControlNetData controlNetData;
		[Input("Prompt")] public Prompt prompt;
		[Input("Step"), ShowAsDrawer] public int step = 20;
		[Input("CFG"), ShowAsDrawer] public int cfg = 7;
		[Input("Denoising Strength"), ShowAsDrawer] public float denisoStrength = 0.75f;
		[Input("Width"), ShowAsDrawer] public int width = 512;
		[Input("Height"), ShowAsDrawer] public int height = 512;
		[Input("Seed")] public long seed = -1;
		
		[Output("Image")] public Texture2D outputImage;
		[Output("Seed")] public long outSeed;
		
		[HideInInspector]
		public string samplerMethod = "Euler";
		
		[ChangeEvent(true)]
		public bool Inpaint = false;
		
		[VisibleIf(nameof(Inpaint), true)]
		public int inpainting_fill = 0;
		[VisibleIf(nameof(Inpaint), true)]
		public bool inpaint_full_res = true;
		[VisibleIf(nameof(Inpaint), true)]
		public int inpaint_full_res_padding = 32;
		[VisibleIf(nameof(Inpaint), true)]
		public int inpainting_mask_invert = 0;
		[VisibleIf(nameof(Inpaint), true)]
		public int mask_blur = 0;
		
		public Action<long, long> OnUpdateSeedField;
		
		private float aspect;
		private Color whiteColor = new Color(1, 1, 1, 1);

		public Texture2D OutputImage
		{
			get
			{
				if (outputImage == null)
					outputImage = new Texture2D(width, height, DefaultFormat.LDR, TextureCreationFlags.None);
                
				return outputImage;
			}
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

		protected override void Execute()
		{
			EditorCoroutineUtility.StartCoroutine(GenerateAsync(), this);
		}
		
		IEnumerator GenerateAsync()
        {
            long seed = this.seed;
            if (seed == -1)
                seed = SDUtil.GenerateRandomLong(-1, Int64.MaxValue);
            // Generate the image
            HttpWebRequest httpWebRequest = null;
            //try
            {
                // Make a HTTP POST request to the Stable Diffusion server
                httpWebRequest =
                    (HttpWebRequest)WebRequest.Create(SDGraphResource.SdGraphDataHandle.GetServerURL() + SDGraphResource.SdGraphDataHandle.ImageToImageAPI);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

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

                    byte[] inputImgBytes = InputImage.EncodeToPNG();
                    string inputImgString = Convert.ToBase64String(inputImgBytes);
                    string maskImgString = "";

                    string json;
                    
                    if (controlNetData != null)
                    {
                        SDUtil.Log("use ControlNet");
                        SDParamsInImg2ImgControlNet sd = new SDParamsInImg2ImgControlNet();

                        sd.init_images = new string[] { inputImgString };
                        sd.prompt = prompt.positive;
                        sd.negative_prompt = prompt.negative;
                        sd.steps = step;
                        sd.cfg_scale = cfg;
                        sd.denoising_strength = denisoStrength;
                        sd.width = Screen.width;
                        sd.height = Screen.height;
                        sd.seed = seed;
                        sd.tiling = false;
                        sd.sampler_name = samplerMethod;
                        sd.alwayson_scripts = new ALWAYSONSCRIPTS();
                        sd.alwayson_scripts.controlnet = new ControlNetDataArgs();
                        sd.alwayson_scripts.controlnet.args = new[] { controlNetData };
                        json = JsonConvert.SerializeObject(sd);
                    }else if (MaskImage != null)
                    {
                        SDUtil.Log("use Mask");
                        
                        SDParamsInImg2ImgMask sd = new SDParamsInImg2ImgMask();
                        sd.init_images = new string[] { inputImgString };
                        sd.prompt = prompt.positive;
                        sd.negative_prompt = prompt.negative;
                        sd.steps = step;
                        sd.cfg_scale = cfg;
                        sd.denoising_strength = denisoStrength;
                        sd.width = Screen.width;
                        sd.height = Screen.height;
                        sd.seed = seed;
                        sd.tiling = false;
                        sd.sampler_name = samplerMethod;
                        byte[] maskImgBytes = MaskImage.EncodeToPNG();
                        maskImgString = Convert.ToBase64String(maskImgBytes);
                        sd.mask = maskImgString;
                        sd.inpainting_fill = inpainting_fill;
                        sd.inpaint_full_res = inpaint_full_res;
                        sd.inpaint_full_res_padding = inpaint_full_res_padding;
                        sd.inpainting_mask_invert = inpainting_mask_invert;
                        sd.mask_blur = mask_blur;

                        json = JsonConvert.SerializeObject(sd);
                    }
                    else
                    {
                        SDUtil.Log("use Only Img2Img");
                        SDParamsInImg2Img sd = new SDParamsInImg2Img();

                        sd.init_images = new string[] { inputImgString };
                        sd.prompt = prompt.positive;
                        sd.negative_prompt = prompt.negative;
                        sd.steps = step;
                        sd.cfg_scale = cfg;
                        sd.denoising_strength = denisoStrength;
                        sd.width = Screen.width;
                        sd.height = Screen.height;
                        sd.seed = seed;
                        sd.tiling = false;
                        sd.sampler_name = samplerMethod;

                        json = JsonConvert.SerializeObject(sd);
                    }
                    
                    // Send to the server
                    streamWriter.Write(json);
                }
            }
            // catch (Exception e)
            // {
            //     SDUtil.LogError(e.Message + "\n\n" + e.StackTrace);
            // }

            // Read the output of generation
            if (httpWebRequest != null)
            {
                // Wait that the generation is complete before procedding
                Task<WebResponse> webResponse = httpWebRequest.GetResponseAsync();

                while (!webResponse.IsCompleted)
                {
                    //if (SDGraphResource.SdGraphDataHandle.UseAuth && !SDGraphResource.SdGraphDataHandle.Username.Equals("") && !SDGraphResource.SdGraphDataHandle.Password.Equals(""))
                        //UpdateGenerationProgressWithAuth();
                        // else
                        // UpdateGenerationProgress();

                        yield return new WaitForSeconds(0.5f);
                }

                // Stream the result from the server
                var httpResponse = webResponse.Result;

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    // Decode the response as a JSON string
                    string result = streamReader.ReadToEnd();

                    // Deserialize the JSON string into a data structure
                    SDResponseImg2Img json = JsonConvert.DeserializeObject<SDResponseImg2Img>(result);

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
                            OnUpdateSeedField?.Invoke(this.seed, outSeed);
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
	}
}

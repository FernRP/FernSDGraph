#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityEngine.SDGraph
{
    //[CreateAssetMenu(fileName = "SDGraphDataHandle", menuName = "SDGraph/SDGraphDataHandle")]
    public class SDGraphDataHandle : ScriptableObject
    {
        [Header("Stable Diffuse")]
        public string serverURL = "http://127.0.0.1:7860";
        public bool UseAuth = false;
        public string Username = "";
        public string Password = "";
        
        [HideInInspector] public string ModelsAPI = "/sdapi/v1/sd-models";
        [HideInInspector] public string LorasAPI = "/sdapi/v1/loras";
        [HideInInspector] public string ControlNetTex2Img = "/controlnet/txt2img";
        [HideInInspector] public string UpscalersAPI = "/sdapi/v1/upscalers";
        [HideInInspector] public string ControlNetModelList = "/controlnet/model_list";
        [HideInInspector] public string ControlNetMoudleList = "/controlnet/module_list";
        [HideInInspector] public string ControlNetDetect = "/controlnet/detect";
        [HideInInspector] public string TextToImageAPI = "/sdapi/v1/txt2img";
        [HideInInspector] public string ImageToImageAPI = "/sdapi/v1/img2img";
        [HideInInspector] public string OptionAPI = "/sdapi/v1/options";
        [HideInInspector]  public string DataDirAPI = "/sdapi/v1/cmd-flags";
        [HideInInspector] public string ProgressAPI = "/sdapi/v1/progress";
        [HideInInspector] public string ExtraSingleImage = "/sdapi/v1/extra-single-image";
        [HideInInspector] public string SavePath = "TmpPhotos";
        [HideInInspector] public string SavePath_Upscale = "TmpPhotos/UpScale";
        

        [HideInInspector] public string[] samplers = new string[]
        {
            "Euler a", "Euler", "LMS", "Heun", "DPM2", "DPM2 a", "DPM++ 2S a", "DPM++ 2M", "DPM++ SDE", "DPM fast",
            "DPM adaptive",
            "LMS Karras", "DPM2 Karras", "DPM2 a Karras", "DPM++ 2S a Karras", "DPM++ 2M Karras", "DPM++ SDE Karras",
            "DDIM", "PLMS"
        };

        [HideInInspector] public string[] UpscalerModel = new string[]
        {
            "None", "Lanczos", "Nearest", "ESRGAN_4x", "LDSR", "R-ESRGAN 4x+",
            "R-ESRGAN 4x+ Anime6B", "ScuNET", "ScuNET PSNR", "SwinIR 4x"
        };

        [FormerlySerializedAs("loraBlockWeightPresets")] [HideInInspector] public StringStringDic loraBlockWeight;

        [HideInInspector] public string modelUse;
        [HideInInspector] public string loraUse;

        [HideInInspector] public bool OverrideSettings;
        [HideInInspector] public string OverrideServerURL;
        [HideInInspector] public bool OverrideUseAuth;
        [HideInInspector] public string OverrideUsername;
        [HideInInspector] public string OverridePassword;

        [FormerlySerializedAs("shaderDatas")] [Header("Shader")] 
        public SDGraphShaderData shaderData; 

        [Header("Texture")] public SDGraphTextureData TextureData;

        public string GetServerURL()
        {
            if (OverrideSettings && !string.IsNullOrEmpty(OverrideServerURL))
            {
                return OverrideServerURL;
            }

            return serverURL;
        }

        public bool GetUseAuth()
        {
            return OverrideSettings ? OverrideUseAuth : UseAuth;
        }

        public string GetUserName()
        {
            if (OverrideSettings && !string.IsNullOrEmpty(OverrideUsername))
            {
                return OverrideUsername;
            }

            return Username;
        }

        public string GetPassword()
        {
            if (OverrideSettings && !string.IsNullOrEmpty(OverridePassword))
            {
                return OverridePassword;
            }

            return Password;
        }
    }
}
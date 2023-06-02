#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [CreateAssetMenu(fileName = "SDGraphDataHandle", menuName = "FernGraph/NodeGraph/SDGraphDataHandle")]
    public class SDGraphDataHandle : ScriptableObject
    {
        public string serverURL = "http://127.0.0.1:7860";
        public string ModelsAPI = "/sdapi/v1/sd-models";
        public string LorasAPI = "/sdapi/v1/loras";
        public string ControlNetTex2Img = "/controlnet/txt2img";
        public string UpscalersAPI = "/sdapi/v1/upscalers";
        public string ControlNetModelList = "/controlnet/model_list";
        public string ControlNetMoudleList = "/controlnet/module_list";
        public string ControlNetDetect = "/controlnet/detect";
        public string TextToImageAPI = "/sdapi/v1/txt2img";
        public string ImageToImageAPI = "/sdapi/v1/img2img";
        public string OptionAPI = "/sdapi/v1/options";
        public string DataDirAPI = "/sdapi/v1/cmd-flags";
        public string ProgressAPI = "/sdapi/v1/progress";
        public string ExtraSingleImage = "/sdapi/v1/extra-single-image";
        public string SavePath = "TmpPhotos";
        public bool UseAuth = false;
        public string Username = "";
        public string Password = "";

        public string[] samplers = new string[]
        {
            "Euler a", "Euler", "LMS", "Heun", "DPM2", "DPM2 a", "DPM++ 2S a", "DPM++ 2M", "DPM++ SDE", "DPM fast",
            "DPM adaptive",
            "LMS Karras", "DPM2 Karras", "DPM2 a Karras", "DPM++ 2S a Karras", "DPM++ 2M Karras", "DPM++ SDE Karras",
            "DDIM", "PLMS"
        };

        public string[] UpscalerModel = new string[]
        {
            "None", "Lanczos", "Nearest", "ESRGAN_4x", "LDSR", "R-ESRGAN 4x+",
            "R-ESRGAN 4x+ Anime6B", "ScuNET", "ScuNET PSNR", "SwinIR 4x"
        };

        public StringStringDic loraBlockWeightPresets;

        public string modelUse;
        public string loraUse;

        [HideInInspector] public bool OverrideSettings;
        [HideInInspector] public string OverrideServerURL;
        [HideInInspector] public bool OverrideUseAuth;
        [HideInInspector] public string OverrideUsername;
        [HideInInspector] public string OverridePassword;

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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using GraphProcessor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Hires Upscaler (txt2img)")]
    public class SDTxt2ImgUpscalers : SDNode
    {
        [Output] public HiresUpscaler upscaler;
        public override string name => "SD Hires Upscaler";        

        public SDTxt2ImgUpscalers()
        {
            upscaler = new HiresUpscaler();
        }
    }
}
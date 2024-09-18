using System.Collections;
using System.Collections.Generic;
using UnityEngine.SDGraph;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Mirror")]
    public class SDMirror : SDShaderNode
    {
        
        public enum MirrorMode
        {
            MirrorX = 0,
            MirrorY,
            MirrorZ,
            MirrorCorner,
        }
        public enum MirrorType
        {
            Top_Left = 0,
            Top_Right,
            Bottom_Left,
            Bottom_Right,
        }
        
        public enum MirrorZPositionn
        {
            Back = 0,
            Front,
        }
        
        [Input(name = "Source")] public Texture inputImage;
        public override string name => "SD Mirror";

        protected override void Enable()
        {
            shader = SDGraphResource.SdGraphDataHandle.shaderData.mirrorPS;
            base.Enable();
        }

        public MirrorMode mode = MirrorMode.MirrorX;
        [VisibleIf(nameof(mode), MirrorMode.MirrorCorner)]
        public MirrorType mirrorType = MirrorType.Top_Left;
        [VisibleIf(nameof(mode), MirrorMode.MirrorCorner)]
        public MirrorZPositionn cornerZPosition = MirrorZPositionn.Back;
        [Range(0, 1)] public float offset = 0;

        protected override void Process(CommandBuffer cmd)
        {
            base.Process();
            BeforeProcessSetup();
            if(inputImage != null)
                SDUtil.SetTextureWithDimension(material, "_Source", inputImage);
            material.SetFloat("_Offset", offset);
            material.SetFloat("_Mode", (float)mode);
            material.SetFloat("_CornerType", (float)mirrorType);
            material.SetFloat("_CornerZPosition", (float)cornerZPosition);
            output.Update();
        }
    }
}

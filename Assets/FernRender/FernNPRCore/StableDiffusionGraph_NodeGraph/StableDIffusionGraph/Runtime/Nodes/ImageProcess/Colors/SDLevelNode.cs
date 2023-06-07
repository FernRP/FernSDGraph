using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Level")]
    public class SDLevelNode : SDShaderNode
    {
        public enum Mode
        {
            Manual,
            Automatic,
        }

        public enum ChannelMode
        {
            Single,
            Separate,
        }

        [Input(name = "Image")] public Texture inputImage;

        public float min = 0;
        public float max = 1;

        public Mode mode;

        public ChannelMode channelMode = ChannelMode.Single;

        [VisibleIf(nameof(channelMode), ChannelMode.Single)]
        public AnimationCurve interpolationCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [VisibleIf(nameof(channelMode), ChannelMode.Separate)]
        public AnimationCurve interpolationCurveR = AnimationCurve.Linear(0, 0, 1, 1);

        [VisibleIf(nameof(channelMode), ChannelMode.Separate)]
        public AnimationCurve interpolationCurveG = AnimationCurve.Linear(0, 0, 1, 1);

        [VisibleIf(nameof(channelMode), ChannelMode.Separate)]
        public AnimationCurve interpolationCurveB = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField, HideInInspector] HistogramMode histogramMode = HistogramMode.Luminance;

        [SerializeField, HideInInspector] Texture2D curveTexture;

        [SerializeField, HideInInspector] Texture2D curveTextureR;
        Texture2D curveTextureG;
        Texture2D curveTextureB;


        static internal readonly int histogramBucketCount = 256;
        internal ComputeBuffer minMaxBuffer;
        [SerializeField, HideInInspector] public HistogramData histogramData;

        public override string name => "SD Level";

        public override string shaderName => "Hidden/Mixture/Levels";
        
        protected CustomRenderTexture tempRenderTexture;
        public override Texture previewTexture => output;

        protected override void Enable()
        {
            base.Enable();
            minMaxBuffer = new ComputeBuffer(1, sizeof(float) * 2, ComputeBufferType.Structured);
            HistogramUtility.AllocateHistogramData(histogramBucketCount, histogramMode, out histogramData);
        }
        
        protected override void Process(CommandBuffer cmd)
        {
            base.Process(cmd);
            if (inputImage == null) return;
            if (output == null) return;
            BeforeProcessSetup();

            HistogramUtility.ComputeLuminanceMinMax(cmd, minMaxBuffer, inputImage);

            if (channelMode == ChannelMode.Single)
                TextureUtils.UpdateTextureFromCurve(interpolationCurve, ref curveTexture);
            else
            {
                TextureUtils.UpdateTextureFromCurve(interpolationCurveR, ref curveTextureR);
                TextureUtils.UpdateTextureFromCurve(interpolationCurveG, ref curveTextureG);
                TextureUtils.UpdateTextureFromCurve(interpolationCurveB, ref curveTextureB);
            }
            
            material.SetFloat("_Mode", (int)mode);
            material.SetInt("_ChannelMode", (int)channelMode);
            material.SetFloat("_ManualMin", min);
            material.SetFloat("_ManualMax", max);
            material.SetVector("_RcpTextureSize",
                new Vector4(1.0f / inputImage.width, 1.0f / inputImage.height,
                    1.0f / TextureUtils.GetSliceCount(inputImage), 0));
            SDUtil.SetTextureWithDimension(material, "_Input", inputImage);
            SDUtil.SetupDimensionKeyword(material, output.dimension);
            material.SetBuffer("_Luminance", minMaxBuffer);
            material.SetTexture("_InterpolationCurve", curveTexture);
            material.SetTexture("_InterpolationCurveR", curveTextureR);
            material.SetTexture("_InterpolationCurveG", curveTextureG);
            material.SetTexture("_InterpolationCurveB", curveTextureB);
            
            output.Update();
        }
    }
}
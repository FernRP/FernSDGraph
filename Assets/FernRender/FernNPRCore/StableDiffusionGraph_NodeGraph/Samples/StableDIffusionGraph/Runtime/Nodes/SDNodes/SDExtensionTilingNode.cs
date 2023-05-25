using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace FernNPRCore.SDNodeGraph
{
    /// <summary>
    /// Extention From: https://github.com/tjm35/asymmetric-tiling-sd-webui/
    /// </summary>
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Extension Asymmetric Tiling")]
    public class SDExtensionTilingNode : SDNode
    {
        public class AsymmetricTiling
        {
            public bool active = true;
            public bool tilingX = true;
            public bool tilingY = false;
            public int startTilingFromStep = 0;
            public int stoptilingAfterStep = -1;
        }
        
        [Input(name = "Extension")] public string extension = "";
        [Output(name = "Extension")] public string extensionOut = "";

        public bool active = true;
        public bool tilingX = true;
        public bool tilingY = false;
        public int startTilingFromStep = 0;
        public int stoptilingAfterStep = -1;

        private AsymmetricTiling asymmetricTiling;
        private readonly string header = "\"Asymmetric Tiling\":{\"args\":[";
       
        public override string name => "SD Extension Asymmetric Tiling";

        protected override void Enable()
        {
            nodeWidth = 260;
            base.Enable();
            asymmetricTiling = new AsymmetricTiling()
            {
                active = this.active,
                tilingX = this.tilingX,
                tilingY = this.tilingY,
                startTilingFromStep = this.startTilingFromStep,
                stoptilingAfterStep = this.stoptilingAfterStep
            };
        }

        protected override void Process()
        {
            base.Process();
            asymmetricTiling.active = active;
            asymmetricTiling.tilingX = tilingX;
            asymmetricTiling.tilingY = tilingY;
            asymmetricTiling.startTilingFromStep = startTilingFromStep;
            asymmetricTiling.stoptilingAfterStep = stoptilingAfterStep;
            string json = JsonConvert.SerializeObject(asymmetricTiling);
            // just for test
            var activeValue = active ? "true" : "false";
            var tilingXValue = tilingX ? "true" : "false";
            var tilingYValue = tilingY ? "true" : "false";
            if (!string.IsNullOrEmpty(extension))
            {
                extensionOut = $"{extension},{header}{activeValue},{tilingXValue},{tilingYValue},{startTilingFromStep},{stoptilingAfterStep}]" + "}";
            }
            else
            {
                extensionOut = $"{header}{activeValue},{tilingXValue},{tilingYValue},{startTilingFromStep},{stoptilingAfterStep}]" + "}";
            }
        }
    }
}
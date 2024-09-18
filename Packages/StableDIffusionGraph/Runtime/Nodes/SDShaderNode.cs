using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace UnityEngine.SDGraph
{
    public class SDShaderNode : SDNode
    {

        [Output(name = "Out"), Tooltip("Output Texture")]
        public CustomRenderTexture output = null;
        
        [HideInInspector] public Shader shader;
        [HideInInspector] public Material material;

        [HideInInspector] public string shaderGUID;

        protected bool hasMips => false;

        public override Texture previewTexture => output;
        
        protected override void Enable()
        {
            hasPreview = true;
            hasSettings = true;
            base.Enable();
            UpdateShaderAndMaterial();
            if (shader != null && material == null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }

            UpdateTempRenderTexture(ref output, hasMips: hasMips);
            output.material = material;
        }
        
        public void BeforeProcessSetup()
        {
            UpdateTempRenderTexture(ref output, hasMips: hasMips);
        }

        
        void UpdateShaderAndMaterial()
        {
            if (material == null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        public override void Process()
        {
            base.Process();
            UpdateShaderAndMaterial();
            UpdateSettings();
        }
        
        protected override void Disable()
        {
            base.Disable();
            output.Release();
        }
    }
}
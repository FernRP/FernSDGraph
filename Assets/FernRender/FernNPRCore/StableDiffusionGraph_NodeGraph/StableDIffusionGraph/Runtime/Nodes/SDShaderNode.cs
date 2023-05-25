using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    public class SDShaderNode : SDNode
    {

        [Output(name = "Out"), Tooltip("Output Texture")]
        public CustomRenderTexture output = null;
        
        [HideInInspector] public Shader shader;
        [HideInInspector] public Material material;

        [HideInInspector] public string shaderGUID;

        public virtual string shaderName => null;
        
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

            UpdateShader();
            UpdateTempRenderTexture(ref output, hasMips: hasMips);
            output.material = material;
        }
        
        public void BeforeProcessSetup()
        {
            UpdateShader();
            UpdateTempRenderTexture(ref output, hasMips: hasMips);
        }

        
        void UpdateShader()
        {
#if UNITY_EDITOR
            bool updateGUID = false;

            if (shader == null && !string.IsNullOrEmpty(shaderGUID))
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(shaderGUID);
                if (!string.IsNullOrEmpty(path))
                    shader = UnityEditor.AssetDatabase.LoadAssetAtPath<Shader>(path);
            }

            if (shader != null && material.shader != shader)
            {
                material.shader = shader;
                updateGUID = true;
            }

            if (shader != null && (updateGUID || string.IsNullOrEmpty(shaderGUID)))
                UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(shader, out shaderGUID, out long _);
#endif
        }
        
        void UpdateShaderAndMaterial()
        {
            if (shader == null)
                shader = Shader.Find(shaderName);

            if (material != null && material.shader != shader)
                material.shader = shader;

            if (material == null)
            {
                material = new Material(shader);
                material.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector;
            }
        }

        protected override void Process()
        {
            base.Process();
            UpdateSettings();
        }
    }
}
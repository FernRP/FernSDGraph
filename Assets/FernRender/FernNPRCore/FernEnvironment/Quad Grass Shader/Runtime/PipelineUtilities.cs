//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if URP
using UnityEngine.Rendering.Universal;

#if UNITY_2021_2_OR_NEWER
using ForwardRendererData = UnityEngine.Rendering.Universal.UniversalRendererData;
#endif
#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StylizedGrass
{
    public static class PipelineUtilities
    {
        private const string renderDataListFieldName = "m_RendererDataList";
        
#if URP
        /// <summary>
        /// Retrieves a ForwardRenderer asset in the project, based on name
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public static ForwardRendererData GetRenderer(string guid)
        {
#if UNITY_EDITOR
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.Length == 0)
            {
                Debug.LogError("The <i>GrassBendRenderer</i> asset could not be found in the project. Was it renamed or not imported?");
                return null;
            }

            ForwardRendererData data = (ForwardRendererData)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ForwardRendererData));

            return data;
#else
            Debug.LogError("StylizedGrass.PipelineUtilities.GetRenderer() cannot be called in a build, it requires AssetDatabase. References to renderers should be saved beforehand!");
            return null;
#endif
        }
        
        /// <summary>
        /// Checks if a ForwardRenderer has been assigned to the pipeline asset, if not it is added
        /// </summary>
        /// <param name="pass"></param>
        public static void ValidatePipelineRenderers(ScriptableRendererData pass)
        {
            if (pass == null)
            {
                Debug.LogError("Pass is null");
                return;
            }
            
            BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, bindings).GetValue(UniversalRenderPipeline.asset);
            bool isPresent = false;
            
            for (int i = 0; i < m_rendererDataList.Length; i++)
            {
                if (m_rendererDataList[i] == pass) isPresent = true;
            }

            if (!isPresent)
            {
                AddRendererToPipeline(pass);
            }
            else
            {
                //Debug.Log("The " + AssetName + " ScriptableRendererFeature is assigned to the pipeline asset");
            }
        }
        
        private static void AddRendererToPipeline(ScriptableRendererData pass)
        {
            if (pass == null) return;

            BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, bindings).GetValue(UniversalRenderPipeline.asset);
            List<ScriptableRendererData> rendererDataList = new List<ScriptableRendererData>();

            for (int i = 0; i < m_rendererDataList.Length; i++)
            {
                rendererDataList.Add(m_rendererDataList[i]);
            }
            rendererDataList.Add(pass);

            typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, bindings).SetValue(UniversalRenderPipeline.asset, rendererDataList.ToArray());

            //Debug.Log("The <i>" + DrawGrassBenders.AssetName + "</i> renderer is required and was automatically added to the \"" + UniversalRenderPipeline.asset.name + "\" pipeline asset");
        }

        public static void RemoveRendererFromPipeline(ScriptableRendererData pass)
        {
            if (pass == null) return;
            
            BindingFlags bindings = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            ScriptableRendererData[] m_rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, bindings).GetValue(UniversalRenderPipeline.asset);
            List<ScriptableRendererData> rendererDataList = new List<ScriptableRendererData>(m_rendererDataList);
            
            if(rendererDataList.Contains(pass)) rendererDataList.Remove((pass));
            
            typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, bindings).SetValue(UniversalRenderPipeline.asset, rendererDataList.ToArray());
        }
        
        private static int GetDefaultRendererIndex(UniversalRenderPipelineAsset asset)
        {
            return (int)typeof(UniversalRenderPipelineAsset).GetField("m_DefaultRendererIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(asset);;
        }
        
        /// <summary>
        /// Gets the renderer from the current pipeline asset that's marked as default
        /// </summary>
        /// <returns></returns>
        public static ScriptableRendererData GetDefaultRenderer()
        {
            if (UniversalRenderPipeline.asset)
            {
                ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset)
                    .GetField(renderDataListFieldName, BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(UniversalRenderPipeline.asset);
                int defaultRendererIndex = GetDefaultRendererIndex(UniversalRenderPipeline.asset);

                return rendererDataList[defaultRendererIndex];
            }
            else
            {
                Debug.LogError("No Universal Render Pipeline is currently active.");
                return null;
            }
        }
        
        /// <summary>
        /// Checks if a ScriptableRendererFeature is added to the default renderer
        /// </summary>
        /// <param name="addIfMissing"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RenderFeatureAdded<T>(bool addIfMissing = false)
        {
            ScriptableRendererData forwardRenderer = GetDefaultRenderer();
            bool isPresent = false;

            foreach (ScriptableRendererFeature feature in forwardRenderer.rendererFeatures)
            {
                if(feature == null) continue;
                
                if (feature.GetType() == typeof(T)) isPresent = true;
            }
            
            if(!isPresent && addIfMissing) AddRenderFeature<T>(forwardRenderer);
            
            return isPresent;
        }
        
        /// <summary>
        /// Adds a ScriptableRendererFeature to the renderer (default is none is supplied)
        /// </summary>
        /// <param name="forwardRenderer"></param>
        /// <typeparam name="T"></typeparam>
        public static void AddRenderFeature<T>(ScriptableRendererData forwardRenderer = null, bool persistent = true)
        {
            if (forwardRenderer == null) forwardRenderer = GetDefaultRenderer();
            
            ScriptableRendererFeature feature = (ScriptableRendererFeature)ScriptableRendererFeature.CreateInstance(typeof(T).ToString());
            feature.name = typeof(T).ToString();
            
            //Add component https://github.com/Unity-Technologies/Graphics/blob/d0473769091ff202422ad13b7b764c7b6a7ef0be/com.unity.render-pipelines.universal/Editor/ScriptableRendererDataEditor.cs#L180
#if UNITY_EDITOR
            if (persistent)
            {
                AssetDatabase.AddObjectToAsset(feature, forwardRenderer);
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(feature, out var guid, out long localId);
            }
#endif

            //Get feature list
            FieldInfo renderFeaturesInfo = typeof(ScriptableRendererData).GetField("m_RendererFeatures", BindingFlags.Instance | BindingFlags.NonPublic);
            List<ScriptableRendererFeature> m_RendererFeatures = (List<ScriptableRendererFeature>)renderFeaturesInfo.GetValue(forwardRenderer);

            //Modify and set list
            m_RendererFeatures.Add(feature);
            renderFeaturesInfo.SetValue(forwardRenderer, m_RendererFeatures);

            if (persistent)
            {
                //Onvalidate will call ValidateRendererFeatures and update m_RendererPassMap
                MethodInfo validateInfo = typeof(ScriptableRendererData).GetMethod("OnValidate", BindingFlags.Instance | BindingFlags.NonPublic);
                validateInfo.Invoke(forwardRenderer, null);

#if UNITY_EDITOR
                EditorUtility.SetDirty(forwardRenderer);
                AssetDatabase.SaveAssets();
#endif
            }

            if (persistent) Debug.Log("<b>" + feature.name + "</b> was added to the " + forwardRenderer.name + " renderer");
        }

        public static void AssignRendererToCamera(UniversalAdditionalCameraData camData, ScriptableRendererData pass)
        {
            if (UniversalRenderPipeline.asset)
            {
                if (pass)
                {
                    //list is internal, so perform reflection workaround
                    ScriptableRendererData[] rendererDataList = (ScriptableRendererData[])typeof(UniversalRenderPipelineAsset).GetField(renderDataListFieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(UniversalRenderPipeline.asset);

                    for (int i = 0; i < rendererDataList.Length; i++)
                    {
                        if (rendererDataList[i] == pass) camData.SetRenderer(i);
                    }
                }
            }
            else
            {
                Debug.LogError("[StylizedGrassRenderer] No Universal Render Pipeline is currently active.");
            }
        }
#endif
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.SDGraph
{
    [DisallowMultipleRendererFeature]
    public class SDGraphRenderFeature : ScriptableRendererFeature
    {
        class SDGraphRenderPass : ScriptableRenderPass
        {
            private readonly string SDGRAPHCOMMAND = "SDGraphRender";
            private SDGraphRenderSetting m_Setting;
            private FilteringSettings m_FilteringSettings;
            private RenderStateBlock m_RenderStateBlock;
            
            List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

            public SDGraphRenderPass()
            {
                
                RenderQueueRange renderQueueRange = RenderQueueRange.opaque;
                m_FilteringSettings = new FilteringSettings(renderQueueRange);
                
                m_ShaderTagIdList.Clear();
                m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForward"));
                m_ShaderTagIdList.Add(new ShaderTagId("UniversalForwardOnly"));
            }

            public bool Setup(SDGraphRenderSetting setting)
            {
                if (SDGraphRenderHelper.Get() == null) return false;
                if (SDGraphRenderHelper.Get().renderType == SDGraphRenderHelper.SDGraphRenderType.None) return false;
                m_Setting = setting;
                
                return true;
            }

            // This method is called before executing the render pass.
            // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
            // When empty this render pass will render to the active camera render target.
            // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
            // The render pipeline will ensure target setup and clearing happens in a performant manner.
            public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
            {
            }

            // Here you can implement the rendering logic.
            // Use <c>ScriptableRenderContext</c> to issue drawing commands or execute command buffers
            // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
            // You don't have to call ScriptableRenderContext.submit, the render pipeline will call it at specific points in the pipeline.
            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                var cmd = CommandBufferPool.Get(SDGRAPHCOMMAND);
                using (new ProfilingScope(cmd, new ProfilingSampler("SDGraph")))
                {
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                    var sortFlags = renderingData.cameraData.defaultOpaqueSortFlags;
                    var drawSettings =
                        RenderingUtils.CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortFlags);
                    drawSettings.overrideShader = GetCurrentRenderShader();
                    // Render the objects...
                   context.DrawRenderers(renderingData.cullResults, ref drawSettings, ref m_FilteringSettings);
                }
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
            }

            public Shader GetCurrentRenderShader()
            {
                switch (SDGraphRenderHelper.Get().renderType)
                {
                    case SDGraphRenderHelper.SDGraphRenderType.Color:
                        return m_Setting.sdGraphColorPS;
                    case SDGraphRenderHelper.SDGraphRenderType.Depth:
                        return m_Setting.sdGraphDepthPS;
                    case SDGraphRenderHelper.SDGraphRenderType.Normal:
                        return m_Setting.sdGraphNormalPS;
                    case SDGraphRenderHelper.SDGraphRenderType.InPaint:
                        return m_Setting.sdGraphInPaintPS;
                    case SDGraphRenderHelper.SDGraphRenderType.WireFrame:
                        return m_Setting.sdGraphWireFramePS;
                }

                return null;
            }

            // Cleanup any allocated resources that were created during the execution of this render pass.
            public override void OnCameraCleanup(CommandBuffer cmd)
            {
            }
        }

        [Serializable]
        public class SDGraphRenderSetting
        {
            public Shader sdGraphColorPS;
            public Shader sdGraphDepthPS;
            public Shader sdGraphNormalPS;
            public Shader sdGraphInPaintPS;
            public Shader sdGraphWireFramePS;
        }

        SDGraphRenderPass m_ScriptablePass;
        public SDGraphRenderSetting setting;

        /// <inheritdoc/>
        public override void Create()
        {
            m_ScriptablePass = new SDGraphRenderPass();

            // Configures where the render pass should be injected.
            m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        }

        // Here you can inject one or multiple render passes in the renderer.
        // This method is called when setting up the renderer once per-camera.
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (m_ScriptablePass.Setup(setting))
            {
                renderer.EnqueuePass(m_ScriptablePass);
            }
        }
    }
}
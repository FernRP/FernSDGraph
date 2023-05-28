using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering.Universal;
using UnityEditor.Rendering.Universal.ShaderGraph;
using UnityEngine;
using UnityEditor.ShaderGraph;
using UnityEngine.UIElements;
using UnityEditor.ShaderGraph.Legacy;
using UnityEngine.Assertions;
using static UnityEditor.Rendering.Universal.ShaderGraph.SubShaderUtils;
using UnityEngine.Rendering.Universal;
using static Unity.Rendering.Universal.ShaderUtils;

namespace FernShaderGraph
{

    internal struct LitSubTargetParams
    {
        public DiffusionModel diffusionModel;
        public SpecularModel specularModel;
        public EnvReflectionMode envReflectionMode;
        public bool geometryAA;
        public bool depthNormal;
        public bool _2D;
        public bool envRotate;
        public bool screenSpaceRim;
    }
    
    sealed class FernSG_UniversalLitSubTarget : FernSG_UniversalSubTarget, ILegacyTarget
    {
        static readonly GUID kSourceCodeGuid = new GUID("d6c78107b64145745805d963de80cc17"); // FernSG_UniversalLitSubTarget.cs

        public override int latestVersion => 2;

        [SerializeField]
        NormalDropOffSpace m_NormalDropOffSpace = NormalDropOffSpace.Tangent;

        [SerializeField]
        bool m_ClearCoat = false; 
        
        [SerializeField]
        bool m_BlendModePreserveSpecular = true;

        public FernSG_UniversalLitSubTarget()
        {
            displayName = "Fern Standard";
        }

        protected override ShaderID shaderID => ShaderID.SG_Lit;

        public NormalDropOffSpace normalDropOffSpace
        {
            get => m_NormalDropOffSpace;
            set => m_NormalDropOffSpace = value;
        }
        
        [SerializeField] DiffusionModel m_DiffusionModel = DiffusionModel.Lambert;

        [SerializeField] SpecularModel m_SpecularModel = SpecularModel.GGX;

        [SerializeField] EnvReflectionMode m_EnvReflection = EnvReflectionMode.Default;

        [SerializeField] bool m_GeometryAA = false;

        [SerializeField] bool m_depthNormal = false;

        [SerializeField] private bool m_2D;
        
        [SerializeField] bool m_EnvRotate = false;
        
        
        public DiffusionModel diffusionModel
        {
            get => m_DiffusionModel;
            set => m_DiffusionModel = value;
        }

        public SpecularModel specularModel
        {
            get => m_SpecularModel;
            set => m_SpecularModel = value;
        }

        public EnvReflectionMode envReflectionMode
        {
            get => m_EnvReflection;
            set => m_EnvReflection = value;
        }

        public bool geometryAA
        {
            get => m_GeometryAA;
            set => m_GeometryAA = value;
        }

        public bool depthNormal
        {
            get => m_depthNormal;
            set => m_depthNormal = value;
        }

        public bool _2D
        {
            get => m_2D;
            set => m_2D = value;
        }

        public bool clearCoat
        {
            get => m_ClearCoat;
            set => m_ClearCoat = value;
        }
        
        public bool envRotate
        {
            get => m_EnvRotate;
            set => m_EnvRotate = value;
        }

        public bool blendModePreserveSpecular
        {
            get => m_BlendModePreserveSpecular;
            set => m_BlendModePreserveSpecular = value;
        }

        private LitSubTargetParams m_litSubTargetParams;

        [SerializeField] private bool fernControlFoldout = false;

        public override bool IsActive() => true;

        private TargetPropertyGUIFoldout foldoutFernControl;

        public override void Setup(ref TargetSetupContext context)
        {
            context.AddAssetDependency(kSourceCodeGuid, AssetCollection.Flags.SourceDependency);
            base.Setup(ref context);

            var universalRPType = typeof(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset);
            if (!context.HasCustomEditorForRenderPipeline(universalRPType))
            {
                var gui = typeof(ShaderGraphLitGUI);
#if HAS_VFX_GRAPH
                if (TargetsVFX())
                    gui = typeof(VFXShaderGraphLitGUI);
#endif
                context.AddCustomEditorForRenderPipeline(gui.FullName, universalRPType);
            }
            
            // setup subtargetparas
            m_litSubTargetParams.diffusionModel = diffusionModel;
            m_litSubTargetParams.specularModel = specularModel;
            m_litSubTargetParams.envReflectionMode = envReflectionMode;
            m_litSubTargetParams.geometryAA = geometryAA;
            m_litSubTargetParams.depthNormal = depthNormal;
            m_litSubTargetParams._2D = _2D;
            m_litSubTargetParams.envRotate = envRotate;

            // Process SubShaders
            context.AddSubShader(PostProcessSubShader(SubShaders.LitComputeDotsSubShader(target, m_litSubTargetParams, target.renderType, target.renderQueue, target.disableBatching, blendModePreserveSpecular)));
            context.AddSubShader(PostProcessSubShader(SubShaders.LitGLESSubShader(target, m_litSubTargetParams, target.renderType, target.renderQueue, target.disableBatching, blendModePreserveSpecular)));

            // if (foldoutFernControl != null)
            // {
            //     foldoutFernControl.UnregisterCallback<ChangeEvent<bool>>(UnregisterFoldout);
            //     foldoutFernControl.Clear();
            // }
            // else
            // {
            //     foldoutFernControl = new TargetPropertyGUIFoldout() { text = "Fern Control", value = fernControlFoldout, name = "foldout" };
            // }
            // foldoutFernControl.RegisterCallback<ChangeEvent<bool>>(UnregisterFoldout);
        }

        public override void ProcessPreviewMaterial(Material material)
        {
            if (target.allowMaterialOverride)
            {
                // copy our target's default settings into the material
                // (technically not necessary since we are always recreating the material from the shader each time,
                // which will pull over the defaults from the shader definition)
                // but if that ever changes, this will ensure the defaults are set
                material.SetFloat(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                material.SetFloat(Property.ReceiveShadows, target.receiveShadows ? 1.0f : 0.0f);
                material.SetFloat(Property.SurfaceType, (float)target.surfaceType);
                material.SetFloat(Property.BlendMode, (float)target.alphaMode);
                material.SetFloat(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                material.SetFloat(Property.CullMode, (int)target.renderFace);
                material.SetFloat(Property.ZWriteControl, (float)target.zWriteControl);
                material.SetFloat(Property.ZTest, (float)target.zTestMode);
            }

            // We always need these properties regardless of whether the material is allowed to override
            // Queue control & offset enable correct automatic render queue behavior
            // Control == 0 is automatic, 1 is user-specified render queue
            material.SetFloat(Property.QueueOffset, 0.0f);
            material.SetFloat(Property.QueueControl, (float)BaseShaderGUI.QueueControl.Auto);

            // call the full unlit material setup function
            ShaderGraphLitGUI.UpdateMaterial(material, MaterialUpdateType.CreatedNewMaterial);
        }

        public override void GetFields(ref TargetFieldContext context)
        {
            base.GetFields(ref context);

            var descs = context.blocks.Select(x => x.descriptor);

            // Lit -- always controlled by subtarget
            context.AddField(UniversalFields.NormalDropOffOS, normalDropOffSpace == NormalDropOffSpace.Object);
            context.AddField(UniversalFields.NormalDropOffTS, normalDropOffSpace == NormalDropOffSpace.Tangent);
            context.AddField(UniversalFields.NormalDropOffWS, normalDropOffSpace == NormalDropOffSpace.World);
            context.AddField(UniversalFields.Normal, descs.Contains(BlockFields.SurfaceDescription.NormalOS) ||
                descs.Contains(BlockFields.SurfaceDescription.NormalTS) ||
                descs.Contains(BlockFields.SurfaceDescription.NormalWS));
            // Complex Lit

            // Template Predicates
            //context.AddField(UniversalFields.PredicateClearCoat, clearCoat);
        }

        public override void GetActiveBlocks(ref TargetActiveBlockContext context)
        {
            context.AddBlock(BlockFields.SurfaceDescription.Smoothness);
            context.AddBlock(BlockFields.SurfaceDescription.NormalOS, normalDropOffSpace == NormalDropOffSpace.Object);
            context.AddBlock(BlockFields.SurfaceDescription.NormalTS, normalDropOffSpace == NormalDropOffSpace.Tangent);
            context.AddBlock(BlockFields.SurfaceDescription.NormalWS, normalDropOffSpace == NormalDropOffSpace.World);
            context.AddBlock(BlockFields.SurfaceDescription.Emission);
            context.AddBlock(BlockFields.SurfaceDescription.Occlusion);

            // when the surface options are material controlled, we must show all of these blocks
            // when target controlled, we can cull the unnecessary blocks
            context.AddBlock(BlockFields.SurfaceDescription.Metallic);
            context.AddBlock(BlockFields.SurfaceDescription.Alpha, (target.surfaceType == SurfaceType.Transparent || target.alphaClip) || target.allowMaterialOverride);
            context.AddBlock(BlockFields.SurfaceDescription.AlphaClipThreshold, (target.alphaClip) || target.allowMaterialOverride);

            // always controlled by subtarget clearCoat checkbox (no Material control)
            context.AddBlock(BlockFields.SurfaceDescription.CoatMask, clearCoat);
            context.AddBlock(BlockFields.SurfaceDescription.CoatSmoothness, clearCoat);
            
            context.AddBlock(FernSG_Field.SurfaceDescription.CellThreshold, diffusionModel == DiffusionModel.Cell);
            context.AddBlock(FernSG_Field.SurfaceDescription.CellSmoothness, diffusionModel == DiffusionModel.Cell);
            context.AddBlock(FernSG_Field.SurfaceDescription.RampColor, diffusionModel == DiffusionModel.Ramp);
            context.AddBlock(FernSG_Field.SurfaceDescription.SpecularColor);
            context.AddBlock(FernSG_Field.SurfaceDescription.StylizedSpecularSize, specularModel == SpecularModel.STYLIZED);
            context.AddBlock(FernSG_Field.SurfaceDescription.StylizedSpecularSoftness, specularModel == SpecularModel.STYLIZED);
            
            context.AddBlock(FernSG_Field.SurfaceDescription.GeometryAAStrength, geometryAA);
            context.AddBlock(FernSG_Field.SurfaceDescription.GeometryAAVariant, geometryAA); 
            
            context.AddBlock(FernSG_Field.SurfaceDescription.LightenColor, diffusionModel != DiffusionModel.Ramp);
            context.AddBlock(FernSG_Field.SurfaceDescription.DarkColor, diffusionModel != DiffusionModel.Ramp);
            
            context.AddBlock(FernSG_Field.SurfaceDescription.EnvReflection, envReflectionMode == EnvReflectionMode.Custom);
            context.AddBlock(FernSG_Field.SurfaceDescription.EnvRotate, envRotate && envReflectionMode == default);
            context.AddBlock(FernSG_Field.SurfaceDescription.EnvSpeularcIntensity, envReflectionMode == default || envReflectionMode == EnvReflectionMode.Custom);
        }

        public override void CollectShaderProperties(PropertyCollector collector, GenerationMode generationMode)
        {
            // if using material control, add the material property to control workflow mode
            if (target.allowMaterialOverride)
            {
                collector.AddFloatProperty(Property.CastShadows, target.castShadows ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.ReceiveShadows, target.receiveShadows ? 1.0f : 0.0f);

                // setup properties using the defaults
                collector.AddFloatProperty(Property.SurfaceType, (float)target.surfaceType);
                collector.AddFloatProperty(Property.BlendMode, (float)target.alphaMode);
                collector.AddFloatProperty(Property.AlphaClip, target.alphaClip ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.BlendModePreserveSpecular, blendModePreserveSpecular ? 1.0f : 0.0f);
                collector.AddFloatProperty(Property.SrcBlend, 1.0f);    // always set by material inspector, ok to have incorrect values here
                collector.AddFloatProperty(Property.DstBlend, 0.0f);    // always set by material inspector, ok to have incorrect values here
                collector.AddToggleProperty(Property.ZWrite, (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.ZWriteControl, (float)target.zWriteControl);
                collector.AddFloatProperty(Property.ZTest, (float)target.zTestMode);    // ztest mode is designed to directly pass as ztest
                collector.AddFloatProperty(Property.CullMode, (float)target.renderFace);    // render face enum is designed to directly pass as a cull mode

                bool enableAlphaToMask = (target.alphaClip && (target.surfaceType == SurfaceType.Opaque));
                collector.AddFloatProperty(Property.AlphaToMask, enableAlphaToMask ? 1.0f : 0.0f);
            }

            // We always need these properties regardless of whether the material is allowed to override other shader properties.
            // Queue control & offset enable correct automatic render queue behavior.  Control == 0 is automatic, 1 is user-specified.
            // We initialize queue control to -1 to indicate to UpdateMaterial that it needs to initialize it properly on the material.
            collector.AddFloatProperty(Property.QueueOffset, 0.0f);
            collector.AddFloatProperty(Property.QueueControl, -1.0f);
        }

        private Color fernFoldoutColor = new Color(0.55f, 0.6f, 1f);
        public override void GetPropertiesGUI(ref TargetPropertyGUIContext context, Action onChange, Action<String> registerUndo)
        {
            var universalTarget = (target as FernSG_UniversalTarget);
            universalTarget.AddDefaultMaterialOverrideGUI(ref context, onChange, registerUndo);

            context.AddProperty("Fragment Normal Space", new EnumField(NormalDropOffSpace.Tangent) { value = normalDropOffSpace }, (evt) =>
            {
                if (Equals(normalDropOffSpace, evt.newValue))
                    return;

                registerUndo("Change Fragment Normal Space");
                normalDropOffSpace = (NormalDropOffSpace)evt.newValue;
                onChange();
            });
            
            if (target.surfaceType == SurfaceType.Transparent)
            {
                if (target.alphaMode == AlphaMode.Alpha || target.alphaMode == AlphaMode.Additive)
                    context.AddProperty("Preserve Specular Lighting", new Toggle() { value = blendModePreserveSpecular }, (evt) =>
                    {
                        if (Equals(blendModePreserveSpecular, evt.newValue))
                            return;

                        registerUndo("Change Preserve Specular");
                        blendModePreserveSpecular = evt.newValue;
                        onChange();
                    });
            }
            
            universalTarget.AddDefaultSurfacePropertiesGUI(ref context, onChange, registerUndo, showReceiveShadows: true);
            
            
            foldoutFernControl = new TargetPropertyGUIFoldout() { text = "Fern Control", value = fernControlFoldout, style = { color = fernFoldoutColor}, name = "Fern foldout" };
            foldoutFernControl.RegisterCallback<ChangeEvent<bool>>(evt =>
            {
                fernControlFoldout = !fernControlFoldout;
                onChange();
            });
        
            context.Add(foldoutFernControl);

            if (fernControlFoldout)
            {
                context.AddProperty("Depth Normal", 1,new Toggle() { value = depthNormal }, (evt) =>
                {
                    if (Equals(depthNormal, evt.newValue))
                        return;

                    registerUndo("Change Depth Normal");
                    depthNormal = evt.newValue;
                    onChange();
                });

                context.AddProperty("Universal 2D", 1,new Toggle() { value = _2D }, (evt) =>
                {
                    if (Equals(_2D, evt.newValue))
                        return;

                    registerUndo("Change Universal 2D");
                    _2D = evt.newValue;
                    onChange();
                });
                            
                context.AddProperty("Geometry AA", 1, new Toggle() { value = geometryAA }, (evt) =>
                {
                    if (Equals(geometryAA, evt.newValue))
                        return;

                    registerUndo("Change Geometry AA");
                    geometryAA = evt.newValue;
                    onChange();
                });

                context.AddProperty("Clear Coat", 1, new Toggle() { value = clearCoat }, (evt) =>
                {
                    if (Equals(clearCoat, evt.newValue))
                        return;

                    registerUndo("Change Clear Coat");
                    clearCoat = evt.newValue;
                    onChange();
                });
                 
                context.AddProperty("Diffusion Model",1,  new EnumField(DiffusionModel.Lambert) { value = diffusionModel }, (evt) =>
                {
                    if (Equals(diffusionModel, evt.newValue))
                        return;

                    registerUndo("Change Diffusion Model");
                    diffusionModel = (DiffusionModel)evt.newValue;
                    onChange();
                });
                
                context.AddProperty("Specular Model",1,  new EnumField(SpecularModel.GGX) { value = specularModel }, (evt) =>
                {
                    if (Equals(specularModel, evt.newValue))
                        return;

                    registerUndo("Change Specular Model");
                    specularModel = (SpecularModel)evt.newValue;
                    onChange();
                });
                
                context.AddProperty("Env Reflection Mode",1,  new EnumField(EnvReflectionMode.Default) { value = envReflectionMode }, (evt) =>
                {
                    if (Equals(envReflectionMode, evt.newValue))
                        return;

                    registerUndo("Change Env Reflection Mode");
                    envReflectionMode = (EnvReflectionMode)evt.newValue;
                    onChange();
                });
                
                context.AddProperty("Env Rotate",1,  new Toggle(){ value = envRotate}, (evt) =>
                {
                    if (Equals(envRotate, evt.newValue))
                        return;

                    registerUndo("Change Env Rotate");
                    envRotate = evt.newValue;
                    onChange();
                });
            }

        }

        protected override int ComputeMaterialNeedsUpdateHash()
        {
            int hash = base.ComputeMaterialNeedsUpdateHash();
            hash = hash * 23 + target.allowMaterialOverride.GetHashCode();
            return hash;
        }

        public bool TryUpgradeFromMasterNode(IMasterNode1 masterNode, out Dictionary<BlockFieldDescriptor, int> blockMap)
        {
            blockMap = null;
            if (!(masterNode is PBRMasterNode1 pbrMasterNode))
                return false;

            m_NormalDropOffSpace = (NormalDropOffSpace)pbrMasterNode.m_NormalDropOffSpace;

            // Handle mapping of Normal block specifically
            BlockFieldDescriptor normalBlock;
            switch (m_NormalDropOffSpace)
            {
                case NormalDropOffSpace.Object:
                    normalBlock = BlockFields.SurfaceDescription.NormalOS;
                    break;
                case NormalDropOffSpace.World:
                    normalBlock = BlockFields.SurfaceDescription.NormalWS;
                    break;
                default:
                    normalBlock = BlockFields.SurfaceDescription.NormalTS;
                    break;
            }

            // Set blockmap
            blockMap = new Dictionary<BlockFieldDescriptor, int>()
            {
                { BlockFields.VertexDescription.Position, 9 },
                { BlockFields.VertexDescription.Normal, 10 },
                { BlockFields.VertexDescription.Tangent, 11 },
                { BlockFields.SurfaceDescription.BaseColor, 0 },
                { normalBlock, 1 },
                { BlockFields.SurfaceDescription.Emission, 4 },
                { BlockFields.SurfaceDescription.Smoothness, 5 },
                { BlockFields.SurfaceDescription.Occlusion, 6 },
                { BlockFields.SurfaceDescription.Alpha, 7 },
                { BlockFields.SurfaceDescription.AlphaClipThreshold, 8 },
                { FernSG_Field.SurfaceDescription.DarkColor, 12 },
                { FernSG_Field.SurfaceDescription.LightenColor, 13 },
            };

            blockMap.Add(BlockFields.SurfaceDescription.Metallic, 2);

            return true;
        }

        internal override void OnAfterParentTargetDeserialized()
        {
            Assert.IsNotNull(target);

            if (this.sgVersion < latestVersion)
            {
                // Upgrade old incorrect Premultiplied blend into
                // equivalent Alpha + Preserve Specular blend mode.
                if (this.sgVersion < 1)
                {
                    if (target.alphaMode == AlphaMode.Premultiply)
                    {
                        target.alphaMode = AlphaMode.Alpha;
                        blendModePreserveSpecular = true;
                    }
                    else
                        blendModePreserveSpecular = false;
                }
                ChangeVersion(latestVersion);
            }
        }

        #region SubShader
        static class SubShaders
        {
            // SM 4.5, compute with dots instancing
            public static SubShaderDescriptor LitComputeDotsSubShader(FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams, string renderType, string renderQueue, string disableBatchingTag, bool blendModePreserveSpecular)
            {
                SubShaderDescriptor result = new SubShaderDescriptor()
                {
                    pipelineTag = FernSG_UniversalTarget.kPipelineTag,
                    customTags = FernSG_UniversalTarget.kLitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    disableBatchingTag = disableBatchingTag,
                    generatesPreview = true,
                    passes = new PassCollection()
                };

               
                result.passes.Add(LitPasses.Forward(target, litSubTargetParams, blendModePreserveSpecular, CorePragmas.ForwardSM45, LitKeywords.DOTSForward));

                // if (!complexLit)
                //     result.passes.Add(LitPasses.GBuffer(target, blendModePreserveSpecular));

                // cull the shadowcaster pass if we know it will never be used
                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(PassVariant(CorePasses.ShadowCaster(target), CorePragmas.InstancedSM45));

                if (target.mayWriteDepth)
                    result.passes.Add(PassVariant(CorePasses.DepthOnly(target), CorePragmas.InstancedSM45));

                if (litSubTargetParams.depthNormal)
                    result.passes.Add(PassVariant(LitPasses.DepthNormal(target), CorePragmas.InstancedSM45));
               
                result.passes.Add(PassVariant(LitPasses.Meta(target), CorePragmas.DefaultSM45));
                // Currently neither of these passes (selection/picking) can be last for the game view for
                // UI shaders to render correctly. Verify [1352225] before changing this order.
                result.passes.Add(PassVariant(CorePasses.SceneSelection(target), CorePragmas.DefaultSM45));
                result.passes.Add(PassVariant(CorePasses.ScenePicking(target), CorePragmas.DefaultSM45));

                if(litSubTargetParams._2D)
                    result.passes.Add(PassVariant(LitPasses._2D(target), CorePragmas.DefaultSM45));

                return result;
            }

            public static SubShaderDescriptor LitGLESSubShader(FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams, string renderType, string renderQueue, string disableBatchingTag, bool blendModePreserveSpecular)
            {
                // SM 2.0, GLES

                // ForwardOnly pass is used as complex Lit SM 2.0 fallback for GLES.
                // Drops advanced features and renders materials as Lit.

                SubShaderDescriptor result = new SubShaderDescriptor()
                {
                    pipelineTag = FernSG_UniversalTarget.kPipelineTag,
                    customTags = FernSG_UniversalTarget.kLitMaterialTypeTag,
                    renderType = renderType,
                    renderQueue = renderQueue,
                    disableBatchingTag = disableBatchingTag,
                    generatesPreview = true,
                    passes = new PassCollection()
                };

                result.passes.Add(LitPasses.Forward(target, litSubTargetParams, blendModePreserveSpecular, CorePragmas.Forward, LitKeywords.FernForward));

                // cull the shadowcaster pass if we know it will never be used
                if (target.castShadows || target.allowMaterialOverride)
                    result.passes.Add(CorePasses.ShadowCaster(target));

                if (target.mayWriteDepth)
                    result.passes.Add(CorePasses.DepthOnly(target));

                if(litSubTargetParams.depthNormal)
                    result.passes.Add(CorePasses.DepthNormal(target));
                result.passes.Add(LitPasses.Meta(target));
                // Currently neither of these passes (selection/picking) can be last for the game view for
                // UI shaders to render correctly. Verify [1352225] before changing this order.
                result.passes.Add(CorePasses.SceneSelection(target));
                result.passes.Add(CorePasses.ScenePicking(target));

                if(litSubTargetParams._2D)
                    result.passes.Add(LitPasses._2D(target));

                return result;
            }
        }
        #endregion

        #region Passes
        static class LitPasses
        {
            static void AddReceiveShadowsControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, bool receiveShadows)
            {
                if (target.allowMaterialOverride)
                    pass.keywords.Add(LitKeywords.ReceiveShadowsOff);
                else if (!receiveShadows)
                    pass.defines.Add(LitKeywords.ReceiveShadowsOff, 1);
            }
            
            internal static void AddEnvRotateControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
            {
                if (litSubTargetParams.envRotate)
                {
                    pass.defines.Add(CoreKeywordDescriptors.UseEnvRotate, 1);
                }
            }
            
             internal static void AddDiffusionModelControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
        {
            switch (litSubTargetParams.diffusionModel)
            {
                case DiffusionModel.Lambert:
                    pass.defines.Add(CoreKeywordDescriptors.DiffuseModel, 0);
                    break;
                case DiffusionModel.Cell:
                    pass.defines.Add(CoreKeywordDescriptors.DiffuseModel, 1);

                    break;
                case DiffusionModel.Ramp:
                    pass.defines.Add(CoreKeywordDescriptors.DiffuseModel, 2);
                    break;
            }
        }

        

        internal static void AddEnvReflectionModeControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
        {
            switch (litSubTargetParams.envReflectionMode)
            {
                case EnvReflectionMode.Default:
                    pass.defines.Add(CoreKeywordDescriptors.EnvReflectionMode, 0);
                    break;
                case EnvReflectionMode.Custom:
                    pass.defines.Add(CoreKeywordDescriptors.EnvReflectionMode, 1);
                    break;
            }
        }

        internal static void AddSpecularModelControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
        {
            switch (litSubTargetParams.specularModel)
            {
                case SpecularModel.GGX:
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 0);
                    break;
                case SpecularModel.STYLIZED:
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 1);

                    break;
                case SpecularModel.BLINNPHONG
                    :
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 2);
                    break;
            }
        }

        internal static void AddEnvReflectionModelControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
        {
            switch (litSubTargetParams.specularModel)
            {
                case SpecularModel.GGX:
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 0);
                    break;
                case SpecularModel.STYLIZED:
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 1);

                    break;
                case SpecularModel.BLINNPHONG
                    :
                    pass.defines.Add(CoreKeywordDescriptors.SpecularModel, 2);
                    break;
            }
        }

        internal static void AddGeometryAAControlToPass(ref PassDescriptor pass, FernSG_UniversalTarget target, LitSubTargetParams litSubTargetParams)
        {
            if (litSubTargetParams.geometryAA)
            {
                pass.defines.Add(CoreKeywordDescriptors.UseGeometryAA, 1);
            }
        }


            public static PassDescriptor Forward(
                FernSG_UniversalTarget target,
                LitSubTargetParams litSubTargetParams,
                bool blendModePreserveSpecular,
                PragmaCollection pragmas,
                KeywordCollection keywords)
            {
                KeywordCollection addForward = new KeywordCollection
                {
                    keywords
                };

                if (target.ScreenSpaceAmbientOcclusion)
                {
                    addForward.Add(CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion);
                }
                if (target.StaticLightmap)
                {
                    addForward.Add(CoreKeywordDescriptors.StaticLightmap);
                }                
                if (target.DynamicLightmap)
                {
                    addForward.Add(CoreKeywordDescriptors.DynamicLightmap);
                }                
                if (target.DirectionalLightmapCombined)
                {
                    addForward.Add(CoreKeywordDescriptors.DirectionalLightmapCombined);
                }       
                if (target.AdditionalLights)
                {
                    addForward.Add(CoreKeywordDescriptors.AdditionalLights);
                }   
                if (target.AdditionalLightShadows)
                {
                    addForward.Add(CoreKeywordDescriptors.AdditionalLightShadows);
                }                
                if (target.ReflectionProbeBlending)
                {
                    addForward.Add(CoreKeywordDescriptors.ReflectionProbeBlending);
                }                
                if (target.ReflectionProbeBoxProjection)
                {
                    addForward.Add(CoreKeywordDescriptors.ReflectionProbeBoxProjection);
                }                
                if (target.LightmapShadowMixing)
                {
                    addForward.Add(CoreKeywordDescriptors.LightmapShadowMixing);
                }                
                if (target.DBuffer)
                {
                    addForward.Add(CoreKeywordDescriptors.DBuffer);
                }                
                if (target.LightLayers)
                {
                    addForward.Add(CoreKeywordDescriptors.LightLayers);
                }                
                if (target.DebugDisplay)
                {
                    addForward.Add(CoreKeywordDescriptors.DebugDisplay);
                }                
                if (target.LightCookies)
                {
                    addForward.Add(CoreKeywordDescriptors.LightCookies);
                }
                if (target.ForwardPlus)
                {
                    addForward.Add(CoreKeywordDescriptors.ForwardPlus);
                }
                
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "Universal Forward",
                    referenceName = "SHADERPASS_FORWARD",
                    lightMode = "UniversalForward",
                    useInPreview = true,

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = LitBlockMasks.FragmentLit,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = LitRequiredFields.Forward,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target, blendModePreserveSpecular),
                    pragmas = pragmas ?? CorePragmas.Forward,     // NOTE: SM 2.0 only GL
                    defines = new DefineCollection() { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection() { addForward },
                    includes = LitIncludes.Forward,

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target, blendModePreserveSpecular);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);
                AddReceiveShadowsControlToPass(ref result, target, target.receiveShadows);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);
                AddDiffusionModelControlToPass(ref result, target, litSubTargetParams);
                AddEnvRotateControlToPass(ref result, target, litSubTargetParams);
                AddEnvReflectionModeControlToPass(ref result, target, litSubTargetParams);
                AddSpecularModelControlToPass(ref result, target, litSubTargetParams);
                AddGeometryAAControlToPass(ref result, target, litSubTargetParams);
                
                return result;
            }

            public static PassDescriptor ForwardOnly(
                FernSG_UniversalTarget target,
                bool complexLit,
                bool blendModePreserveSpecular,
                BlockFieldDescriptor[] vertexBlocks,
                BlockFieldDescriptor[] pixelBlocks,
                PragmaCollection pragmas,
                KeywordCollection keywords)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "Universal Forward Only",
                    referenceName = "SHADERPASS_FORWARDONLY",
                    lightMode = "UniversalForwardOnly",
                    useInPreview = true,

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = vertexBlocks,
                    validPixelBlocks = pixelBlocks,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = LitRequiredFields.Forward,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target, blendModePreserveSpecular),
                    pragmas = pragmas,
                    defines = new DefineCollection { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection { keywords },
                    includes = new IncludeCollection { LitIncludes.Forward },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                if (complexLit)
                    result.defines.Add(LitDefines.ClearCoat, 1);

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target, blendModePreserveSpecular);
                CorePasses.AddAlphaToMaskControlToPass(ref result, target);
                AddReceiveShadowsControlToPass(ref result, target, target.receiveShadows);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            // Deferred only in SM4.5, MRT not supported in GLES2
            public static PassDescriptor GBuffer(FernSG_UniversalTarget target, bool blendModePreserveSpecular)
            {
                var result = new PassDescriptor
                {
                    // Definition
                    displayName = "GBuffer",
                    referenceName = "SHADERPASS_GBUFFER",
                    lightMode = "UniversalGBuffer",
                    useInPreview = true,

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = LitBlockMasks.FragmentLit,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = LitRequiredFields.GBuffer,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target, blendModePreserveSpecular),
                    pragmas = CorePragmas.GBufferSM45,
                    defines = new DefineCollection() { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection() { LitKeywords.GBuffer },
                    includes = new IncludeCollection { LitIncludes.GBuffer },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddTargetSurfaceControlsToPass(ref result, target, blendModePreserveSpecular);
                AddReceiveShadowsControlToPass(ref result, target, target.receiveShadows);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor Meta(FernSG_UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "Meta",
                    referenceName = "SHADERPASS_META",
                    lightMode = "Meta",

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = LitBlockMasks.FragmentMeta,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = LitRequiredFields.Meta,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.Meta,
                    pragmas = CorePragmas.Default,
                    defines = new DefineCollection() { CoreDefines.UseFragmentFog },
                    keywords = new KeywordCollection() { CoreKeywordDescriptors.EditorVisualization },
                    includes = LitIncludes.Meta,

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddAlphaClipControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor _2D(FernSG_UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    referenceName = "SHADERPASS_2D",
                    lightMode = "Universal2D",

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentColorAlpha,

                    // Fields
                    structs = CoreStructCollections.Default,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.UberSwitchedRenderState(target),
                    pragmas = CorePragmas.Instanced,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection(),
                    includes = LitIncludes._2D,

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddAlphaClipControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor DepthNormal(FernSG_UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "DepthNormals",
                    referenceName = "SHADERPASS_DEPTHNORMALS",
                    lightMode = "DepthNormals",
                    useInPreview = false,

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentDepthNormals,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = CoreRequiredFields.DepthNormals,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.DepthNormalsOnly(target),
                    pragmas = CorePragmas.Instanced,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection { CoreKeywords.DOTSDepthNormal },
                    includes = new IncludeCollection { CoreIncludes.DepthNormalsOnly },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddAlphaClipControlToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }

            public static PassDescriptor DepthNormalOnly(FernSG_UniversalTarget target)
            {
                var result = new PassDescriptor()
                {
                    // Definition
                    displayName = "DepthNormalsOnly",
                    referenceName = "SHADERPASS_DEPTHNORMALSONLY",
                    lightMode = "DepthNormalsOnly",
                    useInPreview = false,

                    // Template
                    passTemplatePath = FernSG_UniversalTarget.kUberTemplatePath,
                    sharedTemplateDirectories = FernSG_UniversalTarget.kSharedTemplateDirectories,

                    // Port Mask
                    validVertexBlocks = CoreBlockMasks.Vertex,
                    validPixelBlocks = CoreBlockMasks.FragmentDepthNormals,

                    // Fields
                    structs = CoreStructCollections.Default,
                    requiredFields = CoreRequiredFields.DepthNormals,
                    fieldDependencies = CoreFieldDependencies.Default,

                    // Conditional State
                    renderStates = CoreRenderStates.DepthNormalsOnly(target),
                    pragmas = CorePragmas.Instanced,
                    defines = new DefineCollection(),
                    keywords = new KeywordCollection() { CoreKeywords.DOTSDepthNormal },
                    includes = new IncludeCollection { CoreIncludes.DepthNormalsOnly },

                    // Custom Interpolator Support
                    customInterpolators = CoreCustomInterpDescriptors.Common
                };

                CorePasses.AddAlphaClipControlToPass(ref result, target);
                CorePasses.AddLODCrossFadeControlToPass(ref result, target);

                return result;
            }
        }
        #endregion

        #region PortMasks
        static class LitBlockMasks
        {
            public static readonly BlockFieldDescriptor[] FragmentLit = new BlockFieldDescriptor[]
            {
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.NormalOS,
                BlockFields.SurfaceDescription.NormalTS,
                BlockFields.SurfaceDescription.NormalWS,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Metallic,
                BlockFields.SurfaceDescription.Specular,
                BlockFields.SurfaceDescription.Smoothness,
                BlockFields.SurfaceDescription.Occlusion,
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.AlphaClipThreshold,
                BlockFields.SurfaceDescription.CoatMask,
                BlockFields.SurfaceDescription.CoatSmoothness,
                FernSG_Field.SurfaceDescription.RampColor,
                FernSG_Field.SurfaceDescription.SpecularColor,
                FernSG_Field.SurfaceDescription.StylizedSpecularSize,
                FernSG_Field.SurfaceDescription.StylizedSpecularSoftness,
                FernSG_Field.SurfaceDescription.CellThreshold,
                FernSG_Field.SurfaceDescription.CellSmoothness,
                FernSG_Field.SurfaceDescription.GeometryAAVariant,
                FernSG_Field.SurfaceDescription.GeometryAAStrength,
                FernSG_Field.SurfaceDescription.DarkColor,
                FernSG_Field.SurfaceDescription.LightenColor,
                FernSG_Field.SurfaceDescription.EnvReflection,
                FernSG_Field.SurfaceDescription.EnvRotate,
                FernSG_Field.SurfaceDescription.EnvSpeularcIntensity,
            };

            public static readonly BlockFieldDescriptor[] FragmentMeta = new BlockFieldDescriptor[]
            {
                BlockFields.SurfaceDescription.BaseColor,
                BlockFields.SurfaceDescription.Emission,
                BlockFields.SurfaceDescription.Alpha,
                BlockFields.SurfaceDescription.AlphaClipThreshold,
            };
        }
        #endregion

        #region RequiredFields
        static class LitRequiredFields
        {
            public static readonly FieldCollection Forward = new FieldCollection()
            {
                StructFields.Attributes.uv1,
                StructFields.Attributes.uv2,
                StructFields.Varyings.positionWS,
                StructFields.Varyings.normalWS,
                StructFields.Varyings.tangentWS,                        // needed for vertex lighting
                UniversalStructFields.Varyings.staticLightmapUV,
                UniversalStructFields.Varyings.dynamicLightmapUV,
                UniversalStructFields.Varyings.sh,
                UniversalStructFields.Varyings.fogFactorAndVertexLight, // fog and vertex lighting, vert input is dependency
                UniversalStructFields.Varyings.shadowCoord,             // shadow coord, vert input is dependency
            };

            public static readonly FieldCollection GBuffer = new FieldCollection()
            {
                StructFields.Attributes.uv1,
                StructFields.Attributes.uv2,
                StructFields.Varyings.positionWS,
                StructFields.Varyings.normalWS,
                StructFields.Varyings.tangentWS,                        // needed for vertex lighting
                UniversalStructFields.Varyings.staticLightmapUV,
                UniversalStructFields.Varyings.dynamicLightmapUV,
                UniversalStructFields.Varyings.sh,
                UniversalStructFields.Varyings.fogFactorAndVertexLight, // fog and vertex lighting, vert input is dependency
                UniversalStructFields.Varyings.shadowCoord,             // shadow coord, vert input is dependency
            };

            public static readonly FieldCollection Meta = new FieldCollection()
            {
                StructFields.Attributes.positionOS,
                StructFields.Attributes.normalOS,
                StructFields.Attributes.uv0,                            //
                StructFields.Attributes.uv1,                            // needed for meta vertex position
                StructFields.Attributes.uv2,                            // needed for meta UVs
                StructFields.Attributes.instanceID,                     // needed for rendering instanced terrain
                StructFields.Varyings.positionCS,
                StructFields.Varyings.texCoord0,                        // needed for meta UVs
                StructFields.Varyings.texCoord1,                        // VizUV
                StructFields.Varyings.texCoord2,                        // LightCoord
            };
        }
        #endregion

        #region Defines
        static class LitDefines
        {
            public static readonly KeywordDescriptor ClearCoat = new KeywordDescriptor()
            {
                displayName = "Clear Coat",
                referenceName = "_CLEARCOAT",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.ShaderFeature,
                scope = KeywordScope.Local,
                stages = KeywordShaderStage.Fragment
            };

            public static readonly KeywordDescriptor SpecularSetup = new KeywordDescriptor()
            {
                displayName = "Specular Setup",
                referenceName = "_SPECULAR_SETUP",
                type = KeywordType.Boolean,
                definition = KeywordDefinition.ShaderFeature,
                scope = KeywordScope.Local,
                stages = KeywordShaderStage.Fragment
            };
        }
        #endregion

        #region Keywords
        static class LitKeywords
        {
            public static readonly KeywordDescriptor ReceiveShadowsOff = new KeywordDescriptor()
            {
                displayName = "Receive Shadows Off",
                referenceName = ShaderKeywordStrings._RECEIVE_SHADOWS_OFF,
                type = KeywordType.Boolean,
                definition = KeywordDefinition.ShaderFeature,
                scope = KeywordScope.Local,
            };

            public static readonly KeywordCollection Forward = new KeywordCollection
            {
                { CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion },
                { CoreKeywordDescriptors.StaticLightmap },
                { CoreKeywordDescriptors.DynamicLightmap },
                { CoreKeywordDescriptors.DirectionalLightmapCombined },
                { CoreKeywordDescriptors.MainLightShadows },
                { CoreKeywordDescriptors.AdditionalLights },
                { CoreKeywordDescriptors.AdditionalLightShadows },
                { CoreKeywordDescriptors.ReflectionProbeBlending },
                { CoreKeywordDescriptors.ReflectionProbeBoxProjection },
                { CoreKeywordDescriptors.ShadowsSoft },
                { CoreKeywordDescriptors.LightmapShadowMixing },
                { CoreKeywordDescriptors.ShadowsShadowmask },
                { CoreKeywordDescriptors.DBuffer },
                { CoreKeywordDescriptors.LightLayers },
                { CoreKeywordDescriptors.DebugDisplay },
                { CoreKeywordDescriptors.LightCookies },
                { CoreKeywordDescriptors.ForwardPlus },
            };
            
            public static readonly KeywordCollection FernForward = new KeywordCollection
            {
                //{ CoreKeywordDescriptors.ScreenSpaceAmbientOcclusion },
                //{ CoreKeywordDescriptors.StaticLightmap },
                //{ CoreKeywordDescriptors.DynamicLightmap },
                //{ CoreKeywordDescriptors.DirectionalLightmapCombined },
                { CoreKeywordDescriptors.MainLightShadows },
                //{ CoreKeywordDescriptors.AdditionalLights },
                //{ CoreKeywordDescriptors.AdditionalLightShadows },
                //{ CoreKeywordDescriptors.ReflectionProbeBlending },
                //{ CoreKeywordDescriptors.ReflectionProbeBoxProjection },
                { CoreKeywordDescriptors.ShadowsSoft },
                //{ CoreKeywordDescriptors.LightmapShadowMixing },
                //{ CoreKeywordDescriptors.ShadowsShadowmask },
                //{ CoreKeywordDescriptors.DBuffer },
               // { CoreKeywordDescriptors.LightLayers },
                //{ CoreKeywordDescriptors.DebugDisplay },
                //{ CoreKeywordDescriptors.LightCookies },
                //{ CoreKeywordDescriptors.ForwardPlus },
            };
            
            public static readonly KeywordCollection DOTSForward = new KeywordCollection
            {
                { FernForward },
                { CoreKeywordDescriptors.WriteRenderingLayers },
            };

            public static readonly KeywordCollection GBuffer = new KeywordCollection
            {
                { CoreKeywordDescriptors.StaticLightmap },
                { CoreKeywordDescriptors.DynamicLightmap },
                { CoreKeywordDescriptors.DirectionalLightmapCombined },
                { CoreKeywordDescriptors.MainLightShadows },
                { CoreKeywordDescriptors.ReflectionProbeBlending },
                { CoreKeywordDescriptors.ReflectionProbeBoxProjection },
                { CoreKeywordDescriptors.ShadowsSoft },
                { CoreKeywordDescriptors.LightmapShadowMixing },
                { CoreKeywordDescriptors.ShadowsShadowmask },
                { CoreKeywordDescriptors.MixedLightingSubtractive },
                { CoreKeywordDescriptors.DBuffer },
                { CoreKeywordDescriptors.GBufferNormalsOct },
                { CoreKeywordDescriptors.WriteRenderingLayers },
                { CoreKeywordDescriptors.RenderPassEnabled },
                { CoreKeywordDescriptors.DebugDisplay },
            };
        }
        #endregion

        #region Includes
        static class LitIncludes
        {
            const string kShadows = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl";
            const string kMetaInput = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/MetaInput.hlsl";
            //const string kForwardPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRForwardPass.hlsl";
            const string kGBuffer = "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl";
            const string kPBRGBufferPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBRGBufferPass.hlsl";
            const string kLightingMetaPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/LightingMetaPass.hlsl";
            const string k2DPass = "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/PBR2DPass.hlsl";

            public static readonly IncludeCollection Forward = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.CorePregraph },
                { kShadows, IncludeLocation.Pregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { CoreIncludes.DBufferPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { FERNSG_Resources.KForwardPass, IncludeLocation.Postgraph },
            };

            public static readonly IncludeCollection GBuffer = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.CorePregraph },
                { kShadows, IncludeLocation.Pregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { CoreIncludes.DBufferPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kGBuffer, IncludeLocation.Postgraph },
                { kPBRGBufferPass, IncludeLocation.Postgraph },
            };

            public static readonly IncludeCollection Meta = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },
                { kMetaInput, IncludeLocation.Pregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { kLightingMetaPass, IncludeLocation.Postgraph },
            };

            public static readonly IncludeCollection _2D = new IncludeCollection
            {
                // Pre-graph
                { CoreIncludes.CorePregraph },
                { CoreIncludes.ShaderGraphPregraph },

                // Post-graph
                { CoreIncludes.CorePostgraph },
                { k2DPass, IncludeLocation.Postgraph },
            };
        }
        #endregion
    }
}

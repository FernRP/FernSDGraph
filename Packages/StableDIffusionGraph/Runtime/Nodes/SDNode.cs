using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

namespace UnityEngine.SDGraph
{
    public class SDNode : BaseNode
    {
        public new StableDiffusionGraph graph => base.graph as StableDiffusionGraph;
        [HideInInspector]
        public SDNodeSetting settings = SDNodeSetting.defaultValue;

        [NonSerialized] internal SDNode parentSettingsNode;
        [NonSerialized] internal SDNode childSettingsNode;

        protected virtual SDNodeSetting defaultSettings => SDNodeSetting.defaultValue;
        private float m_nodeWidth = 0;
        public virtual float nodeWidth
        {
	        get
	        {
		        if (hasPreview)
		        {
			        return SDUtil.previewNodeWidth;
		        }
		        else
		        {
			        if (m_nodeWidth != 0) return m_nodeWidth;
			        return SDUtil.defaultNodeWidth;
		        }
	        }
	        set
	        {
		        m_nodeWidth = value;
	        }
        }

        [HideInInspector]
        public virtual Texture previewTexture => null;
        [HideInInspector]
        public bool hasSettings = false;
        [HideInInspector]
        public bool hasPreview = false;
        [HideInInspector]
        public bool isCanSaveTexture = false;

        public virtual List<OutputDimension> supportedDimensions => new List<OutputDimension>()
        {
            OutputDimension.Texture2D,
            OutputDimension.Texture3D,
            OutputDimension.CubeMap,
        };

        public virtual PreviewChannels defaultPreviewChannels => PreviewChannels.RGBA;

        public virtual bool canEditPreviewSRGB => true;
        public virtual bool defaultPreviewSRGB => false;

        public virtual bool showDefaultInspector => false;
        public virtual bool showPreviewExposure => false;
        [SerializeField, HideInInspector] public bool isPreviewCollapsed = false;

        public event Action onSettingsChanged;

        internal event Action onEnabled;

        public override bool showControlsOnHover => false; // Disable this feature for now

        public override bool needsInspector => true;

        protected Dictionary<string, Material> temporaryMaterials = new Dictionary<string, Material>();

        // UI Serialization
        [SerializeField, HideInInspector] public PreviewChannels previewMode;
        [SerializeField, HideInInspector] public bool previewSRGB;

        [SerializeField, HideInInspector] public float previewMip = 0.0f;
        [SerializeField, HideInInspector] public bool previewVisible = true;
        [SerializeField, HideInInspector] public float previewEV100 = 0.0f;
        [HideInInspector] public float previewSlice = 0;
        [HideInInspector] public bool isPinned;

        CustomSampler _sampler = null;

        CustomSampler sampler
        {
            get
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (_sampler == null)
                {
                    _sampler = CustomSampler.Create($"{name} - {GetHashCode()}", true);
                    recorder = _sampler.GetRecorder();
                    recorder.enabled = true;
                }
#endif
                return _sampler;
            }
        }

        protected Recorder recorder { get; private set; }

        internal virtual float processingTimeInMillis
        {
            get
            {
                // By default we display the GPU processing time
                if (recorder != null)
                    return recorder.gpuElapsedNanoseconds / 1000000.0f;
                return 0;
            }
        }

        protected SDNodeSetting Get2DOnlyRTSettings(SDNodeSetting defaultSettings)
        {
            var rtSettings = defaultSettings;

            rtSettings.editFlags &= ~EditFlags.Dimension;
            rtSettings.dimension = OutputDimension.Texture2D;

            return rtSettings;
        }

        protected SDNodeSetting Get3DOnlyRTSettings(SDNodeSetting defaultSettings)
        {
            var rtSettings = defaultSettings;

            rtSettings.editFlags &= ~EditFlags.Dimension;
            rtSettings.dimension = OutputDimension.Texture3D;

            return rtSettings;
        }

        protected SDNodeSetting GetCubeOnlyRTSettings(SDNodeSetting defaultSettings)
        {
            var rtSettings = defaultSettings;

            rtSettings.editFlags &= ~EditFlags.Dimension;
            rtSettings.dimension = OutputDimension.CubeMap;

            return rtSettings;
        }

        public override void OnNodeCreated()
        {
            base.OnNodeCreated();
            settings = defaultSettings;
            previewMode = defaultPreviewChannels;
            previewSRGB = defaultPreviewSRGB;

            // Patch up inheritance mode with default value in graph
            onEnabled += () => settings.SyncInheritanceMode(graph.defaultNodeInheritanceMode);
        }

        protected override void Enable()
        {
            base.Enable();
            onAfterEdgeConnected += UpdateSettings;
            onAfterEdgeDisconnected += UpdateSettings;
            onSettingsChanged += UpdateSettings;
            UpdateSettings();
            onEnabled?.Invoke();
        }
        
        protected override void Disable()
        {
            foreach (var matKp in temporaryMaterials)
                CoreUtils.Destroy(matKp.Value);
            base.Disable();
            onAfterEdgeConnected -= UpdateSettings;
            onAfterEdgeDisconnected -= UpdateSettings;
            onSettingsChanged -= UpdateSettings;
        }
        
        public override void InitializePorts()
        {
            UpdateSettings();
            base.InitializePorts();
        }
        
        bool IsNodeUsingSettings(BaseNode n)
        {
            bool settings = n is SDNode m && m.hasSettings;

            // There are some exception where node don't have settings but we still inherit from them
            settings |= n is TextureNode;

            return true;
        }

        public void UpdateSettings() => UpdateSettings(null);

        void UpdateSettings(SerializableEdge edge)
        {
            // Update nodes used to infere settings values
            parentSettingsNode = GetInputNodes().FirstOrDefault(n => IsNodeUsingSettings(n)) as SDNode;
            childSettingsNode = GetOutputNodes().FirstOrDefault(n => IsNodeUsingSettings(n)) as SDNode;

            settings.ResolveAndUpdate(this);
        }
        
        protected bool UpdateTempRenderTexture(ref CustomRenderTexture target, bool hasMips = false, bool autoGenerateMips = false,
			CustomRenderTextureUpdateMode updateMode = CustomRenderTextureUpdateMode.OnDemand, bool depthBuffer = true,
			GraphicsFormat overrideGraphicsFormat = GraphicsFormat.None, int outputWidth = 0, int outputHeight = 0, bool hideAsset = true)
		{
			bool changed = false;
			if(outputWidth == 0) outputWidth = settings.GetResolvedWidth(graph);
			if(outputHeight == 0)  outputHeight = settings.GetResolvedHeight(graph);
			int outputDepth = settings.GetResolvedDepth(graph);
			var filterMode = settings.GetResolvedFilterMode(graph);
			var wrapMode = settings.GetResolvedWrapMode(graph);
			GraphicsFormat targetFormat = overrideGraphicsFormat != GraphicsFormat.None ? overrideGraphicsFormat : settings.GetGraphicsFormat(graph);
			TextureDimension dimension = GetTempTextureDimension();

			outputWidth = Mathf.Max(outputWidth, 1);
			outputHeight = Mathf.Max(outputHeight, 1);
			outputDepth = Mathf.Max(outputDepth, 1);

			if (dimension != TextureDimension.Tex3D)
				outputDepth = 1;
			
			if (dimension == TextureDimension.Cube)
				outputHeight = outputDepth = outputWidth; // we only use the width for cubemaps

            if (targetFormat == GraphicsFormat.None)
                targetFormat = graph.mainOutputTexture.graphicsFormat;
			if (dimension == TextureDimension.None)
				dimension = TextureDimension.Tex2D;

			if (dimension == TextureDimension.Tex3D)
				depthBuffer = false;

			if (target == null)
			{
                target = new CustomRenderTexture(outputWidth, outputHeight, targetFormat)
                {
                    volumeDepth = Math.Max(1, outputDepth),
					depth = depthBuffer ? 32 : 0,
                    dimension = dimension,
                    name = $"SDGraph Temp {name}",
                    updateMode = CustomRenderTextureUpdateMode.OnDemand,
                    doubleBuffered = settings.doubleBuffered,
                    wrapMode = settings.GetResolvedWrapMode(graph),
                    filterMode = settings.GetResolvedFilterMode(graph),
                    useMipMap = hasMips,
					autoGenerateMips = autoGenerateMips,
					enableRandomWrite = true,
					hideFlags = hideAsset ? HideFlags.HideAndDontSave : HideFlags.None,
					updatePeriod = settings.GetUpdatePeriodInMilliseconds(),
				};
				target.Create();
				target.material = SDUtil.dummyCustomRenderTextureMaterial;

				return true;
			}

			// TODO: check if format is supported by current system

			// Warning: here we use directly the settings from the 
			if (target.width != outputWidth
				|| target.height != outputHeight
				|| target.graphicsFormat != targetFormat
				|| target.dimension != dimension
				|| target.volumeDepth != outputDepth
				|| target.doubleBuffered != settings.doubleBuffered
				|| target.useMipMap != hasMips
				|| target.autoGenerateMips != autoGenerateMips)
			{
				target.Release();
				target.width = outputWidth;
				target.height = outputHeight;
				target.graphicsFormat = (GraphicsFormat)targetFormat;
				target.dimension = dimension;
				target.volumeDepth = outputDepth;
				target.depth = depthBuffer ? 32 : 0;
				target.doubleBuffered = settings.doubleBuffered;
                target.useMipMap = hasMips;
				target.autoGenerateMips = autoGenerateMips;
				target.enableRandomWrite = true;
				target.hideFlags = hideAsset ? HideFlags.HideAndDontSave : HideFlags.None;
				target.Create();
				if (target.material == null)
					target.material = SDUtil.dummyCustomRenderTextureMaterial;
				changed = true;
			}

			// Patch settings that don't require to re-create the texture
			target.updateMode = updateMode;
			target.updatePeriod = settings.GetUpdatePeriodInMilliseconds();
			target.wrapMode = settings.GetResolvedWrapMode(graph);
			target.filterMode = settings.GetResolvedFilterMode(graph);

			if (target.doubleBuffered)
			{
				target.EnsureDoubleBufferConsistency();
				var rt = target.GetDoubleBufferRenderTexture();
				if (rt.enableRandomWrite != true)
				{
					rt.Release();
					rt.enableRandomWrite = true;
					rt.Create();
				}
			}

			if (!target.IsCreated())
				target.Create();

			return changed;
		}
        
        protected virtual TextureDimension GetTempTextureDimension() => settings.GetResolvedTextureDimension(graph);
        
        public void OnSettingsChanged()
        {
	        onSettingsChanged?.Invoke();
	        graph.NotifyNodeChanged(this);
        }
        
        Dictionary<Material, Material>		defaultMaterials = new Dictionary<Material, Material>();
        public Material	GetDefaultMaterial(Material mat)
        {
	        Material defaultMat;

	        if (defaultMaterials.TryGetValue(mat, out defaultMat))
		        return defaultMat;
			
	        return defaultMaterials[mat] = CoreUtils.CreateEngineMaterial(mat.shader);
        }
        
        public void ResetMaterialPropertyToDefault(Material mat, string propName)
        {
	        if (mat == null)
		        return;

	        int idx = mat.shader.FindPropertyIndex(propName);
	        if (idx == -1)
		        return;

	        switch (mat.shader.GetPropertyType(idx))
	        {
		        case ShaderPropertyType.Float:
		        case ShaderPropertyType.Range:
			        mat.SetFloat(propName, GetDefaultMaterial(mat).GetFloat(propName));
			        break;
		        case ShaderPropertyType.Vector:
			        mat.SetVector(propName, GetDefaultMaterial(mat).GetVector(propName));
			        break;
		        case ShaderPropertyType.Texture:
			        mat.SetTexture(propName, GetDefaultMaterial(mat).GetTexture(propName));
			        break;
	        }
        }
        
        public Material GetTempMaterial(string shaderName)
        {
	        temporaryMaterials.TryGetValue(shaderName, out var material);

	        if (material == null)
	        {
		        var shader = Shader.Find(shaderName);
		        if (shader == null)
			        throw new Exception("Can't find shader " + shaderName);
		        material = temporaryMaterials[shaderName] = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
	        }

	        return material;
        }
    }


    [Flags]
    public enum EditFlags
    {
        None = 0,
        Width = 1 << 0,
        SizeMode = 1 << 1,
        Height = 1 << 2,
        Depth = 1 << 4,
        Dimension = 1 << 6,
        TargetFormat = 1 << 7,
        POTSize = 1 << 8,

        Size = SizeMode | Width | Height | Depth,
        Format = POTSize | Dimension | TargetFormat,

        All = ~0,
    }

    public enum POTSize
    {
        _32 = 32,
        _64 = 64,
        _128 = 128,
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192,
        Custom = -1,
    }

    public enum OutputSizeMode
    {
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        Absolute = 1,
    }

    public enum OutputDimension
    {
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        Texture2D = TextureDimension.Tex2D,
        CubeMap = TextureDimension.Cube,
        Texture3D = TextureDimension.Tex3D,
        // Texture2DArray = TextureDimension.Tex2DArray, // Not supported by CRT, will be handled as Texture3D and then saved as Tex2DArray
    }

    public enum OutputPrecision
    {
        InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        InheritFromChild = NodeInheritanceMode.InheritFromChild,
        LDR = 2,
        Half = 3,
        Full = 4,
    }

    public enum OutputChannel
    {
        InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        InheritFromChild = NodeInheritanceMode.InheritFromChild,
        RGBA = 1,
        RG = 2,
        R = 3,
    }

    public enum OutputWrapMode
    {
        InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        InheritFromChild = NodeInheritanceMode.InheritFromChild,
        Repeat = TextureWrapMode.Repeat,
        Clamp = TextureWrapMode.Clamp,
        Mirror = TextureWrapMode.Mirror,
        MirrorOnce = TextureWrapMode.MirrorOnce,
    }

    public enum OutputFilterMode
    {
        InheritFromGraph = NodeInheritanceMode.InheritFromGraph,
        InheritFromParent = NodeInheritanceMode.InheritFromParent,
        InheritFromChild = NodeInheritanceMode.InheritFromChild,
        Point = FilterMode.Point,
        Bilinear = FilterMode.Bilinear,
        Trilinear = FilterMode.Trilinear,
    }

    [Flags]
    public enum PreviewChannels
    {
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        RG = R | G,
        RB = R | B,
        GB = G | B,
        RGB = R | G | B,
        RGBA = R | G | B | A,
    }

    // Note: to keep in sync with UnityEditor.TextureCompressionQuality
    public enum SDGraphCompressionQuality
    {
        Fast = 0,
        Normal = 50,
        Best = 100,
    }

    public enum RefreshMode
    {
        OnLoad,
        EveryXFrame,
        EveryXMillis,
        EveryXSeconds,
    }

    public static class SDGraphEnumExtension
    {
        public static bool Inherits(this OutputSizeMode mode) => (int)mode <= 0;
        public static bool Inherits(this OutputChannel mode) => (int)mode <= 0;
        public static bool Inherits(this OutputPrecision mode) => (int)mode <= 0;
        public static bool Inherits(this OutputDimension mode) => (int)mode <= 0;
        public static bool Inherits(this OutputWrapMode mode) => (int)mode < 0;
        public static bool Inherits(this OutputFilterMode mode) => (int)mode < 0;
    }
}
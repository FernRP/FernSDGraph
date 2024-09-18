using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using UnityEngine.SDGraph;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Asset/TextureNode")]
    public class TextureNode : SDNode, ICreateNodeFrom<Texture>
    {
        [Output(name = "Texture")] public Texture outputTexture;
        public override Texture previewTexture => outputTexture;

        protected override void Enable()
        {
            hasPreview = true;
            hasSettings = true;
            base.Enable();
        }

        public bool InitializeNodeFromObject(Texture value)
        {
            outputTexture = value;
            return true;
        }
    }
}
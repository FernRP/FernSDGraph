using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GraphProcessor;
using System.Linq;
using FernNPRCore.SDNodeGraph;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Custom/TextureNode")]
    public class TextureNode : SDNode, ICreateNodeFrom<Texture>
    {
        public Texture textureAsset;

        [Output(name = "Texture")] public Texture outputTexture;

        public bool InitializeNodeFromObject(Texture value)
        {
            textureAsset = value;
            return true;
        }
    }
}
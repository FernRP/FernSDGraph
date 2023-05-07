using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

namespace FernShaderGraph
{
    [CreateAssetMenu(fileName = "CustomSGSettings", 
        menuName = "Create/CustomSGSettings")]
    public class FernSG_Settings : ScriptableObject
    {
        public Object CustomLitForwardPass;
        public Object NPRLighting;
    }

}


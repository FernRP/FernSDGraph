using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    public class SDPromptFavoriteData : ScriptableObject
    {
        public List<PromptRegisterData> FavoriteData = new List<PromptRegisterData>();
    }
    
    [CustomEditor(typeof(SDPromptFavoriteData))]
    public class SDPromptFavoriteDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
}
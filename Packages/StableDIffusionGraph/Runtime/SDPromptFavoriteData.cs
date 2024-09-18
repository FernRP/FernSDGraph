using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.SDGraph
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
            base.OnInspectorGUI();
        }
    }
}
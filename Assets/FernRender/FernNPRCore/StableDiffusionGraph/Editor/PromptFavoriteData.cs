using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FernNPRCore.StableDiffusionGraph
{
    public class PromptFavoriteData : ScriptableObject
    {
        public List<PromptRegisterData> FavoriteData = new List<PromptRegisterData>();
    }
    
    [CustomEditor(typeof(PromptFavoriteData))]
    public class PromptFavoriteDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
        }
    }
    
}
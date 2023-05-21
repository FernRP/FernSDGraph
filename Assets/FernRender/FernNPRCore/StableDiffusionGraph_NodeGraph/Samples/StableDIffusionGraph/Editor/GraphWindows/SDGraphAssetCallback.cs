using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;
using System.IO;

namespace FernNPRCore.SDNodeGraph
{
    public class SDGraphAssetCallback
    {
        [MenuItem("Assets/Create/FernGraph/NodeGraph/Stable Diffusion Graph", false, 10)]
        public static void CreateGraphPorcessor()
        {
            var graph = ScriptableObject.CreateInstance<StableDiffusionGraph>();
            ProjectWindowUtil.CreateAsset(graph, "StableDiffusionGraph.asset");
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as StableDiffusionGraph;
            
            var path = AssetDatabase.GetAssetPath(EditorUtility.InstanceIDToObject(instanceID));
            var graph = SDEditorUtils.GetGraphAtPath(path);
            
            if (graph == null)
                return false;

            StableDiffusionGraphWindow.Open(graph);
            return true;
        }
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GraphProcessor;
using UnityEditor.Callbacks;
using System.IO;
using System.Reflection;
using UnityEditor.ProjectWindowCallback;

namespace FernNPRCore.SDNodeGraph
{
    public class SDGraphAssetCallback
    {
        public static readonly string Extension = "asset";

        [MenuItem("Assets/Create/FernGraph/NodeGraph/Stable Diffusion Graph", false, 10)]
        public static void CreateGraphPorcessor()
        {
            var graph = ScriptableObject.CreateInstance<StableDiffusionGraph>();
            
             ProjectWindowUtil.CreateAsset(graph, "New StableDiffusionGraph.asset");
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as StableDiffusionGraph;

            if (asset == null) return false;
            
            var path = AssetDatabase.GetAssetPath(asset);
            var graph = SDEditorUtils.GetGraphAtPath(path);
            
            if (graph == null)
                return false;

            StableDiffusionGraphWindow.Open(graph);
            return true;
        }
    }
}
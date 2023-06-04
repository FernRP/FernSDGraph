using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace StylizedGrass
{
    [CustomEditor(typeof(StylizedGrassRenderer))]
    public class StylizedGrassRendererInspector : Editor
    {
        StylizedGrassRenderer script;
        SerializedProperty colorMap;
        SerializedProperty listenToWindZone;
        SerializedProperty windZone;

        private bool renderFeaturePresent;
        private void OnEnable()
        {
            script = (StylizedGrassRenderer)target;
            
            #if URP
            renderFeaturePresent = PipelineUtilities.RenderFeatureAdded<GrassBendingFeature>();
            #endif

            //if (script.followTarget == null) script.followTarget = Camera.main?.transform;
            
            colorMap = serializedObject.FindProperty("colorMap");
            listenToWindZone = serializedObject.FindProperty("listenToWindZone");
            windZone = serializedObject.FindProperty("windZone");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Version " + AssetInfo.INSTALLED_VERSION, EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);

#if !URP
            EditorGUILayout.HelpBox("The Universal Render Pipeline v" + AssetInfo.MIN_URP_VERSION + " is not installed", MessageType.Error);
#else

            if (!renderFeaturePresent)
            {
                EditorGUILayout.HelpBox("The grass bending render feature hasn't been added\nto the current renderer", MessageType.Error);

                GUILayout.Space(-32);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Add", GUILayout.Width(60)))
                    {
                        AddRenderFeature();
                    }
                    GUILayout.Space(8);
                }
                GUILayout.Space(11);
            }


            serializedObject.Update();

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(colorMap, new GUIContent("Active color map"));

            if (colorMap.objectReferenceValue == null && GrassColorMapRenderer.Instance)
            {
                EditorGUILayout.HelpBox("A Colormap Renderer component is present, you don't have to assign a colormap in this case", MessageType.Info);
            }
            EditorGUILayout.PropertyField(listenToWindZone);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (listenToWindZone.boolValue)
                {
                    EditorGUILayout.PropertyField(windZone);

                    if (!windZone.objectReferenceValue)
                    {
                        if (GUILayout.Button("Create", GUILayout.MaxWidth(75f)))
                        {
                            GameObject obj = new GameObject();
                            obj.name = "Wind Zone";
                            WindZone wz = obj.AddComponent<WindZone>();

                            windZone.objectReferenceValue = wz;

                            Selection.activeGameObject = obj;
                        }
                    }

                }
            }
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                if (colorMap.objectReferenceValue) script.colorMap.SetActive();
            }

            EditorGUILayout.LabelField("- Staggart Creations -", EditorStyles.centeredGreyMiniLabel);
#endif
        }
        
        #if URP
        private void AddRenderFeature()
        {
            PipelineUtilities.AddRenderFeature<GrassBendingFeature>();
            renderFeaturePresent = true;
        }
        #endif

        public override bool HasPreviewGUI()
        {
            return script.vectorRT;
        }

        public override GUIContent GetPreviewTitle()
        {
            return new GUIContent("Grass bending vectors");
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (!script.vectorRT) return;

            GUI.DrawTexture(r, script.vectorRT, ScaleMode.ScaleToFit);

            Rect btnRect = r;
            btnRect.x += 5f;
            btnRect.y += 5f;
            btnRect.width = 150f;
            btnRect.height = 20f;
            script.debug = GUI.Toggle(btnRect, script.debug, new GUIContent(" Pin to viewport"));

            GUI.Label(new Rect(r.width * 0.5f - (175 * 0.5f), r.height - 5, 175, 25), string.Format("{0} texel(s) per meter", ColorMapEditor.GetTexelSize(script.vectorRT.height, GrassBendingFeature.RenderBendVectors.CurrentResolution)), EditorStyles.toolbarButton);

        }
    }
}

//Stylized Grass Shader
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#if SGS_DEV
#define ENABLE_SHADER_STRIPPING_LOG
//Deep debugging only, makes the stripping process A LOT slower
//#define ENABLE_KEYWORD_STRIPPING_LOG
#endif

using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;
#if URP
using UnityEngine.Rendering.Universal;
#endif

namespace StylizedGrass
{
    public class ShaderConfigurator
    {
        public enum Configuration
        {
            VegetationStudio,
            NatureRenderer,
            GPUInstancer
        }

        private const string NatureRendererGUID = "e184c5532d8acad44a76e8763685710f";
        private const string GPUInstancerGUID = "18df6f4b5f1ec6045ad24ed3cf05d13b";

        public static Configuration CurrentConfig
        {
            get { return (Configuration)EditorPrefs.GetInt(PlayerSettings.productName + "_SGS_SHADER_CONFIG", 0); }
            set { EditorPrefs.SetInt(PlayerSettings.productName + "_SGS_SHADER_CONFIG", (int)value); }
        }

        private const string ShaderGUID = "d7dd1c3f4cba1d441a7d295a168bac0d";
        private static string ShaderFilePath;
        private struct CodeBlock
        {
            public int startLine;
            public int endLine;
        }

        private static void RefreshShaderFilePath()
        {
            ShaderFilePath = AssetDatabase.GUIDToAssetPath(ShaderGUID);
        }

#if SGS_DEV
        [MenuItem("SGS/Installation/ConfigureForVegetationStudio")]
#endif
        public static void ConfigureForVegetationStudio()
        {
            RefreshShaderFilePath();

            EditorUtility.DisplayProgressBar(AssetInfo.ASSET_NAME, "Modifying shader...", 1f);
            {
                ToggleCodeBlock(ShaderFilePath, "NatureRenderer", false);
                ToggleCodeBlock(ShaderFilePath, "GPUInstancer", false);
                ToggleCodeBlock(ShaderFilePath, "VegetationStudio", true);
            }
            EditorUtility.ClearProgressBar();

            Debug.Log("Shader file modified to use Vegetation Studio integration");

            CurrentConfig = Configuration.VegetationStudio;

        }

#if SGS_DEV
        [MenuItem("SGS/Installation/ConfigureForGPUInstancer")]
#endif
        public static void ConfigureForGPUInstancer()
        {
            RefreshShaderFilePath();

            string libraryFilePath = AssetDatabase.GUIDToAssetPath(GPUInstancerGUID);
            if (libraryFilePath == string.Empty)
            {
                Debug.LogError("GPU Instancer shader library could not be found with GUID " + GPUInstancerGUID + ". This means it was changed by the author, or it simply doesn't exist");
                return;
            }
            
            Debug.Log("GPU Instancer shader library found at <i>" + libraryFilePath + "</i>");

            EditorUtility.DisplayProgressBar(AssetInfo.ASSET_NAME, "Modifying shader...", 1f);
            {
                ToggleCodeBlock(ShaderFilePath, "NatureRenderer", false);
                ToggleCodeBlock(ShaderFilePath, "GPUInstancer", true);
                ToggleCodeBlock(ShaderFilePath, "VegetationStudio", false);
            }
            EditorUtility.ClearProgressBar();
            
            SetIncludePath(ShaderFilePath, "GPUInstancer", libraryFilePath);

            Debug.Log("Shader file modified to use GPU Instancer integration");

            CurrentConfig = Configuration.GPUInstancer;
        }

#if SGS_DEV
        [MenuItem("SGS/Installation/ConfigureForNatureRenderer")]
#endif
        public static void ConfigureForNatureRenderer()
        {
            RefreshShaderFilePath();
            
            string libraryFilePath = AssetDatabase.GUIDToAssetPath(NatureRendererGUID);
            if (libraryFilePath == string.Empty)
            {
                Debug.LogError("Nature Shaders shader library could not be found with GUID " + NatureRendererGUID + ". This means it was changed by the author, or it simply doesn't exist");
                return;
            }

            Debug.Log("Nature Renderer shader library found at <i>" + libraryFilePath + "</i>");

            EditorUtility.DisplayProgressBar(AssetInfo.ASSET_NAME, "Modifying shader...", 1f);
            {
                ToggleCodeBlock(ShaderFilePath, "NatureRenderer", true);
                ToggleCodeBlock(ShaderFilePath, "GPUInstancer", false);
                ToggleCodeBlock(ShaderFilePath, "VegetationStudio", false);
            }
            EditorUtility.ClearProgressBar();
            
            SetIncludePath(ShaderFilePath, "NatureRenderer", libraryFilePath);

            Debug.Log("Shader file modified to use Nature Renderer integration");

            CurrentConfig = Configuration.NatureRenderer;
        }

        public static void ToggleCodeBlock(string filePath, string id, bool enable)
        {
            string[] lines = File.ReadAllLines(filePath);

            List<CodeBlock> codeBlocks = new List<CodeBlock>();

            //Find start and end line indices
            for (int i = 0; i < lines.Length; i++)
            {
                bool blockEndReached = false;

                if (lines[i].Contains("/* Configuration: ") && enable)
                {
                    lines[i] = lines[i].Replace(lines[i], "/* Configuration: " + id + " */");
                }

                if (lines[i].Contains("start " + id))
                {
                    CodeBlock codeBlock = new CodeBlock();

                    codeBlock.startLine = i;

                    //Find related end point
                    for (int l = codeBlock.startLine; l < lines.Length; l++)
                    {
                        if (blockEndReached == false)
                        {
                            if (lines[l].Contains("end " + id))
                            {
                                codeBlock.endLine = l;

                                blockEndReached = true;
                            }
                        }
                    }

                    codeBlocks.Add(codeBlock);
                    blockEndReached = false;
                }

            }

            if (codeBlocks.Count == 0)
            {
                //Debug.Log("No code blocks with the marker \"" + id + "\" were found in file");

                return;
            }

            foreach (CodeBlock codeBlock in codeBlocks)
            {
                if (codeBlock.startLine == codeBlock.endLine) continue;

                //Debug.Log((enable ? "Enabled" : "Disabled") + " \"" + id + "\" code block. Lines " + (codeBlock.startLine + 1) + " through " + (codeBlock.endLine + 1));

                for (int i = codeBlock.startLine + 1; i < codeBlock.endLine; i++)
                {
                    //Uncomment lines
                    if (enable == true)
                    {
                        if (lines[i].StartsWith("//") == true) lines[i] = lines[i].Remove(0, 2);
                    }
                    //Comment out lines
                    else
                    {
                        if (lines[i].StartsWith("//") == false) lines[i] = "//" + lines[i];
                    }
                }
            }

            File.WriteAllLines(filePath, lines);

            AssetDatabase.ImportAsset(filePath);
        }

        private static void SetIncludePath(string filePath, string id, string libraryPath)
        {
            string[] lines = File.ReadAllLines(filePath);

            //This assumes the line is already uncommented
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("/* include " + id))
                {
                    lines[i + 1] = System.String.Format("#include \"{0}\"", libraryPath);
                    
                    File.WriteAllLines(filePath, lines);
                    AssetDatabase.ImportAsset(filePath);
                }
            }
        }

        public static Configuration GetConfiguration(Shader shader)
        {
            string filePath = AssetDatabase.GetAssetPath(shader);

            string[] lines = File.ReadAllLines(filePath);

            string configStr = lines[0].Replace("/* Configuration: ", string.Empty);
            configStr = configStr.Replace(" */", string.Empty);

            Configuration config = Configuration.VegetationStudio;
            if (configStr == "NatureRenderer") config = Configuration.NatureRenderer;
            if (configStr == "GPUInstancer") config = Configuration.GPUInstancer;

            CurrentConfig = config;
            return config;
        }
    }
    
    #if URP
    //Strips shader variants for features belonging to a newer URP version. This avoids unnecessarily long build times in older URP versions
    class KeywordStripper : IPreprocessShaders
    {
        public int callbackOrder { get { return 0; } }
		private const string LOG_FILEPATH = "Library/Grass Shader Compilation.log";
		private const string SHADER_NAME = "Universal Render Pipeline/Nature/Stylized Grass";
        
        private List<ShaderKeyword> excludedKeywords;
        
        public KeywordStripper()
        {
            Initialize();   
        }

        private void Initialize()
        {
            //Note: Order in which keywords are declared should match the order in the passes
            excludedKeywords = new List<ShaderKeyword> 
            { 
                //new ShaderKeyword("DEBUG"),
                #if !ENABLE_HYBRID_RENDERER_V2 || !DOTS_INSTANCING
                new ShaderKeyword("DOTS_INSTANCING_ON"),
                #endif
                
                #if !UNITY_2020_2_OR_NEWER
                new ShaderKeyword("LIGHTMAP_SHADOW_MIXING"),
                new ShaderKeyword("SHADOWS_SHADOWMASK"),
                new ShaderKeyword("_SCREEN_SPACE_OCCLUSION"),
                #endif
				
				#if !UNITY_2021_2_OR_NEWER
				new ShaderKeyword("_MAIN_LIGHT_SHADOWS_SCREEN"),
				#endif

                #if !UNITY_2021_2_OR_NEWER
                new ShaderKeyword("_DISABLE_DECALS"),
                
                new ShaderKeyword("_DBUFFER_MRT1"),
                new ShaderKeyword("_DBUFFER_MRT2"),
                new ShaderKeyword("_DBUFFER_MRT3"),
                new ShaderKeyword("_LIGHT_LAYERS"),
                new ShaderKeyword("_LIGHT_COOKIES"),
                //new ShaderKeyword("_RENDER_PASS_ENABLED"), //GBuffer only, so stripped anyway
                new ShaderKeyword("_CLUSTERED_RENDERING"),
                new ShaderKeyword("DYNAMICLIGHTMAP_ON"),
                new ShaderKeyword("DEBUG_DISPLAY"),
                #endif
                
                #if !UNITY_2022_2_OR_NEWER
                new ShaderKeyword("_FORWARD_PLUS"),
                #endif
            };
			
			#if ENABLE_SHADER_STRIPPING_LOG
			//Clear log file
			File.WriteAllLines(LOG_FILEPATH, new string[] {});
			
			m_stripTimer = new Stopwatch();
			#endif
            
            #if SGS_DEV
            Debug.LogFormat("KeywordStripper initialized. {0} keywords are to be stripped", excludedKeywords.Count);
            #endif
        }

        #if ENABLE_SHADER_STRIPPING_LOG
        private System.Diagnostics.Stopwatch m_stripTimer;
        #endif
        
        //https://github.com/Unity-Technologies/Graphics/blob/9e934fb134d995259903b4850259c5c8953597f9/com.unity.render-pipelines.universal/Editor/ShaderPreprocessor.cs#L398
        public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> compilerDataList)
        {      
            if (compilerDataList == null || compilerDataList.Count == 0) return;

            if (shader.name != SHADER_NAME) return;

            if(excludedKeywords == null) Initialize();

			#if ENABLE_SHADER_STRIPPING_LOG
			File.AppendAllText(LOG_FILEPATH, $"\nOnProcessShader running for {shader.name}, Pass {snippet.passName}, (stage: {snippet.shaderType}). Num variants: {compilerDataList.Count}" + "\n" );

			m_stripTimer.Start();
			#endif

            if (StripUnusedPasses(shader, snippet))
            {
                compilerDataList.Clear();
            }
            
            for (int i = compilerDataList.Count -1; i >= 0; i--)
            {
				bool removeInput = false;
                removeInput = StripUnusedVariants(shader, compilerDataList[i], snippet);

                if (removeInput)
                {
                    compilerDataList.RemoveAt(i);
                    continue;
                }
            }
            
            #if ENABLE_SHADER_STRIPPING_LOG
            m_stripTimer.Stop();
			System.TimeSpan stripTimespan = m_stripTimer.Elapsed;
			File.AppendAllText(LOG_FILEPATH, $"OnProcessShader, stripping for pass {snippet.shaderType} took {stripTimespan.Minutes}m{stripTimespan.Seconds}s. Remaining variants to compile: {compilerDataList.Count}" + "\n" );
			m_stripTimer.Reset();
            #endif
        }
        
        private bool StripAllUnused(Shader shader, ShaderCompilerData compilerData, ShaderSnippetData snippet)
        {
            if (StripUnusedPasses(shader, snippet))
            {
                return true;
            }

			foreach (ShaderKeyword keyword in excludedKeywords)
			{
				if (StripKeyword(shader, compilerData, keyword, snippet))
				{
					return true;
				}
			}
			
            return false;
        }

        private bool StripUnusedVariants(Shader shader, ShaderCompilerData compilerData, ShaderSnippetData snippet)
        {
            foreach (ShaderKeyword keyword in excludedKeywords)
            {
                if (StripKeyword(shader, compilerData, keyword, snippet))
                {
                    return true;
                }
            }
			
            return false;
        }
        
        private bool StripUnusedPasses(Shader shader, ShaderSnippetData snippet)
        {
            #if !UNITY_2020_2_OR_NEWER
            if (snippet.passName == "DepthNormals")
            {
				#if ENABLE_SHADER_STRIPPING_LOG
				File.AppendAllText(LOG_FILEPATH, $"Stripped {snippet.passName} pass, (stage: {snippet.shaderType}) from {shader.name}, it belongs to a newer URP version" + "\n" );
				#endif
                return true;
            }
            #endif

            #if !UNITY_2021_2_OR_NEWER //Starting from URP10, there is a GBuffer pass, but no way to enable deferred rendering until URP 12
            if (snippet.passName == "GBuffer")
            {
				#if ENABLE_SHADER_STRIPPING_LOG
				File.AppendAllText(LOG_FILEPATH, $"Stripped {snippet.passName} pass, (stage: {snippet.shaderType}) from {shader.name}, it belongs to a newer URP version" + "\n" );
				#endif
                return true;
            }
            #endif
            
            return false;
        }

        private string GetKeywordName(Shader shader, ShaderKeyword keyword)
        {
            #if UNITY_2021_2_OR_NEWER
			return keyword.name;
			#else
            return ShaderKeyword.GetKeywordName(shader, keyword);
			#endif
        }

        private bool StripKeyword(Shader shader, ShaderCompilerData compilerData, ShaderKeyword keyword,  ShaderSnippetData snippet)
        {
            if (compilerData.shaderKeywordSet.IsEnabled(keyword))
            {
				#if ENABLE_SHADER_STRIPPING_LOG && ENABLE_KEYWORD_STRIPPING_LOG
                File.AppendAllText(LOG_FILEPATH, "- " + $"Stripped {GetKeywordName(shader, keyword)} variant from pass {snippet.passName} (stage: {snippet.shaderType})" + "\n" );
				#endif                

                return true;
            }

            return false;
        }
    }
    #endif
}
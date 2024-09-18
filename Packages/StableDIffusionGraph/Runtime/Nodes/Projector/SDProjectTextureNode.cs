using System.IO;
using GraphProcessor;
using Unity.Collections;
using Unity.Jobs;
using UnityEditor;
using UnityEditor.VersionControl;

namespace UnityEngine.SDGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/Projector Texture")]
    public class SDProjectTextureNode : SDNode
    {
        [Input("Image")] public Texture2D inputImage;
        
        public override string name => "Projector Texture";

        private Camera camera;
        private Texture2D bakingTexture;
        public int bakingTextureSize = 32;
        private string bakingTextureName;
        private const string bakingTextureDefaultName = "Projector_OutputImage";
        
        private Color32 blankPixelColor = new Color32(0, 0, 0, 0);
        public int samples = 1;

        public void Projector()
        {
            camera = Camera.main;

            Color[] screenTexturePixels = inputImage.GetPixels();
            bakingTexture = new Texture2D(bakingTextureSize, bakingTextureSize, TextureFormat.ARGB32, false);
            
            Color[] bakingTexturePixels = bakingTexture.GetPixels();
            
            for (int i = 0; i < bakingTexturePixels.Length; i++)
            { 
                bakingTexturePixels[i] = blankPixelColor;
            }

            Color[] currentPassPixels = bakingTexturePixels;
                
            for (int i = 0; i < samples; i++)
            {
                var results = new NativeArray<RaycastHit>(inputImage.height * inputImage.width, Allocator.TempJob);
                var commands = new NativeArray<RaycastCommand>(inputImage.height * inputImage.width, Allocator.TempJob);

                if (EditorUtility.DisplayCancelableProgressBar("Screen Texture Baker", "Progress: " + (i + 1) + " / " + samples + " Sample(s)", (i + 1) / (float)samples))
                {
                    EditorUtility.ClearProgressBar();
                    results.Dispose();
                    commands.Dispose();

                    Debug.Log("Screen Texture Baker: Bake canceled by the user.");

                    return;
                }

                for (int y = 0; y < 512; y++)
                {
                    for (int x = 0; x < 512; x++)
                    {
                        Ray ray = camera.ScreenPointToRay(new Vector3(x + Random.value, y + Random.value, 0));
                        commands[inputImage.width * y + x] = new RaycastCommand(ray.origin, ray.direction, 90);
                    }
                }

                JobHandle handle = RaycastCommand.ScheduleBatch(commands, results, 1, default(JobHandle));

                handle.Complete();
                
                for (int j = 0; j < results.Length; j++)
                {
                    RaycastHit batchedHit = results[j];

                    if (batchedHit.collider != null)
                    {
                        Vector2 pixelUV = batchedHit.textureCoord;

                        pixelUV.x *= bakingTextureSize;
                        pixelUV.y *= bakingTextureSize;

                        currentPassPixels[bakingTexture.width * (int)pixelUV.y + (int)pixelUV.x] = screenTexturePixels[j];
                    }
                }

                for (int n = 0; n < bakingTexturePixels.Length; n++)
                {
                    bakingTexturePixels[n] = (bakingTexturePixels[n].a <= 0) ? currentPassPixels[n] : Color.Lerp(bakingTexturePixels[n], currentPassPixels[n], 1.0f / (i + 1.0f));
                }

                results.Dispose();
                commands.Dispose();
            }
            
            bakingTexture.SetPixels(bakingTexturePixels);
            bakingTexture.Apply();
            byte[] bytes = bakingTexture.EncodeToPNG();
            
            if (bakingTextureName == "")
            {
                bakingTextureName = bakingTextureDefaultName;
            }
            
            File.WriteAllBytes(Application.dataPath + "/" + bakingTextureName + ".png", bytes);
            AssetDatabase.Refresh();
            
            EditorUtility.ClearProgressBar();

            Debug.Log("Screen Texture Baker: Bake Complete.");
            

        }
    }
}
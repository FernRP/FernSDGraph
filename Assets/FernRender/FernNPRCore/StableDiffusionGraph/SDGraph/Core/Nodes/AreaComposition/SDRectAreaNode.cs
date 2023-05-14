using FernGraph;
using FernGraph.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace FernNPRCore.StableDiffusionGraph
{
    public class RectRegion
    {
        public Rect rect;
        public Color color;

        public RectRegion(Rect rect, Color color)
        {
            this.rect = rect;
            this.color = color;
        }
    }

    [Node(Path = "SD AreaComposition")]
    [Tags("SD Node")]
    public class SDRectAreaNode : Node
    {
        [Input] public int Width = 512;
        [Input] public int Height = 512;
        public float scale = 1;
        public int colCount = 8, rowCount = 8;
        public bool isSnapGrid = true;
        public bool showGrid = true;
        public bool alwaysUpdateTexture = true;

        [Output("Out AreaTexture")] public Texture2D areaTexture;

        public List<RectRegion> regions = new List<RectRegion>();

        public override object OnRequestValue(Port port)
        {
            areaTexture = new Texture2D(Width, Height);
            for (int i = 0; i < regions.Count; i++)
            {
               var rect = new Rect(regions[i].rect.x / scale,
                regions[i].rect.y / scale,
                regions[i].rect.width / scale,
                regions[i].rect.height / scale
                );

                DrawRect(areaTexture, rect, regions[i].color, Width, Height);
            }
            areaTexture.Apply();
            return areaTexture;
        }
        private void DrawRect(Texture2D texture, Rect rect, Color color, int width, int height)
        {
            for (int y = (int)rect.yMin; y < rect.yMax; y++)
            {
                for (int x = (int)rect.xMin; x < rect.xMax; x++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        texture.SetPixel(x, height - 1 - y, color);
                    }
                }
            }
        }
    }
}
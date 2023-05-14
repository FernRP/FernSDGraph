using FernGraph;
using FernGraph.Editor;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.StableDiffusionGraph
{
    [CustomNodeView(typeof(SDSplitAreaNode))]
    public class SDSplitAreaNodeView : NodeView
    {
        SDSplitAreaNode splitAreaNode;
        protected override void OnInitialize()
        {
            base.OnInitialize();
            splitAreaNode = Target as SDSplitAreaNode;

            PortView inView = GetInputPort("areaTexture");
            if (inView != null) inView.AddToClassList("PreviewInImg");
            var button = new Button(Split);
            button.style.backgroundImage = SDTextureHandle.RefreshIcon;
            button.style.width = 20;
            button.style.height = 20;
            button.style.alignSelf = Align.Auto;
            button.style.bottom = 5;
            titleContainer.Add(button);
        }

        void Split()
        {
            if(splitAreaNode!=null&& splitAreaNode.areaTexture != null)
            {
                ReadColors(splitAreaNode.areaTexture, splitAreaNode.colors, splitAreaNode.threshold);
                splitAreaNode.RemoveAllOutputs();
                Outputs.Clear();
                outputContainer.Clear();
                for (int i = 0; i < splitAreaNode.colors.Count; i++)
                {
                    FernGraph.Port port = new FernGraph.Port()
                    {
                         Type=typeof(Color),
                           Direction = PortDirection.Output,
                           Name = $"Area_{i.ToString("D2")}"
                    };
                    splitAreaNode.AddPort(port);

                    AddOutputPort(port);
                }
            }
        }


        public void ReadColors(Texture2D texture,List<Color> colors,float threshold)
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color pixelColor = texture.GetPixel(x, y);
                    bool addColor = true;
                    foreach (Color color in colors)
                    {
                        if (Mathf.Abs(color.r - pixelColor.r) < threshold &&
                            Mathf.Abs(color.g - pixelColor.g) < threshold &&
                            Mathf.Abs(color.b - pixelColor.b) < threshold)
                        {
                            addColor = false;
                            break;
                        }
                    }
                    if (addColor)
                    {
                        Debug.Log(pixelColor);

                        colors.Add(pixelColor);
                    }
                }
            }
        }
    }
}
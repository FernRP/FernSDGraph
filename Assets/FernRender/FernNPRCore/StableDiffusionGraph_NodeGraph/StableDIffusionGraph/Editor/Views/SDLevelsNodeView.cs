using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{
    [NodeCustomEditor(typeof(SDLevelNode))]
    public class SDLevelsNodeView : SDNodeView
    {
        private SDLevelNode levelsNode;

        // Workaround to update the sliders we have in the inspector / node
        // When serialization issues are fixed, we could have a drawer for min max and avoid to manually write the UI for it
        List<MinMaxSlider> sliders = new List<MinMaxSlider>();

        public override void Enable(bool fromInspector = false)
        {
            base.Enable(fromInspector);
            levelsNode = nodeTarget as SDLevelNode;

            var slider = new MinMaxSlider("Luminance", levelsNode.min, levelsNode.max, 0, 1);
            sliders.Add(slider);
            slider.RegisterValueChangedCallback(e =>
            {
                owner.RegisterCompleteObjectUndo("Changed Luminance remap");
                levelsNode.min = e.newValue.x;
                levelsNode.max = e.newValue.y;
                foreach (var s in sliders)
                    if (s != null && s.parent != null)
                        s.SetValueWithoutNotify(e.newValue);
                NotifyNodeChanged();
            });
            controlsContainer.Add(slider);

            var mode = this.Q<EnumField>();

            mode.RegisterValueChangedCallback((m) => { UpdateMinMaxSliderVisibility((SDLevelNode.Mode)m.newValue); });
            UpdateMinMaxSliderVisibility(levelsNode.mode);

            void UpdateMinMaxSliderVisibility(SDLevelNode.Mode mode)
            {
                slider.style.display = mode == SDLevelNode.Mode.Automatic ? DisplayStyle.None : DisplayStyle.Flex;
            }

            UpdateHistogram();

            levelsNode.onProcessed -= UpdateHistogram;
            levelsNode.onProcessed += UpdateHistogram;

            var histogram = new HistogramView(levelsNode.histogramData, owner);
            controlsContainer.Add(histogram);
        }

        void UpdateHistogram()
        {
            if (levelsNode.output != null)
            {
                var cmd = CommandBufferPool.Get("Update Histogram");
                HistogramUtility.ComputeHistogram(cmd, levelsNode.output, levelsNode.histogramData);
                Graphics.ExecuteCommandBuffer(cmd);
            }
        }
    }
}
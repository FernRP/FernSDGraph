using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphProcessor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace FernNPRCore.SDNodeGraph
{
    public class SDEditorUtils
    {
        public static StableDiffusionGraph GetGraphAtPath(string path)
        {
            return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o is StableDiffusionGraph) as
                StableDiffusionGraph;
        }

        static Texture2D _pinIcon;

        public static Texture2D pinIcon
        {
            get => _pinIcon == null ? _pinIcon = LoadIcon("Icons/Pin") : _pinIcon;
        }

        static Texture2D _unpinIcon;

        public static Texture2D unpinIcon
        {
            get => _unpinIcon == null ? _unpinIcon = LoadIcon("Icons/Unpin") : _unpinIcon;
        }

        static Texture2D LoadIcon(string resourceName)
        {
            if (UnityEditorInternal.InternalEditorUtility.HasPro())
            {
                string darkIconPath = Path.GetDirectoryName(resourceName) + "/d_" + Path.GetFileName(resourceName);
                var darkIcon = Resources.Load<Texture2D>(darkIconPath);
                if (darkIcon != null)
                    return darkIcon;
            }

            return Resources.Load<Texture2D>(resourceName);
        }

        public static Vector4 GetChannelsMask(PreviewChannels channels)
        {
            return new Vector4(
                (channels & PreviewChannels.R) == 0 ? 0 : 1,
                (channels & PreviewChannels.G) == 0 ? 0 : 1,
                (channels & PreviewChannels.B) == 0 ? 0 : 1,
                (channels & PreviewChannels.A) == 0 ? 0 : 1
            );
        }
        
        public static void ScheduleAutoHide(VisualElement target, BaseGraphView view)
        {
            target.schedule.Execute(() =>
                {
                    target.visible = float.IsNaN(target.worldBound.x) ||
                                     target.worldBound.Overlaps(view.worldBound);
                })
                .Every(16); // refresh the visible for 60hz screens (should not cause problems for higher refresh rates)
        }
    }
}
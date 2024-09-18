using System;

namespace UnityEngine.SDGraph
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class SDGraphRenderHelper : MonoBehaviour
    {
        public enum SDGraphRenderType
        {
            None,
            Color,
            Depth,
            Normal,
            InPaint,
            WireFrame
        }
        
        private static SDGraphRenderHelper _instance;
        public static SDGraphRenderHelper Get() => _instance;

        public SDGraphRenderType renderType;
        
        private void OnEnable()
        {
            _instance = this;
        }
    }
}
using System;
using UnityEditor;

namespace UnityEngine.SDGraph
{
    public class SDCaptureProxy : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, SDTextureHandle.IconsPath + "Eye.png", true);
        }
    }
}
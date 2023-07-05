using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UPMImporterCollections
{
    [InitializeOnLoad]
    public static class UPMImporterCollections
    {
        static UPMImporterCollections()
        {
            Install("com.unity.nuget.newtonsoft-json");
            Install("com.unity.editorcoroutines");
        }

        public static bool Install(string id)
        {
            var request = Client.Add(id);
            while (!request.IsCompleted) { };
            if (request.Error != null) Debug.LogError(request.Error.message);
            return request.Error == null;
        }
    }
}

using GraphProcessor;
using Newtonsoft.Json;

namespace UnityEngine.SDGraph
{
    public class SDExtensionNode : SDNode
    {
        [Input(name = "Extension")] public string extensionInput = "";
        [Output(name = "Extension")] public string extensionOut = "";
        protected override void Enable()
        {
            nodeWidth = 260;
            base.Enable();
        }

        public virtual string header => name;
        public virtual object args => null;
        public override void Process()
        {
            base.Process();
            extensionOut = GetExtension(header, args, extensionInput);
        }
        public static string GetExtension(string header, object args, string extensionInput = null)
        {
            var json = JsonConvert.SerializeObject(args);
            return !string.IsNullOrEmpty(extensionInput) ? 
                $"{extensionInput},\"{header}\":{{\"args\":{json}}}" : 
                $"\"{header}\":{{\"args\":{json}}}";
        }
    }
}
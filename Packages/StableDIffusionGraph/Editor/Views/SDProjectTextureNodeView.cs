using GraphProcessor;
using UnityEngine.SDGraph;
using UnityEngine.UIElements;

namespace UnityEditor.SDGraph
{
    [NodeCustomEditor(typeof(SDProjectTextureNode))]
    public class SDProjectTextureNodeView: SDNodeView
    {
        
        private SDProjectTextureNode node;
        private Button projectBtn;

        public override void Enable()
        {
            base.Enable();
            node = nodeTarget as SDProjectTextureNode;
            if(node == null) return;
            
            projectBtn = new Button(ProjectImage);
            projectBtn.text = "Projector";
            extensionContainer.Add(projectBtn);
            RefreshExpandedState();  
        }

        private void ProjectImage()
        {
            node.Projector();
        }
    }
}
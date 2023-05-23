using System;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace FernNPRCore.SDNodeGraph
{
    // Undo.RecordObject(cur_register.RegisterData, tag);
    [Serializable]
    public struct PromptData
    {
        public string       word;
        public float        weight;
        public bool         end;
        public float        process;

        public int          color;
        public PromptData SetWeight(float weight)
        {
            this.weight = weight;
            return this;
        }
        public PromptData SetColor(int color)
        {
            this.color = color;
            return this;
        }
        public PromptData SetProcess(float process)
        {
            this.process = process;
            return this;
        }
        public PromptData SetProcessType(bool end)
        {
            this.end = end;
            return this;
        }
    }

    [Serializable]
    public class PromptRegisterData
    {
        public GUIContent title = new GUIContent();
        public List<PromptData> positiveDatas = new List<PromptData>();
        public List<PromptData> negativeDatas = new List<PromptData>();

        public void CopyTo(PromptRegisterData data)
        {
            data.title = new GUIContent(title);
            data.positiveDatas = new List<PromptData>(positiveDatas);
            data.negativeDatas = new List<PromptData>(negativeDatas);
        }
    }
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Prompt Register")]
    public class SDPromptRegisterNode : SDNode
    {
        [HideInInspector] public PromptRegisterData RegisterData = new PromptRegisterData();
        [HideInInspector] public Prompt Prompt = new Prompt();
        [Output] public string Positive;
        [Output] public string Negative;

        public override string name => "SD Prompt Register";        
        
        protected override void Process()
        {
            Positive = Prompt.positive;
            Negative = Prompt.negative;
        }
    }
}
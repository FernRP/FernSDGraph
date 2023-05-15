using System;
using System.Collections.Generic;
using FernGraph;
using UnityEngine;

namespace FernNPRCore.StableDiffusionGraph
{
    [Serializable]
    public struct PromptData
    {
        public string word;
        public float  weight;
        public int    color;

        public void SetData(string word, float weight, int color)
        {
            this.word = word;
            this.weight = weight;
            this.color = color;
        }
        public void SetData(string word)
        {
            this.word = word;
        }
        public void SetData(float weight)
        {
            this.weight = weight;
        }
        public void SetData(int color)
        {
            this.color = color;
        }
    }
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class SDPromptRegisterNode : Node
    {
        public List<PromptData> positiveDatas = new List<PromptData>();
        public List<PromptData> negativeDatas = new List<PromptData>();
        
        public Prompt Prompt = new Prompt();
        [Output] public string Positive;
        [Output] public string Negative;

        public override object OnRequestValue(Port port) => port.Name switch
        {
            "Positive" => Prompt.positive,
            "Negative" => Prompt.negative,
            _ => null
        };
    }
}
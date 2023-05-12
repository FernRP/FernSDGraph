using System;
using System.Collections.Generic;
using FernGraph;
using UnityEngine;

namespace FernNPRCore.StableDiffusionGraph
{
    [Node(Path = "SD Standard")]
    [Tags("SD Node")]
    public class PromptRegister : Node
    {
        public List<string> PositiveWords = new List<string>();
        public List<float> PositiveWordsWeights = new List<float>();
        public List<int> PositiveWordsColors = new List<int>();
        public List<string> NegativeWords = new List<string>();
        public List<float> NegativeWordsWeights = new List<float>();
        public List<int> NegativeWordsColors = new List<int>();
        [Output] public Prompt Prompt = new Prompt();
        public override object OnRequestValue(Port port) => port.Name switch
        {
            "Prompt" => Prompt,
            _ => null
        };
    }
}
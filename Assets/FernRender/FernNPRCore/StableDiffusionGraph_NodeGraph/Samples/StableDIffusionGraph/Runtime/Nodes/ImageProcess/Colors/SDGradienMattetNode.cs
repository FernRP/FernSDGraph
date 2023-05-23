using System;
using System.Collections;
using System.Collections.Generic;
using GraphProcessor;
using UnityEngine;

namespace FernNPRCore.SDNodeGraph
{
    [System.Serializable, NodeMenuItem("Stable Diffusion Graph/SD Gradient HDR")]
    public class SDGradienMattetNode : SDShaderNode
    {

        
        public enum GradientMode
        {
            Linear = 0, 
            Exponential,
            Radial,
            Circular,
            Square,
            Spiral,
        }
        
        public enum DirectionMode
        {
            Up = 0, 
            Down,
            Right,
            Left,
            Forward,
            Back,
        }
        
        public GradientMode gradientMode = GradientMode.Linear;
        [VisibleIf(nameof(gradientMode), GradientMode.Linear | GradientMode.Exponential)]
        public DirectionMode directionMode = DirectionMode.Up;
        [VisibleIf(nameof(gradientMode), GradientMode.Spiral)]
        public float spiralTurnCount = 1;
        [VisibleIf(nameof(gradientMode), GradientMode.Spiral)]
        public int spiralBranchCount = 1;
        
        [ColorUsage(true, true)]
        public Color color1 = Color.clear;
        [ColorUsage(true, true)]
        public Color color2 = Color.white;
        [Range(0,1)]
        public float falloff = 1;
        
        
        public override string name => "SD Gradient HDR";

        public override string shaderName => "Hidden/Mixture/GradientMatte";

        protected override void Process()
        {
            base.Process();
            BeforeProcessSetup();
            material.SetColor("_Color1", color1);
            material.SetColor("_Color2", color2);
            material.SetFloat("_Direction", (float)directionMode);
            material.SetFloat("_Mode", (float)gradientMode);
            material.SetFloat("_Falloff", falloff);
            material.SetFloat("_SpiralTurnCount", spiralTurnCount);
            material.SetFloat("_SpiralBranchCount", spiralBranchCount);
            output.Update();
        }
    }
}
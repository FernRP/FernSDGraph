using System;
using GraphProcessor;
using UnityEngine;

namespace UnityEngine.SDGraph
{
    [Serializable, NodeMenuItem("Stable Diffusion Graph/Ultimate SD Upscale")]
    public class UltimateSDUpscale : SDNode
    {
        [Output("Script")] public SCRIPT script;

        public UltimateSDUpscale()
        {
            script = new SCRIPT();
        }

        public static string[] target_size_types =
        {
            "From img2img2 settings",
            "Custom size",
            "Scale from image size"
        };

        public static string[] seams_fix_types =
        {
            "None",
            "Band pass",
            "Half tile offset pass",
            "Half tile offset pass + intersections"
        };

        public static string[] redrow_modes =
        {
            "Linear",
            "Chess",
            "None"
        };
        [HideInInspector, SerializeField]public int target_size_type = 0;
        [HideInInspector, SerializeField, Range(64, 8192)]public int custom_width = 2048;
        [HideInInspector, SerializeField, Range(64, 8192)]public int custom_height = 2048;
        [HideInInspector, SerializeField, Range(0, 16)]public float custom_scale = 2;

        [HideInInspector, SerializeField] public int upscaler_index;
        [HideInInspector, SerializeField]public int redraw_mode = 0;
        [HideInInspector, SerializeField, Range(0, 2048)]public int tile_width = 512;
        [HideInInspector, SerializeField, Range(0, 2048)]public int tile_height = 0;
        [HideInInspector, SerializeField, Range(0, 64)]public int mask_blur = 8;
        [HideInInspector, SerializeField, Range(0, 128)]public int padding = 32;
        [HideInInspector, SerializeField]public int seams_fix_type = 0;
        [HideInInspector, SerializeField, Range(0, 1)]public float seams_fix_denoise = 0.35f;
        [HideInInspector, SerializeField, Range(0, 128)]public int seams_fix_width = 64;
        [HideInInspector, SerializeField, Range(0, 128)]public int seams_fix_mask_blur = 8;
        [HideInInspector, SerializeField, Range(0, 128)]public int seams_fix_padding = 32;
        [HideInInspector, SerializeField]public bool save_upscaled_image = true;
        [HideInInspector, SerializeField]public bool save_seams_fix_image = false;
        
        
        public override string name => "Ultimate SD Upscale";

        public override void Process()
        {
            script.name = name;
            script.args = new object[]
            {
                //64 0 8 32 64 0.35 32 0 True 0 False 8 0 0 2048 2048 2
                "", tile_width, tile_height, mask_blur, padding, seams_fix_width, seams_fix_denoise,
                seams_fix_padding,
                upscaler_index, save_upscaled_image, redraw_mode, save_seams_fix_image, seams_fix_mask_blur,
                seams_fix_type, target_size_type, custom_width, custom_height, custom_scale
            };
    }
    }
} 
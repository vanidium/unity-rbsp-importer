using System.Collections.Generic;
using UnityEngine;

namespace BSPFormat {
    public static class Constants {
        /// <summary>
        /// Magic string for RBSP files. The first 4 bytes of the header should be this.
        /// </summary>
        public static string BSPMagicRaven = "RBSP";

        /// <summary>
        /// Lightmap count; for RBSP, this is 4.
        /// </summary>
        public static int MaxLightmaps = 4;

        public static int LumpEntities	= 0;
        public static int LumpTextures = 1;
        public static int LumpPlanes = 2;
        public static int LumpNodes = 3;
        public static int LumpLeafs = 4;
        public static int LumpLeafFaces = 5;
        public static int LumpLeafBrushes = 6;
        public static int LumpModels = 7;
        public static int LumpBrushes = 8;
        public static int LumpBrushSides = 9;
        public static int LumpVertexes = 10;
        public static int LumpMeshVerts = 11;
        public static int LumpEffects = 12;
        public static int LumpFaces = 13;
        public static int LumpLightmaps = 14;
        public static int LumpLightVols = 15;
        public static int LumpVisData = 16;

        /// <summary>
        /// Mapping between blend mode strings (from .shaders) and Rendering.BlendMode
        /// </summary>
        public static Dictionary<string, UnityEngine.Rendering.BlendMode> BlendModeMap = new Dictionary<string, UnityEngine.Rendering.BlendMode> {
            { "GL_ZERO",  UnityEngine.Rendering.BlendMode.Zero },
            { "GL_ONE",  UnityEngine.Rendering.BlendMode.One },
            { "GL_DST_COLOR",  UnityEngine.Rendering.BlendMode.DstColor },
            { "GL_SRC_COLOR",  UnityEngine.Rendering.BlendMode.SrcColor },
            { "GL_ONE_MINUS_DST_COLOR",  UnityEngine.Rendering.BlendMode.OneMinusDstColor },
            { "GL_SRC_ALPHA",  UnityEngine.Rendering.BlendMode.SrcAlpha },
            { "GL_ONE_MINUS_SRC_COLOR",  UnityEngine.Rendering.BlendMode.OneMinusSrcColor },
            { "GL_DST_ALPHA",  UnityEngine.Rendering.BlendMode.DstAlpha },
            { "GL_ONE_MINUS_DST_ALPHA",  UnityEngine.Rendering.BlendMode.OneMinusDstAlpha },
            { "GL_SRC_ALPHA_SATURATE",  UnityEngine.Rendering.BlendMode.SrcAlphaSaturate },
            { "GL_ONE_MINUS_SRC_ALPHA",  UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha },
        };

        /// <summary>
        /// A scale that is used when converting between RBSP and Unity vertex positions
        /// </summary>
        public static Vector3 VertexScaleFactor = new Vector3(0.03f, 0.03f, 0.03f);
    };
}
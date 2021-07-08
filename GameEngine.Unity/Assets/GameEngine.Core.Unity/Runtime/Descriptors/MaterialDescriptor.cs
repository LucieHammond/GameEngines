﻿using UnityEngine;

namespace GameEngine.Core.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a material
    /// </summary>
    public struct MaterialDescriptor
    {
        /// <summary>
        /// The shader script used by the material
        /// </summary>
        public Shader Shader;

        /// <summary>
        /// The main texture of the material
        /// </summary>
        public Texture MainTexture;

        /// <summary>
        /// The main color of the material
        /// </summary>
        public Color MainColor;
    }
}
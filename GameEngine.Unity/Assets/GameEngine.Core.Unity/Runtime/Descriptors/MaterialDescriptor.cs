﻿using GameEngine.Core.Unity.System;
using UnityEngine;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a material
    /// </summary>
    [CreateAssetMenu(fileName = "NewMaterialDescriptor", menuName = "Content/Unity Objects/Material Descriptor", order = 2)]
    public class MaterialDescriptor : ContentDescriptor
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

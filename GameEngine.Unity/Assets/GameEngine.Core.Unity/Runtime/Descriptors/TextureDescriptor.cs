using GameEngine.Core.Unity.System;
using UnityEngine;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a 2D texture
    /// </summary>
    [CreateAssetMenu(fileName = "NewTextureDescriptor", menuName = "Content/Unity Objects/Texture Descriptor", order = 1)]
    public class TextureDescriptor : ContentDescriptor
    {
        /// <summary>
        /// The width of the texture in pixels
        /// </summary>
        public int Width;

        /// <summary>
        /// The height of the texture in pixels
        /// </summary>
        public int Height;

        /// <summary>
        /// The format in which the texture data is encoded
        /// </summary>
        public TextureFormat Format;

        /// <summary>
        /// Enable mipmapping on the texture
        /// </summary>
        public bool UseMipMap;

        /// <summary>
        /// If the texture is encoded in linear or sRGB color space
        /// </summary>
        public bool Linear;

        /// <summary>
        /// Filtering mode of the texture
        /// </summary>
        public FilterMode FilterMode;

        /// <summary>
        /// Wrap mode of the texture
        /// </summary>
        public TextureWrapMode WrapMode;

        /// <summary>
        /// The anisotropic filtering level of the texture
        /// </summary>
        public int AnisoLevel;
    }
}

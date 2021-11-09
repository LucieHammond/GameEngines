using GameEngine.Core.Pools;
using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Pools.Poolers;
using UnityEngine;

namespace GameEngine.Core.Unity.Pools.Managers
{
    /// <summary>
    /// A manager specialized in the processing of texture2D pools
    /// </summary>
    public class TexturePoolManager : PoolManager<Texture2D, TexturePooler, TextureDescriptor>
    {
        /// <summary>
        /// Instantiate a texture pooler configured with the given texture descriptor
        /// </summary>
        /// <param name="objectDescriptor">A descriptor characterizing the textures to pool with this pooler</param>
        /// <returns>The created texture pooler</returns>
        protected override TexturePooler CreateObjectPooler(TextureDescriptor objectDescriptor)
        {
            return new TexturePooler(objectDescriptor);
        }

        /// <summary>
        /// Get a texture from the specified pool and load a given image into it
        /// </summary>
        /// <param name="poolId">The id of the pool</param>
        /// <param name="imageData">The byte array containing the image data to load</param>
        /// <param name="isRawData">
        /// True if the raw image data have been preformatted for texture.
        /// False (default) if the image data should be decoded from a commonly used format (png, jpeg, tga, tif)
        /// </param>
        /// <returns>The reserved texture with the image loaded</returns>
        public Texture2D LoadImageOnPooledTexture(string poolId, byte[] imageData, bool isRawData = false)
        {
            Texture2D texture = GetObjectFromPool(poolId);
            if (!isRawData)
                texture.LoadImage(imageData);
            else
                texture.LoadRawTextureData(imageData);

            texture.Apply(false, false);
            return texture;
        }
    }
}
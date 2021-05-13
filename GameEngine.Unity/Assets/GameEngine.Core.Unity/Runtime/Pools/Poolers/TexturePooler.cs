using GameEngine.Core.Pools.Descriptors;
using UnityEngine;

namespace GameEngine.Core.Pools.Poolers
{
    /// <summary>
    /// A pooling helper defining how to pool Unity 2D textures
    /// </summary>
    public class TexturePooler : IObjectPooler<Texture2D>
    {
        private TextureDescriptor m_TextureDescriptor;

        /// <summary>
        /// Initialize a new instance of TexturePooler
        /// </summary>
        /// <param name="descriptor">The descriptor containing configuration information for the pooled textures</param>
        public TexturePooler(TextureDescriptor descriptor)
        {
            m_TextureDescriptor = descriptor;
        }

        /// <summary>
        /// <see cref="IObjectPooler.CreateObject()"/>
        /// </summary>
        public Texture2D CreateObject()
        {
            Texture2D texture = new Texture2D(
                m_TextureDescriptor.Width,
                m_TextureDescriptor.Height,
                m_TextureDescriptor.Format,
                m_TextureDescriptor.UseMipMap,
                m_TextureDescriptor.Linear);

            texture.filterMode = m_TextureDescriptor.FilterMode;
            texture.wrapMode = m_TextureDescriptor.WrapMode;
            texture.anisoLevel = m_TextureDescriptor.AnisoLevel;

            return texture;
        }

        /// <summary>
        /// <see cref="IObjectPooler.PrepareObject(T)"/>
        /// </summary>
        public void PrepareObject(Texture2D pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.RestoreObject(T)"/>
        /// </summary>
        public void RestoreObject(Texture2D pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.DestroyObject(T)"/>
        /// </summary>
        public void DestroyObject(Texture2D pooledObject)
        {
            Object.Destroy(pooledObject);
        }
    }
}

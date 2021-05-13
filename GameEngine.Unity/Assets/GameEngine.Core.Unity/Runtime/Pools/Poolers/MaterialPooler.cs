using GameEngine.Core.Pools.Descriptors;
using UnityEngine;

namespace GameEngine.Core.Pools.Poolers
{
    /// <summary>
    /// A pooling helper defining how to pool Unity materials
    /// </summary>
    public class MaterialPooler : IObjectPooler<Material>
    {
        private MaterialDescriptor m_MaterialDescriptor;

        /// <summary>
        /// Initialize a new instance of MaterialPooler
        /// </summary>
        /// <param name="descriptor">The descriptor containing configuration information for the pooled materials</param>
        public MaterialPooler(MaterialDescriptor descriptor)
        {
            m_MaterialDescriptor = descriptor;
        }

        /// <summary>
        /// <see cref="IObjectPooler.CreateObject()"/>
        /// </summary>
        public Material CreateObject()
        {
            Material material = new Material(m_MaterialDescriptor.Shader);
            material.mainTexture = m_MaterialDescriptor.MainTexture;
            material.color = m_MaterialDescriptor.MainColor;

            return material;
        }

        /// <summary>
        /// <see cref="IObjectPooler.PrepareObject(T)"/>
        /// </summary>
        public void PrepareObject(Material pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.RestoreObject(T)"/>
        /// </summary>
        public void RestoreObject(Material pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.DestroyObject(T)"/>
        /// </summary>
        public void DestroyObject(Material pooledObject)
        {
            Object.Destroy(pooledObject);
        }
    }
}

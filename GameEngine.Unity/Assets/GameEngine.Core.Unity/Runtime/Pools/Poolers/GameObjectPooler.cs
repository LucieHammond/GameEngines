using GameEngine.Core.Descriptors;
using UnityEngine;

namespace GameEngine.Core.Pools.Poolers
{
    /// <summary>
    /// A pooling helper defining how to pool Unity gameobjects
    /// </summary>
    public class GameObjectPooler : IObjectPooler<GameObject>
    {
        private readonly GameObject m_ReferencePrefab;
        private readonly string m_ParentName;
        private int m_ObjectCount;

        /// <summary>
        /// Initialize a new instance of GameObjectPooler
        /// </summary>
        /// <param name="descriptor">The descriptor containing configuration information for the pooled gameobjects</param>
        public GameObjectPooler(GameObjectDescriptor descriptor)
        {
            m_ReferencePrefab = descriptor.ReferencePrefab;
            m_ParentName = descriptor.ParentName;
            m_ObjectCount = 0;
        }

        /// <summary>
        /// <see cref="IObjectPooler.CreateObject()"/>
        /// </summary>
        public GameObject CreateObject()
        {
            m_ObjectCount++;
            GameObject parent = GameObject.Find(m_ParentName) ?? new GameObject(m_ParentName);
            GameObject gameObject = Object.Instantiate(m_ReferencePrefab, parent.transform);
            gameObject.gameObject.name = $"{m_ReferencePrefab.name}_{m_ObjectCount}";
            gameObject.SetActive(false);
            return gameObject;
        }

        /// <summary>
        /// <see cref="IObjectPooler.PrepareObject(T)"/>
        /// </summary>
        public void PrepareObject(GameObject pooledObject)
        {
            pooledObject.SetActive(true);
        }

        /// <summary>
        /// <see cref="IObjectPooler.RestoreObject(T)"/>
        /// </summary>
        public void RestoreObject(GameObject pooledObject)
        {
            pooledObject.SetActive(false);
        }

        /// <summary>
        /// <see cref="IObjectPooler.DestroyObject(T)"/>
        /// </summary>
        public void DestroyObject(GameObject pooledObject)
        {
            Object.Destroy(pooledObject);
        }
    }
}

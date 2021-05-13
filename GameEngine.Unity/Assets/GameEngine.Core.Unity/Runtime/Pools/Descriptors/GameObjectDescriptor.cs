using UnityEngine;

namespace GameEngine.Core.Pools.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a gameobject in a pool
    /// </summary>
    public struct GameObjectDescriptor
    {
        /// <summary>
        /// The prefab that serves as a reference to create the pooled gameobject
        /// </summary>
        public GameObject ReferencePrefab;

        /// <summary>
        /// The parent transform under which to create the pooled gameobject in the scene hierarchy
        /// </summary>
        public Transform Parent;
    }
}

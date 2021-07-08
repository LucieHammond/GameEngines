using UnityEngine;

namespace GameEngine.Core.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a gameobject
    /// </summary>
    public struct GameObjectDescriptor
    {
        /// <summary>
        /// The prefab that serves as a reference to create the gameobject
        /// </summary>
        public GameObject ReferencePrefab;

        /// <summary>
        /// The parent transform under which to create the gameobject in the scene hierarchy
        /// </summary>
        public Transform Parent;
    }
}

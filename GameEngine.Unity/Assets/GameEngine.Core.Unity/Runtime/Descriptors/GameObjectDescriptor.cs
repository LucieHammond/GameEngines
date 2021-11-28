using GameEngine.Core.Unity.System;
using UnityEngine;

namespace GameEngine.Core.Unity.Descriptors
{
    /// <summary>
    /// A descriptor containing the information needed to configure a gameobject
    /// </summary>
    [CreateAssetMenu(fileName = "NewGameObjectDescriptor", menuName = "Content/Unity Objects/GameObject Descriptor", order = 3)]
    public class GameObjectDescriptor : ContentDescriptor
    {
        /// <summary>
        /// The prefab that serves as a reference to create the gameobject
        /// </summary>
        public GameObject ReferencePrefab;

        /// <summary>
        /// The name of the parent object under which to create the gameobject in the scene hierarchy
        /// </summary>
        public string ParentName;
    }
}

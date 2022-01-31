using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.Core.Unity.System
{
    /// <summary>
    /// A scriptable object describing a collection of content descriptors
    /// </summary>
    [CreateAssetMenu(fileName = "NewCollection", menuName = "Content/Collection/Collection Descriptor", order = 0)]
    public class CollectionDescriptor : ScriptableObject
    {
        /// <summary>
        /// The list of the descriptors in the collection, referenced by id
        /// </summary>
        public List<string> Collection;

        /// <summary>
        /// Returns a string representing the collection descriptor
        /// </summary>
        /// <returns>A string representing the collection descriptor</returns>
        public override string ToString()
        {
            return $"Collection -> [{string.Join(",", Collection)}]";
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace GameEngine.PMR.Unity.Basics.Content
{
    /// <summary>
    /// A scriptable object describing a collection of content descriptors
    /// </summary>
    public class CollectionDescriptor : ScriptableObject
    {
        /// <summary>
        /// Id of the collection
        /// </summary>
        public string CollectionId;

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
            return $"Collection {CollectionId} -> [{string.Join(",", Collection)}]";
        }
    }
}

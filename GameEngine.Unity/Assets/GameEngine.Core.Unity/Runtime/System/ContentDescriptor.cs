using System.Reflection;
using UnityEngine;

namespace GameEngine.Core.Unity.System
{
    /// <summary>
    /// Base model to be derived when defining a certain type of game content
    /// This content takes the form of descriptive scriptable objects stored as assets
    /// </summary>
    public abstract class ContentDescriptor : ScriptableObject
    {
        /// <summary>
        /// Id of the content descriptor, by which it is referenced
        /// </summary>
        [HideInInspector]
        public string ContentId;

        /// <summary>
        /// Returns a string representing the content descriptor
        /// </summary>
        /// <returns>A string representing the content descriptor</returns>
        public override string ToString()
        {
            string description = "";
            foreach (FieldInfo field in GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (description.Length > 0)
                    description += ", ";
                description += $"{field.Name}: {field.GetValue(this)}";
            }

            return $"{ContentId} ({GetType().Name}) -> {{{description}}}";
        }
    }
}

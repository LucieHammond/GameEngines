using UnityEngine;

namespace GameEngine.Core.Unity.System
{
    /// <summary>
    /// A ScriptableObject model allowing the creation and definition of a specific configuration in Unity
    /// </summary>
    /// <typeparam name="TConfig">The type of the underlying configuration</typeparam>
    public abstract class ScriptableSettings<TConfig> : ScriptableObject
    {
        /// <summary>
        /// The configuration to be filled
        /// </summary>
        public TConfig Configuration;

        /// <summary>
        /// A method to check if the actual configuration is valid
        /// </summary>
        /// <returns>True if the configuration is valid, else False</returns>
        public abstract bool Validate();
    }
}

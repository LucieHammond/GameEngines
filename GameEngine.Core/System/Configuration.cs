using System.Collections.Generic;
using System.Linq;

namespace GameEngine.Core.System
{
    /// <summary>
    /// A type representing an in-memory configuration object, that can be used like a dictionary with string keys
    /// </summary>
    public class Configuration : Dictionary<string, object>
    {
        /// <summary>
        /// Get the configuration value for given key, correctly casted with type T
        /// </summary>
        /// <typeparam name="T">The type of the expected object to retrieve</typeparam>
        /// <param name="key">The key to use for retrieving object</param>
        /// <returns>The value associated with the key, and null if the key was not found</returns>
        public T Get<T>(string key)
        {
            if (this.ContainsKey(key))
                return (T)this[key];

            return default;
        }

        /// <summary>
        /// Determines whether the specified configuration is equal to the current configuration
        /// </summary>
        /// <param name="obj">The configuration object to compare with the current configuration</param>
        /// <returns>If the specified configuration is equal to the current configuration</returns>
        public override bool Equals(object obj)
        {
            if (obj is Configuration config)
                return this.OrderBy(pair => pair.Key).SequenceEqual(config.OrderBy(pair => pair.Key));

            return false;
        }

        /// <summary>
        /// Default Hash function
        /// </summary>
        /// <returns>The hash code for the current configuration instance</returns>
        public override int GetHashCode()
        {
            int hashCode = -162625756;
            hashCode = hashCode * -1521134295 + EqualityComparer<KeyCollection>.Default.GetHashCode(Keys);
            hashCode = hashCode * -1521134295 + EqualityComparer<ValueCollection>.Default.GetHashCode(Values);
            return hashCode;
        }
    }
}

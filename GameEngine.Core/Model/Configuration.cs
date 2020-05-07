using System.Collections.Generic;

namespace GameEngine.Core.Model
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
    }
}

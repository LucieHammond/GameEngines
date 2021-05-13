namespace GameEngine.Core.Pools
{
    /// <summary>
    /// A pooling helper defining how to handle T objects at every step of the pooling process (creation, preparation, release, destruction)
    /// </summary>
    /// <typeparam name="T">The type of the objects to pool</typeparam>
    public interface IObjectPooler<T>
    {
        /// <summary>
        /// Create an object of type T so that it can be added to the pool
        /// </summary>
        /// <returns>The created object</returns>
        T CreateObject();

        /// <summary>
        /// Prepare an object of type T so that it can be taken and used
        /// </summary>
        /// <param name="pooledObject">The pooled object to prepare</param>
        void PrepareObject(T pooledObject);

        /// <summary>
        /// Restore an object of type T so that it can be released
        /// </summary>
        /// <param name="pooledObject">The pooled object to restore</param>
        void RestoreObject(T pooledObject);

        /// <summary>
        /// Destroy the given object of type T so that the pool can be cleared
        /// </summary>
        /// <param name="pooledObject">The pooled object to destroy</param>
        void DestroyObject(T pooledObject);
    }
}

namespace GameEngine.Core.Pools
{
    /// <summary>
    /// A descriptor containing the information needed to configure a pool of objects
    /// </summary>
    /// <typeparam name="TDescriptor">The type of the pooled object descriptor</typeparam>
    public struct PoolDescriptor<TDescriptor>
    {
        /// <summary>
        /// The pool id
        /// </summary>
        public string PoolId;

        /// <summary>
        /// The initial size of the pool
        /// </summary>
        public int InitialSize;

        /// <summary>
        /// If the pool can be extended in case of need (if there is no more objects available)
        /// </summary>
        public bool IsExtensible;

        /// <summary>
        /// A customizable object descriptor containing information for configuring the object being pooled
        /// </summary>
        public TDescriptor ObjectDescriptor;
    }
}

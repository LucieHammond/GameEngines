using System;
using System.Collections.Generic;

namespace GameEngine.Core.Pools
{
    /// <summary>
    /// A manager specialized in handling pools of the same type of objects, using the same type of pooler configured with the same type of descriptor
    /// </summary>
    /// <typeparam name="T">The type of the pooled objects</typeparam>
    /// <typeparam name="TPooler">The type of pooler to use for handling these pooled objects</typeparam>
    /// <typeparam name="TDescriptor">The type of the descriptor to use for configuring the pooler of these pooled objects</typeparam>
    public abstract class PoolManager<T, TPooler, TDescriptor> where TPooler : IObjectPooler<T>
    {
        private readonly Dictionary<string, Pool<T>> m_Pools;

        /// <summary>
        /// Initialize an new instance of PoolManager
        /// </summary>
        public PoolManager()
        {
            m_Pools = new Dictionary<string, Pool<T>>();
        }

        /// <summary>
        /// Initialize the pool manager with the requested pools
        /// </summary>
        /// <param name="requestedPools">A list of pool descriptors characterizing the pools to create</param>
        public void Initialize(List<PoolDescriptor<TDescriptor>> requestedPools)
        {
            foreach (PoolDescriptor<TDescriptor> poolDescriptor in requestedPools)
            {
                CreatePool(poolDescriptor);
            }
        }

        /// <summary>
        /// Indicate if the pool manager contains the specified pool
        /// </summary>
        /// <param name="poolId">The id of the pool</param>
        /// <returns>If the pool was found</returns>
        public bool ContainPool(string poolId)
        {
            return m_Pools.ContainsKey(poolId);
        }

        /// <summary>
        /// Create and initialize a pool of T objects configured according to the given descriptor
        /// </summary>
        /// <param name="poolDescriptor">The descriptor characterizing the pool</param>
        public void CreatePool(PoolDescriptor<TDescriptor> poolDescriptor)
        {
            if (m_Pools.ContainsKey(poolDescriptor.PoolId))
            {
                throw new ArgumentException($"A pool with same id ({poolDescriptor.PoolId}) already exists", nameof(poolDescriptor.PoolId));
            }

            TPooler objectPooler = CreateObjectPooler(poolDescriptor.ObjectDescriptor);
            Pool<T> pool = new Pool<T>(objectPooler, poolDescriptor.PoolId, poolDescriptor.InitialSize, poolDescriptor.IsExtensible);
            m_Pools.Add(poolDescriptor.PoolId, pool);
        }

        /// <summary>
        /// Get and reserve an object from the specified pool
        /// </summary>
        /// <param name="poolId">The id of the pool</param>
        /// <returns>The object that have been reserved</returns>
        public T GetObjectFromPool(string poolId)
        {
            if (!m_Pools.ContainsKey(poolId))
            {
                throw new ArgumentException($"No pool with given id ({poolId}) could be found", nameof(poolId));
            }

            return m_Pools[poolId].GetFreeObject();
        }

        /// <summary>
        /// Release an object to the specified pool after use
        /// </summary>
        /// <param name="poolId">The id of the pool</param>
        /// <param name="usedObject">The object to release</param>
        public void ReleaseObjectToPool(string poolId, T usedObject)
        {
            if (!m_Pools.ContainsKey(poolId))
            {
                throw new ArgumentException($"No pool with given id ({poolId}) could be found", nameof(poolId));
            }

            m_Pools[poolId].ReleaseUsedObject(usedObject);
        }

        /// <summary>
        /// Destroy a specified pool and all the objects it contained
        /// </summary>
        /// <param name="poolId">The id of the pool</param>
        public void DestroyPool(string poolId)
        {
            if (!m_Pools.ContainsKey(poolId))
            {
                throw new ArgumentException($"No pool with given id ({poolId}) could be found", nameof(poolId));
            }

            m_Pools[poolId].ClearPool();
            m_Pools.Remove(poolId);
        }

        /// <summary>
        /// Reset all pools handled by the manager
        /// </summary>
        public void ResetPools()
        {
            foreach (Pool<T> pool in m_Pools.Values)
            {
                pool.ResetPool();
            }
        }

        /// <summary>
        /// Clear and remove all pools handled by the manager
        /// </summary>
        public void Clear()
        {
            foreach (Pool<T> pool in m_Pools.Values)
            {
                pool.ClearPool();
            }

            m_Pools.Clear();
        }

        /// <summary>
        /// Instantiate a pooler of type TPooler configured with the given descriptor of type TDescriptor
        /// </summary>
        /// <param name="objectDescriptor">A descriptor characterizing the objects to pool with this pooler</param>
        /// <returns>The created pooler</returns>
        protected abstract TPooler CreateObjectPooler(TDescriptor objectDescriptor);
    }
}

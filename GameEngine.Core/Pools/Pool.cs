using GameEngine.Core.Logger;
using System.Collections.Generic;

namespace GameEngine.Core.Pools
{
    /// <summary>
    /// A generic pool regrouping pre-instantiated T objects that can be used and released on demand
    /// </summary>
    /// <typeparam name="T">The type of the pooled objects</typeparam>
    public class Pool<T>
    {
        private const string TAG = "Pool";

        /// <summary>
        /// The id of the pool
        /// </summary>
        public string PoolId { get; }

        /// <summary>
        /// The current size of the pool
        /// </summary>
        public int PoolSize => m_ObjectPool.Count;

        private readonly IObjectPooler<T> m_ObjectPooler;
        private readonly List<T> m_ObjectPool;
        private readonly Dictionary<T, int> m_ObjectIdsTable;
        private readonly Stack<int> m_FreeObjectIds;
        private readonly int m_InitialSize;
        private readonly bool m_IsExtensible;

        /// <summary>
        /// Create a new instance of Pool and initialize it
        /// </summary>
        /// <param name="pooler">The pooler that defines how to prepare and process the pooled objects</param>
        /// <param name="poolId">The pool id</param>
        /// <param name="initialSize">The initial size of the pool</param>
        /// <param name="isExtensible">If the pool can be extended in case of need (default is true)</param>
        public Pool(IObjectPooler<T> pooler, string poolId, int initialSize, bool isExtensible = true)
        {
            PoolId = poolId;
            m_ObjectPooler = pooler;
            m_InitialSize = initialSize;
            m_IsExtensible = isExtensible;

            m_ObjectPool = new List<T>();
            m_ObjectIdsTable = new Dictionary<T, int>();
            m_FreeObjectIds = new Stack<int>();

            Log.Info(TAG, $"Initialize new pool {PoolId} with {m_InitialSize} instances of {typeof(T).Name}");

            InitializePool();
        }

        /// <summary>
        /// Destroy the current instance of Pool
        /// </summary>
        ~Pool()
        {
            if (PoolSize > 0)
                EmptyPool();
        }

        /// <summary>
        /// Get and reserve an object from the pool among those that are available
        /// </summary>
        /// <returns>The object that has been reserved</returns>
        public T GetFreeObject()
        {
            if (m_FreeObjectIds.Count == 0)
            {
                if (!m_IsExtensible)
                {
                    Log.Error(TAG, $"Cannot retrieve new {typeof(T).Name} from inextensible pool {PoolId} because none is available.\n" +
                        $"Consider increasing the initial size of the pool (initial size: {m_InitialSize})");
                    return default(T);
                }
                else
                {
                    Log.Warning(TAG, $"Instanciating new {typeof(T).Name} in pool {PoolId} at runtime.\n" +
                        $"Consider increasing the initial size of the pool (initial size: {m_InitialSize}, current size: {PoolSize})");

                    int index = PoolSize;
                    T newObject = m_ObjectPooler.CreateObject();
                    m_ObjectPool.Add(newObject);
                    m_ObjectIdsTable.Add(newObject, index);
                    m_FreeObjectIds.Push(index);
                }
            }

            T pooledObject = m_ObjectPool[m_FreeObjectIds.Pop()];
            m_ObjectPooler.PrepareObject(pooledObject);
            return pooledObject;
        }

        /// <summary>
        /// Release an object to the pool after use
        /// </summary>
        /// <param name="pooledObject">The object to release</param>
        public void ReleaseUsedObject(T pooledObject)
        {
            if (!m_ObjectIdsTable.TryGetValue(pooledObject, out int index))
            {
                Log.Error(TAG, $"Cannot release the given {pooledObject.GetType().Name} because it doesn't belong to the pool {PoolId}");
                return;
            }

            m_ObjectPooler.RestoreObject(m_ObjectPool[index]);
            m_FreeObjectIds.Push(index);
        }

        /// <summary>
        /// Clear the pool by destroying all its objects
        /// </summary>
        public void ClearPool()
        {
            Log.Info(TAG, $"Clear pool {PoolId}");

            EmptyPool();
        }

        /// <summary>
        /// Reinitialize the pool by removing and recreating all pooled objects
        /// </summary>
        public void ResetPool()
        {
            Log.Info(TAG, $"Reset pool {PoolId} with {m_InitialSize} instances of {typeof(T).Name}");

            if (PoolSize > 0)
                EmptyPool();

            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < m_InitialSize; i++)
            {
                T pooledObject = m_ObjectPooler.CreateObject();
                m_ObjectPool.Add(pooledObject);
                m_ObjectIdsTable.Add(pooledObject, i);
                m_FreeObjectIds.Push(i);
            }
        }

        private void EmptyPool()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                m_ObjectPooler.DestroyObject(m_ObjectPool[i]);
            }

            m_ObjectPool.Clear();
            m_ObjectIdsTable.Clear();
            m_FreeObjectIds.Clear();
        }
    }
}

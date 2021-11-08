using GameEngine.Core.Descriptors;
using GameEngine.Core.Pools.Poolers;
using UnityEngine;

namespace GameEngine.Core.Pools.Managers
{
    /// <summary>
    /// A manager specialized in the processing of gameobjects pools
    /// </summary>
    public class GameObjectPoolManager : PoolManager<GameObject, GameObjectPooler, GameObjectDescriptor>
    {
        /// <summary>
        /// Instantiate a gameobject pooler configured with the given gameobject descriptor
        /// </summary>
        /// <param name="objectDescriptor">A descriptor characterizing the gameobjects to pool with this pooler</param>
        /// <returns>The created gameobject pooler</returns>
        protected override GameObjectPooler CreateObjectPooler(GameObjectDescriptor objectDescriptor)
        {
            return new GameObjectPooler(objectDescriptor);
        }

        /// <summary>
        /// Extract a particular component from a gameobject that is taken and reserved from a specified pool
        /// </summary>
        /// <typeparam name="TComponent">Tye type of the component to get</typeparam>
        /// <param name="poolId">The id of the pool</param>
        /// <returns>The component extracted from the reserved object</returns>
        public TComponent GetComponentFromPool<TComponent>(string poolId) where TComponent : Component
        {
            GameObject pooledObject = GetObjectFromPool(poolId);
            if (!pooledObject.TryGetComponent(out TComponent component))
            {
                throw new MissingComponentException($"Component {typeof(TComponent).Name} was not found on model prefab for pool {poolId}");
            }

            return component;
        }

        /// <summary>
        /// Release a component (and the gameobject it belongs to) to the specified pool after use
        /// </summary>
        /// <typeparam name="TComponent">The type of the component to release</typeparam>
        /// <param name="poolId">The id of the pool</param>
        /// <param name="component">The component to release</param>
        public void ReleaseComponentToPool<TComponent>(string poolId, TComponent component) where TComponent : Component
        {
            ReleaseObjectToPool(poolId, component.gameObject);
        }

    }
}

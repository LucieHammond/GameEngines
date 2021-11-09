using GameEngine.Core.Pools;
using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Pools.Poolers;
using UnityEngine;

namespace GameEngine.Core.Unity.Pools.Managers
{
    /// <summary>
    /// A manager specialized in the processing of material pools
    /// </summary>
    public class MaterialPoolManager : PoolManager<Material, MaterialPooler, MaterialDescriptor>
    {
        /// <summary>
        /// Instantiate a material pooler configured with the given material descriptor
        /// </summary>
        /// <param name="objectDescriptor">A descriptor characterizing the materials to pool with this pooler</param>
        /// <returns>The created material pooler</returns>
        protected override MaterialPooler CreateObjectPooler(MaterialDescriptor objectDescriptor)
        {
            return new MaterialPooler(objectDescriptor);
        }
    }
}
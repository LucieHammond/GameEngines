using GameEngine.Core.Pools.Descriptors;
using GameEngine.Core.Pools.Poolers;
using UnityEngine;

namespace GameEngine.Core.Pools.Managers
{
    /// <summary>
    /// A manager specialized in the processing of mesh pools
    /// </summary>
    public class MeshPoolManager : PoolManager<Mesh, MeshPooler, MeshDescriptor>
    {
        /// <summary>
        /// Instantiate a mesh pooler configured with the given mesh descriptor
        /// </summary>
        /// <param name="objectDescriptor">A descriptor characterizing the meshes to pool with this pooler</param>
        /// <returns>The created mesh pooler</returns>
        protected override MeshPooler CreateObjectPooler(MeshDescriptor objectDescriptor)
        {
            return new MeshPooler(objectDescriptor);
        }
    }
}
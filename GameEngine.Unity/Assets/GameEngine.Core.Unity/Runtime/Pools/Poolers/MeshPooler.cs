using GameEngine.Core.Pools.Descriptors;
using UnityEngine;

namespace GameEngine.Core.Pools.Poolers
{
    /// <summary>
    /// A pooling helper defining how to pool Unity meshes
    /// </summary>
    public class MeshPooler : IObjectPooler<Mesh>
    {
        private MeshDescriptor m_MeshDescriptor;

        /// <summary>
        /// Initialize a new instance of MeshPooler
        /// </summary>
        /// <param name="descriptor">The descriptor containing configuration information for the pooled meshes</param>
        public MeshPooler(MeshDescriptor descriptor)
        {
            m_MeshDescriptor = descriptor;
        }

        /// <summary>
        /// <see cref="IObjectPooler.CreateObject()"/>
        /// </summary>
        public Mesh CreateObject()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = m_MeshDescriptor.Vertices;
            mesh.triangles = m_MeshDescriptor.Triangles;
            mesh.uv = m_MeshDescriptor.UV;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            return mesh;
        }

        /// <summary>
        /// <see cref="IObjectPooler.PrepareObject(T)"/>
        /// </summary>
        public void PrepareObject(Mesh pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.RestoreObject(T)"/>
        /// </summary>
        public void RestoreObject(Mesh pooledObject)
        {

        }

        /// <summary>
        /// <see cref="IObjectPooler.DestroyObject(T)"/>
        /// </summary>
        public void DestroyObject(Mesh pooledObject)
        {
            pooledObject.Clear();
            Object.Destroy(pooledObject);
        }
    }
}

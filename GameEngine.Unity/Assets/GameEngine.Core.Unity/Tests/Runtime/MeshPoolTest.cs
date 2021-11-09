using GameEngine.Core.Pools;
using GameEngine.Core.Unity.Descriptors;
using GameEngine.Core.Unity.Pools.Managers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.Core.UnityTests
{
    /// <summary>
    /// Component tests for the MeshPoolManager class
    /// <see cref="MeshPoolManager"/>
    /// </summary>
    public class MeshPoolTest
    {
        private readonly MeshPoolManager m_PoolManager;
        private PoolDescriptor<MeshDescriptor> m_PoolDescriptor;

        public MeshPoolTest()
        {
            m_PoolManager = new MeshPoolManager();
        }

        [SetUp]
        public void Initialize()
        {
            m_PoolDescriptor = new PoolDescriptor<MeshDescriptor>()
            {
                PoolId = "mesh_test_pool",
                InitialSize = 10,
                IsExtensible = true,
                ObjectDescriptor = (MeshDescriptor)Resources.Load("TestDescriptors/mesh_descriptor")
            };
        }

        [TearDown]
        public void CleanUp()
        {
            m_PoolManager.Clear();
        }

        [UnityTest]
        public IEnumerator CreateAndDestroyMesh()
        {
            // Create pool and get pooled mesh
            m_PoolManager.CreatePool(m_PoolDescriptor);
            Mesh mesh = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The returned mesh is correctly configured
            Assert.IsTrue(mesh != null);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Vertices, mesh.vertices);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Triangles, mesh.triangles);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.UV, mesh.uv);
            for (int i = 0; i < mesh.vertexCount; i++)
                Assert.AreEqual(new Vector3(0, 1, 0), mesh.normals[i]);
            Assert.AreEqual(new Vector3(-10, 0, -10), mesh.bounds.min);
            Assert.AreEqual(new Vector3(10, 0, 10), mesh.bounds.max);

            // Release pooled mesh and destroy pool
            m_PoolManager.ReleaseObjectToPool(m_PoolDescriptor.PoolId, mesh);
            m_PoolManager.DestroyPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The previously used mesh is null in Unity
            Assert.IsTrue(mesh == null);
        }
    }
}

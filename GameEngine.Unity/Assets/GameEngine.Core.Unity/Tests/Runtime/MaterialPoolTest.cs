using GameEngine.Core.Pools;
using GameEngine.Core.Pools.Descriptors;
using GameEngine.Core.Pools.Managers;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace GameEngine.Core.Test
{
    /// <summary>
    /// Component tests for the MaterialPoolManager class
    /// <see cref="MaterialPoolManager"/>
    /// </summary>
    public class MaterialPoolTest
    {
        private readonly MaterialPoolManager m_PoolManager;
        private PoolDescriptor<MaterialDescriptor> m_PoolDescriptor;

        public MaterialPoolTest()
        {
            m_PoolManager = new MaterialPoolManager();
        }

        [SetUp]
        public void Initialize()
        {
            m_PoolDescriptor = new PoolDescriptor<MaterialDescriptor>()
            {
                PoolId = "material_test_pool",
                InitialSize = 10,
                IsExtensible = true,
                ObjectDescriptor = new MaterialDescriptor()
                {
                    Shader = Shader.Find("Standard"),
                    MainColor = Color.red
                }
            };
        }

        [TearDown]
        public void CleanUp()
        {
            m_PoolManager.Clear();
        }

        [UnityTest]
        public IEnumerator CreateAndDestroyMaterial()
        {
            // Create pool and get pooled material
            m_PoolManager.CreatePool(m_PoolDescriptor);
            Material material = m_PoolManager.GetObjectFromPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The returned material is correctly configured
            Assert.IsTrue(material != null);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.Shader, material.shader);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.MainTexture, material.mainTexture);
            Assert.AreEqual(m_PoolDescriptor.ObjectDescriptor.MainColor, material.color);

            // Release pooled material and destroy pool
            m_PoolManager.ReleaseObjectToPool(m_PoolDescriptor.PoolId, material);
            m_PoolManager.DestroyPool(m_PoolDescriptor.PoolId);
            yield return null;

            // The previously used material is null in Unity
            Assert.IsTrue(material == null);
        }
    }
}
